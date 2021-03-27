using System.Collections.Generic;
using System.Linq;
using TurnOut.Core.Models;
using TurnOut.Core.Models.Entities.Units;
using TurnOut.Core.Models.EntityExtensions;

namespace TurnOut.Core.Services
{
    public class TurnPlanningService
    {
        private readonly GameInstanceService _gameInstanceService;

        public TurnPlanningService(GameInstanceService gameInstanceService)
        {
            _gameInstanceService = gameInstanceService;
        }

        private Dictionary<UnitBase, List<IUnitMove>> turnPlan { get; set; } = new();

        public List<UnitBase> GetAliveUnits(Player player)
        {
            return player.Units.Where(u => !u.IsDestroyed).ToList();
        }

        public List<IUnitMove> GetAvailableMoves(UnitBase unit)
        {
            return unit.Extensions
                .Where(ext => ext.GetType().IsAssignableTo(typeof(UnitMoveExtensionBase)))
                .Select(ext => (UnitMoveExtensionBase)ext)
                .SelectMany(ext => ext.AssociatedMoves).ToList();
        }

        public bool UnitHasMovesLeft(UnitBase unit)
        {
            return ((!turnPlan.ContainsKey(unit) ? 0 : (turnPlan[unit]?.Count ?? 0)) < unit.MovesPerTurn);
        }

        public void PlanMoveForUnit(UnitBase unit, IUnitMove move)
        {
            if (!UnitHasMovesLeft(unit)) return;

            if (!turnPlan.ContainsKey(unit))
            {
                turnPlan[unit] = new List<IUnitMove>();
            }
            turnPlan[unit].Add(move);
            _gameInstanceService.DispatchRenderUpdate();
        }

        public void UndoLastMoveForUnit(UnitBase unit)
        {
            if (turnPlan[unit].Count < 1) return;
            turnPlan[unit].RemoveAt(turnPlan[unit].Count - 1);
            _gameInstanceService.DispatchRenderUpdate();
        }

        public Dictionary<UnitBase, List<IUnitMove>> GetTurnPlan()
        {
            return turnPlan;
        }

        public void ResetPlan()
        {
            turnPlan = new();
        }
    }
}