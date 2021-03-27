using System.Collections.Generic;

namespace TurnOut.Core.Models.EntityExtensions
{
    /// <summary>
    /// Special entity extension type wrapping a set of enabled unit moves.
    /// </summary>
    public abstract class UnitMoveExtensionBase : IEntityExtension
    {
        public abstract List<IUnitMove> AssociatedMoves { get; }
    }

    public interface IUnitMove { }
}