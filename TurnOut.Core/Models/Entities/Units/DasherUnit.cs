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
        }
    }
}