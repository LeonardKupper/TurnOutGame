using TurnOut.Core.Models.EntityExtensions;

namespace TurnOut.Core.Models.Entities.Units
{
    public class DasherUnit : UnitBase
    {
        protected override void SetupBehavior()
        {
            base.SetupBehavior();

            // dasher can atack diagonally
            AddExtension<DashAttackExtension>();

            // set up FOV
            ConfigureExtension<VisionExtension>(v =>
            {
                v.RelativeFOVs.Add(new RelativeFieldOfView
                {
                    AngleOffset = 0,
                    PostionOffset = (front: 0.45f, 0.0f),
                    ViewAngle = 120,
                    ViewDepth = 7.5f
                });
            });
        }
    }
}