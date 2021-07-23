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
    public class GameSessionService
    {
        private Dictionary<string, GameSession> ActiveSessions { get; set; }
        private object SessionCreationLock { get; set; } = new object();

        public GameSessionService()
        {
            ActiveSessions = new Dictionary<string, GameSession>();
        }

        public GameSession CreateNewSession()
        {
            GameSession session;
            lock (SessionCreationLock)
            {
                session = new GameSession
                {
                    ConnectedPlayers = new HashSet<Player>(),
                    SessionCode = GenerateRandomSessionCode()
                };
                ActiveSessions.Add(session.SessionCode, session);
            }
            return session;
        }

        public GameSession FindSessionByCode(string sessionCode)
        {
            return ActiveSessions.GetValueOrDefault(sessionCode);
        }

        public string GenerateRandomSessionCode()
        {
            string sessionCode;
            do
            {
                sessionCode = GenerateRandomString("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 6);
            } while (ActiveSessions.ContainsKey(sessionCode));
            return sessionCode;
        }

        public string GenerateRandomString(string baseSet, ushort length)
        {
            var stringChars = new char[length];
            var random = new Random();
            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = baseSet[random.Next(baseSet.Length)];
            }
            return new string(stringChars);
        }

        public void AddPlayerToSession(GameSession gameSession, Player player)
        {
            gameSession.ConnectedPlayers.Add(player);
            player.Session = gameSession;
        }
    }


    /// <summary>
    /// Main Service responsible for creating and managing game state.
    /// </summary>
    public class GameInstanceService
    {
        public GameInstance GameInstance { get; set; }

        private readonly TurnPlanningService _turnPlanningService;
        private readonly GameWorldService _gameWorldService;
        private readonly UnitMoveService _unitMoveService;
        private readonly FieldOfViewService _fieldOfVisionService;

        public event EventHandler RenderUpdate;

        public GameInstanceService()
        {
            var testBuilder = new TestInstanceBuilderService();
            GameInstance = testBuilder.GetTestInstance();

            _turnPlanningService = new TurnPlanningService(this);
            _gameWorldService = new GameWorldService(this);
            _unitMoveService = new UnitMoveService(this, _gameWorldService);
            _fieldOfVisionService = new FieldOfViewService(this, _gameWorldService, _unitMoveService);

            // FOV Initialization
            UpdateVisiblePositionsForTeam(GameInstance.TeamAlpha);
            UpdateVisiblePositionsForTeam(GameInstance.TeamOmega);
        }

        public List<Player> AllPlayers
        {
            get
            {
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
                while (countdown.IsActive)
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

            await Task.Run(() =>
            {
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

        public void UpdateVisiblePositionsForTeam(Team team)
        {
            team.VisiblePositions = _fieldOfVisionService.ComputeVisiblePositionsForTeam(team);
        }
    }
}
