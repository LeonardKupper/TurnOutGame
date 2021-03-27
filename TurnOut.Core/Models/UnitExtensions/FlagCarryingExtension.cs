using System.Collections.Generic;
using TurnOut.Core.Models.Entities;

namespace TurnOut.Core.Models.EntityExtensions
{
    public class FlagCarryingExtension : UnitMoveExtensionBase
    {
        public class GrabFlagMove : IUnitMove { }
        public class DropFlagMove : IUnitMove { }
        public override List<IUnitMove> AssociatedMoves => new List<IUnitMove>
        {
            new GrabFlagMove(),
            new DropFlagMove()
        };


        public Flag CarriedFlag { get; set; }
        public bool IsCarryingFlag => (CarriedFlag != null);
    }
}