using Turnout.Core.Utility;
using TurnOut.Core.Models.Entities.Units;
using TurnOut.Core.Models.EntityExtensions;

namespace TurnOut.Core.Services
{
    public partial class UnitMoveService
    {
        private bool ExecuteMoveInternal(UnitBase unit, MobilityExtension.StepForwardMove move)
        {
            var dirVec = GetDirectionVector(unit.Facing);

            var pos = _gameWorldService.GetEntityPosition(unit);
            if (!pos.HasValue) return false;

            var targetPos = (x: pos.Value.x + dirVec.x, y: pos.Value.y + dirVec.y);

            return MoveUnitToPosition(unit, targetPos);
        }

        private bool ExecuteMoveInternal(UnitBase unit, MobilityExtension.TurnLeftMove move)
        {
            unit.Facing = unit.Facing.RotateClockWise(-1);
            return true;
        }

        private bool ExecuteMoveInternal(UnitBase unit, MobilityExtension.TurnRightMove move)
        {
            unit.Facing = unit.Facing.RotateClockWise(+1);
            return true;
        }
    }
}
