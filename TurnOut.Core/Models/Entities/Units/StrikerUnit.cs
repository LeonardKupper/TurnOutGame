using TurnOut.Core.Models.EntityExtensions;

namespace TurnOut.Core.Models.Entities.Units
{
    public class StrikerUnit : UnitBase
    {
        protected override void SetupBehavior()
        {
            base.SetupBehavior();

            // striker can shoot a beam
            AddExtension<BeamShootingExtension>();
        }
    }

    public class DefenderUnit : UnitBase
    {
        protected override void SetupBehavior()
        {
            base.SetupBehavior();

            // defender moves slow
            MovesPerTurn -= 1;

            // defenders can not be destroyed, but they also can't carry flags
            RemoveExtension<BeamDestructableExtension>();
            RemoveExtension<DashAttackableExtension>();
            RemoveExtension<FlagCarryingExtension>();
        }
    }
}