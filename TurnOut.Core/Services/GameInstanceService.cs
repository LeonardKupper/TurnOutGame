using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Turnout.Core.Utility;
using TurnOut.Core.Models;

namespace TurnOut.Core.Services
{
    /// <summary>
    /// Main Service responsible for creating and managing game state.
    /// </summary>
    public class GameInstanceService
    {
        public GameInstance GameInstance { get; set; }

        private readonly TurnPlanningService _turnPlanningService;
        private readonly GameWorldService _gameWorldService;
        private readonly UnitMoveService _unitMoveService;
        private readonly FieldOfVisionService _fieldOfVisionService;

        public event EventHandler RenderUpdate;

        public GameInstanceService()
        {
            var testBuilder = new TestInstanceBuilderService();
            GameInstance = testBuilder.GetTestInstance();

            _turnPlanningService = new TurnPlanningService(this);
            _gameWorldService = new GameWorldService(this);
            _unitMoveService = new UnitMoveService(this, _gameWorldService);
            _fieldOfVisionService = new FieldOfVisionService();
        }

        public List<Player> AllPlayers {
            get {
                var players = new List<Player>();
                players.AddRange(GameInstance.TeamAlpha.Players);
                players.AddRange(GameInstance.TeamOmega.Players);
                return players;
            }
        }

        public TurnPlanningService GetTurnPlanningService()
        {
            return _turnPlanningService;
        }

        public List<Player> TempGetUnboundPlayers()
        {
            return AllPlayers.Where(p => !p.IsBoundToClient).ToList();
        }

        public void SignalWinningTeam(Team winningTeam)
        {
            GameInstance.WinningTeam = winningTeam;
        }

        /// <summary>
        /// This method is supposed to be called whenever a player signals that he is ready,
        /// i.e. has finished to plan the next turn. The method checks the planning status for all
        /// players in the instance and takes actions accordingly.
        /// </summary>
        /// <returns></returns>
        public void CheckPlanningState()
        {

            // Check whether all players are ready, if yes, execute the turn.
            var activePlayers = AllPlayers.Where(p => _turnPlanningService.GetAliveUnits(p).Count > 0);
            if (activePlayers.All(p => p.IsReadyInTurn))
            {
                // Stop countdown if it is still running (i.e. all players signaled ready before it ran out)
                if (GameInstance.TurnPlanningCountdown.IsActive)
                {
                    GameInstance.TurnPlanningCountdown.IsActive = false;
                }
                _ = ExecuteTurn();
                return;
            }

            // Check for one team to be ready completely, start the countdown for the remaining planning phase time
            // if it is not already activated.
            var teams = AllPlayers.GroupBy(p => p.Team);
            if (!GameInstance.TurnPlanningCountdown.IsActive && teams.Any(team => team.All(players => players.IsReadyInTurn)))
            {
                _ = ExecuteCountdown();
                return;
            }

        }

        public async Task ExecuteCountdown()
        {
            var countdown = GameInstance.TurnPlanningCountdown;
            countdown.Remaining = 30;
            countdown.IsActive = true;
            DispatchRenderUpdate();
            await Task.Run(() =>
            {
                while(countdown.IsActive)
                {
                    Thread.Sleep(1000);
                    countdown.Remaining--;
                    DispatchRenderUpdate();
                    if (countdown.Remaining < 1)
                    {
                        // Stop countdown and force ready state for all players
                        countdown.IsActive = false;
                        ForceAllPlayersReady();
                    }
                }
            });
        }

        private void ForceAllPlayersReady()
        {
            foreach (var player in AllPlayers)
            {
                player.IsReadyInTurn = true;
            }
            CheckPlanningState();
        }

