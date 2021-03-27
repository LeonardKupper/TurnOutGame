using System.Collections.Generic;
using TurnOut.Core.Models.EntityExtensions;

namespace TurnOut.Core.Models.Entities
{
    public class Obstacle : EntityBase
    {
        protected override void SetupBehavior()
        {
            base.SetupBehavior();
            AddExtension<BeamTargetExtension>();
        }
    }
}
