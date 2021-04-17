using TurnOut.Core.Models.EntityExtensions;

namespace TurnOut.Core.Models.Entities.Units
{
    public class RunnerUnit : UnitBase
    {
        protected override void SetupBehavior()
        {
            base.SetupBehavior();

            // runner has an extra move per turn
            MovesPerTurn += 1;

            // set up FOV
            ConfigureExtension<VisionExtension>(v =>
            {
                v.RelativeFOVs.Add(new RelativeFieldOfView
                {
                    AngleOffset = 40,
                    PostionOffset = (front: 0.55f, 0.45f),
                    ViewAngle = 100,
                    ViewDepth = 10.0f
                });
                v.RelativeFOVs.Add(new RelativeFieldOfView
                {
                    AngleOffset = -40,
                    PostionOffset = (front: 0.55f, -0.45f),
                    ViewAngle = 100,
                    ViewDepth = 10.0f
                });
            });
        }
    }
}