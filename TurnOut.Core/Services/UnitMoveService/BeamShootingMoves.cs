using TurnOut.Core.Models.Entities;
using TurnOut.Core.Models.Entities.Units;
using TurnOut.Core.Models.EntityExtensions;

namespace TurnOut.Core.Services
{
    public partial class UnitMoveService
    {
        private bool ExecuteMoveInternal(UnitBase unit, BeamShootingExtension.ShootBeamMove move)
        {
            var dirVec = GetDirectionVector(unit.Facing);

            var unitPos = _gameWorldService.GetEntityPosition(unit);
            if (!unitPos.HasValue) return false;

            var trajectPos = (unitPos.Value.x, unitPos.Value.y);

            EntityBase hitEntity = null;
            while (hitEntity is null)
            {
                trajectPos.x += dirVec.x;
                trajectPos.y += dirVec.y;
                if (!_gameWorldService.IsValidPosition(trajectPos))
                    break;
                _gameWorldService.DisplayAnimation(trajectPos, "yellow");
                hitEntity = _gameWorldService.GetEntityAt(trajectPos, e => e.Has<BeamTargetExtension>());
            }

            if (hitEntity is not null && hitEntity.Has<BeamDestructableExtension>())
            {
                DestroyEntity(hitEntity);
            }

            return true;
        }
    }
}
