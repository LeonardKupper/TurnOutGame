using System;
using System.Collections.Generic;
using System.Linq;
using TurnOut.Core.Models.EntityExtensions;

namespace TurnOut.Core.Models.Entities.Units
{
    public abstract class UnitBase : EntityBase
    {
        public Player Player { get; set; }

        public UnitDirection Facing { get; set; }

        public int MovesPerTurn { get; set; }

        protected override void SetupBehavior()
        {
            base.SetupBehavior();

            // Default moves per turn:
            MovesPerTurn = 3;

            // Default move extensions:
            AddExtension<MobilityExtension>();
            AddExtension<FlagCarryingExtension>();

            // Default reaction behavior:
            AddExtension<BeamTargetExtension>();
            AddExtension<BeamDestructableExtension>();
            AddExtension<DashAttackableExtension>();
        }
    }
}