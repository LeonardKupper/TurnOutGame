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

        public event EventHandler RenderUpdate;

        public GameInstanceService()
        {
            var testBuilder = new TestInstanceBuilderService();
            GameInstance = testBuilder.GetTestInstance();

            _turnPlanningService = new TurnPlanningService(this);
            _gameWorldService = new GameWorldService(this);
            _unitMoveService = new UnitMoveService(this, _gameWorldService);
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

        public async Task CheckForTurnExecution()
        {
            var activePlayers = AllPlayers.Where(p => _turnPlanningService.GetAliveUnits(p).Count > 0);
            if (activePlayers.All(p => p.IsReadyInTurn))
            {
                await ExecuteTurn();
            }
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
}
