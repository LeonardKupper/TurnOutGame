using System.Collections.Generic;

namespace TurnOut.Core.Models.EntityExtensions
{
    public class BeamShootingExtension : UnitMoveExtensionBase
    {
        public class ShootBeamMove : IUnitMove { }
        public override List<IUnitMove> AssociatedMoves => new List<IUnitMove>
        {
            new ShootBeamMove()
        };


        public int Ammunition { get; set; }
    }

    public class BeamTargetExtension : IEntityExtension { }

    public class BeamDestructableExtension : IEntityExtension { }
}