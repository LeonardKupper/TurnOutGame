using System.Collections.Generic;

namespace TurnOut.Core.Models.EntityExtensions
{
    public class MobilityExtension : UnitMoveExtensionBase
    {
        public class StepForwardMove : IUnitMove { }
        public class TurnRightMove : IUnitMove { }
        public class TurnLeftMove : IUnitMove { }
        public override List<IUnitMove> AssociatedMoves => new List<IUnitMove>
        {
            new StepForwardMove(),
            new TurnLeftMove(),
            new TurnRightMove()
        };

    }
}