        public async Task ExecuteTurn()
        {
            GameInstance.IsInPlanningPhase = false;

            var turnPlan = _turnPlanningService.GetTurnPlan();

            await Task.Run(() => {
                var unitsInTurn = turnPlan.Keys.ToList();
                unitsInTurn.Shuffle();

                Thread.Sleep(450);

                foreach (var nextUnit in unitsInTurn)
                {
                    if (nextUnit.IsDestroyed)
                        continue;
                    GameInstance.CurrentlyExecutingUnit = nextUnit;
                    var movesToExecute = turnPlan[nextUnit];
                    foreach (var executedMove in movesToExecute)
                    {
                        _gameWorldService.ClearAnimations();
                        _unitMoveService.TryExecuteMove(nextUnit, executedMove);
                        Thread.Sleep(450);
                        DispatchRenderUpdate();
                    }
                }

                _gameWorldService.ClearAnimations();

                // Reset turn plan and player ready states
                _turnPlanningService.ResetPlan();

                GameInstance.CurrentlyExecutingUnit = null;

                foreach (var player in AllPlayers)
                {
                    player.IsReadyInTurn = false;
                }
                GameInstance.IsInPlanningPhase = true;

                DispatchRenderUpdate();
            });
        }

        public void DispatchRenderUpdate()
        {
            // Dispatch to all client render update handlers
            RenderUpdate?.Invoke(this, EventArgs.Empty);
        }
    }

    public class FieldOfVisionService
    {
        const double CONV_DEG = (180 / Math.PI);

        /// <summary>
        /// For a vector with x and y components this computes an absolute angle in degrees, where
        /// NORTH (x=0, y=-1) becomes 0°, EAST (x=1, y=0) becomes 90°, ...
        /// </summary>
        /// <param name="vector">The input vector.</param>
        /// <returns>An angle in degrees</returns>
        private double GetNorthBasedAbsDeg((float x, float y) vector)
        {
            // special case of x = 0, prevent division by zero
            if (vector.x == 0)
            {
                if (vector.y <= 0) return 0;
                else return 180;
            }
            return (CONV_DEG * Math.Atan(vector.y / vector.x)) + ((vector.x < 0) ? 270 : 90);
        }

        /// <summary>
        /// Takes two angles given in degrees and computes the deviation , where clockwise deviation
        /// ranges from 0° to +180° and counter-clockwise deviation from 0° to -180°
        /// </summary>
        /// <param name="theta">The deviating angle in degrees.</param>
        /// <param name="phi">The reference angle in degrees.</param>
        /// <returns>An angle between minus and plus 180°.</returns>
        private double GetRelativeDeg(double theta, double phi)
        {
            double rho = theta - phi;
            return (rho <= 180) ? rho : (rho - 360);
        }

        private double GetEuclidDist(float dx, float dy)
        {
            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        private bool CheckCoordinateVisibility((float x, float y) coords, Observer observer)
        {
            (float x, float y) viewVector = (coords.x - observer.Position.x, coords.y - observer.Position.y);
            
            // Check by distance
            double distance = GetEuclidDist(viewVector.x, viewVector.y);
            if (distance > observer.MaxViewDistance) return false;

            // Check by angle
            double absTargetAngle = GetNorthBasedAbsDeg(viewVector);
            double absObserverAngle = GetNorthBasedAbsDeg(observer.FacingDirection);
            double relAngle = GetRelativeDeg(absTargetAngle, absObserverAngle);
            if (relAngle > (observer.FovAngle * 0.5)) return false;

            // Check by obstacle blocking

            return true;
        }

        private bool CheckMapPositionVisibility((int x, int y) mapPos, List<Observer> observers)
        {
            // Check if at least 2 corners are visible
            int visibleCornerCount = 0;
            for (int i = 0; (i < 4) && (visibleCornerCount < 2); i++)
            {
                int x = mapPos.x + (i < 2 ? 0 : 1);
                int y = mapPos.y + ((i % 2 == 0) ? 0 : 1);
                foreach (var observer in observers)
                {
                    if (CheckCoordinateVisibility((x, y), observer))
                    {
                        visibleCornerCount++;
                        break;
                    }
                }
            }
            return (visibleCornerCount >= 2);
        }

        public bool CheckMapPositionVisibleForTeam((int x, int y) mapPos, Team team)
        {
            var units = team.Players.SelectMany(p => p.Units);
        }
    }

    public class Observer
    {
        public (float x, float y) Position { get; set; }
        public (float dx, float dy) FacingDirection { get; set; }
        public double FovAngle { get; set; }
        public float MaxViewDistance { get; set; }
    }
}
