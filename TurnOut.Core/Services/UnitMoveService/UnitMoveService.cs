using System.Collections.Generic;
using System.Linq;
using TurnOut.Core.Models.Entities;
using TurnOut.Core.Models.Entities.Units;
using TurnOut.Core.Models.EntityExtensions;

namespace TurnOut.Core.Services
{

    public partial class UnitMoveService
    {
        private readonly GameInstanceService _gameInstanceService;
        private readonly GameWorldService _gameWorldService;

        public UnitMoveService(GameInstanceService gameInstanceService, GameWorldService gameWorldService)
        {
            _gameInstanceService = gameInstanceService;
            _gameWorldService = gameWorldService;
        }

        public bool TryExecuteMove(UnitBase executingUnit, IUnitMove moveToExecute)
        {
            // Invoke move implementation based on dynamic polymorphism:
            bool status = ExecuteMoveInternal(executingUnit, (dynamic)moveToExecute);

            // Update visible positions after move
            if (status) _gameInstanceService.UpdateVisiblePositionsForTeam(executingUnit.Player.Team);
            
            return status;
        }

        // Implementations of the different moves in partial classes

        // Acts as a catch-all for dynamic polymorphism:
        private bool ExecuteMoveInternal(UnitBase unit, IUnitMove move) => false;


        // Shared utility:

        public (int x, int y) GetDirectionVector(UnitDirection direction)
        {
            int dx = 0, dy = 0;
            switch (direction)
            {
                case UnitDirection.North:
                    dy--;
                    break;
                case UnitDirection.East:
                    dx++;
                    break;
                case UnitDirection.South:
                    dy++;
                    break;
                case UnitDirection.West:
                    dx--;
                    break;
                default:
                    break;
            }
            return (dx, dy);
        }

        private (int x, int y)? GetLookingAtPos(UnitBase unit)
        {
            var unitPos = _gameWorldService.GetEntityPosition(unit);
            if (!unitPos.HasValue) return null;

            var dirVec = GetDirectionVector(unit.Facing);
            return (unitPos.Value.x + dirVec.x, unitPos.Value.y + dirVec.y);
        }

        private void DestroyEntity(EntityBase e)
        {
            var pos = _gameWorldService.GetEntityPosition(e);
            if (pos.HasValue)
                _gameWorldService.RemoveEntityAt(pos.Value);

            // Flag carriers drop flags
            if (e.Has<FlagCarryingExtension>(out var flagCarrier))
            {
                if (flagCarrier.IsCarryingFlag)
                {
                    _gameWorldService.ReplaceEntityAt(pos.Value, flagCarrier.CarriedFlag);
                }
            }

            e.IsDestroyed = true;
        }

        private bool MoveUnitToPosition(UnitBase unit, (int x, int y) targetPos)
        {
            if (!_gameWorldService.IsValidPosition(targetPos)) return false;

            var collisionEntity = _gameWorldService.GetEntityAt(targetPos);
            if (collisionEntity != null) return false;

            var pos = _gameWorldService.GetEntityPosition(unit);
            if (!pos.HasValue) return false;

            _gameWorldService.RemoveEntityAt(pos.Value);
            _gameWorldService.ReplaceEntityAt(targetPos, unit);

            return true;
        }
    }

}