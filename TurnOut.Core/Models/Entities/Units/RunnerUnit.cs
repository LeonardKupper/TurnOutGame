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
        }
    }
}