using System.Collections.Generic;

namespace TurnOut.Core.Models.EntityExtensions
{
    public class DashAttackExtension : UnitMoveExtensionBase
    {
        public class DashRightMove : IUnitMove { }
        public class DashLeftMove : IUnitMove { }
        public override List<IUnitMove> AssociatedMoves => new List<IUnitMove>
        {
            new DashLeftMove(),
            new DashRightMove()
        };
    }

    public class DashAttackableExtension : IEntityExtension { }
}