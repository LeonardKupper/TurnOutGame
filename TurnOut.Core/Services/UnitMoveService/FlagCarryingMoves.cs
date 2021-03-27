using TurnOut.Core.Models.Entities;
using TurnOut.Core.Models.Entities.Units;
using TurnOut.Core.Models.EntityExtensions;

namespace TurnOut.Core.Services
{
    public partial class UnitMoveService
    {
        private bool ExecuteMoveInternal(UnitBase unit, FlagCarryingExtension.GrabFlagMove move)
        {
            if (!unit.Has<FlagCarryingExtension>(out var state)) return false;

            // Can not pick up flag while holding other flag
            if (state.IsCarryingFlag) return false;

            var lookingAtPos = GetLookingAtPos(unit);
            if (lookingAtPos is null) return false;

            var target = _gameWorldService.GetEntityAt(lookingAtPos.Value);
            if (target is null) return false;

            if (!target.GetType().IsAssignableTo(typeof(Flag))) return false;

            if (target == unit.Player.Team.Flag) return false;

            var flag = (Flag)target;
            _gameWorldService.RemoveEntityAt(lookingAtPos.Value);
            state.CarriedFlag = flag;

            return true;
        }

        private bool ExecuteMoveInternal(UnitBase unit, FlagCarryingExtension.DropFlagMove move)
        {
            if (!unit.Has<FlagCarryingExtension>(out var state)) return false;

            if (state.CarriedFlag is null) return false;

            var lookingAtPos = GetLookingAtPos(unit);
            if (lookingAtPos is null) return false;

            var target = _gameWorldService.GetEntityAt(lookingAtPos.Value);
            if (target is null)
            {
                _gameWorldService.ReplaceEntityAt(lookingAtPos.Value, state.CarriedFlag);
                state.CarriedFlag = null;
                return true;
            }

            if ((state.CarriedFlag == unit.Player.Team.OpponentTeam.Flag) && (target == unit.Player.Team.Flag)) {
                _gameInstanceService.SignalWinningTeam(unit.Player.Team);
                return true;
            }

            return false;
        }
    }
}
