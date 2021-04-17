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

            // set up FOV
            ConfigureExtension<VisionExtension>(v =>
            {
                v.RelativeFOVs.Add(new RelativeFieldOfView
                {
                    AngleOffset = 0,
                    PostionOffset = (front: 0.45f, 0.0f),
                    ViewAngle = 80,
                    ViewDepth = 100.0f
                });
            });
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