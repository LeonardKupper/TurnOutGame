using Turnout.Core.Utility;
using TurnOut.Core.Models.Entities;
using TurnOut.Core.Models.Entities.Units;
using TurnOut.Core.Models.EntityExtensions;

namespace TurnOut.Core.Services
{
    public partial class UnitMoveService
    {
        private bool ExecuteMoveInternal(UnitBase unit, DashAttackExtension.DashLeftMove move)
        {
            var forwardVector = GetDirectionVector(unit.Facing);
            var leftVector = GetDirectionVector(unit.Facing.RotateClockWise(-1));
            var dashVector = (forwardVector.x + leftVector.x, forwardVector.y + leftVector.y);

            return Attack(unit, dashVector);
        }

        private bool ExecuteMoveInternal(UnitBase unit, DashAttackExtension.DashRightMove move)
        {
            var forwardVector = GetDirectionVector(unit.Facing);
            var rightVector = GetDirectionVector(unit.Facing.RotateClockWise(+1));
            var dashVector = (forwardVector.x + rightVector.x, forwardVector.y + rightVector.y);

            return Attack(unit, dashVector);
        }

        private bool Attack(UnitBase unit, (int x, int y) dashVector)
        {
            var unitPos = _gameWorldService.GetEntityPosition(unit);
            if (!unitPos.HasValue) return false;

            var targetPos = (unitPos.Value.x + dashVector.x, unitPos.Value.y + dashVector.y);

            var targetEntity = _gameWorldService.GetEntityAt(targetPos, e => e.Has<DashAttackableExtension>());

            if (targetEntity is null) return false;

            // in case a flag carrier is attacked, the dasher unit picks up his flag
            if (targetEntity.Has<FlagCarryingExtension>(out var targetFlagCarrier))
            {
                if (!unit.Has<FlagCarryingExtension>(out var selfFlagCarrier)) return false;

                if (selfFlagCarrier.IsCarryingFlag && targetFlagCarrier.IsCarryingFlag) return false;

                if (!selfFlagCarrier.IsCarryingFlag && targetFlagCarrier.IsCarryingFlag) {
                    selfFlagCarrier.CarriedFlag = targetFlagCarrier.CarriedFlag;
                    targetFlagCarrier.CarriedFlag = null;
                }
            }

            DestroyEntity(targetEntity);

            return MoveUnitToPosition(unit, targetPos);
        }
    }
}
