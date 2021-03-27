using System;
using System.Collections.Generic;
using TurnOut.Core.Models.Entities.Units;

namespace Turnout.Core.Utility
{
    public static class FisherYatesListShuffleExtensions
    {
        private static Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    public static class UnitDirectionExtensions
    {
        public static UnitDirection RotateClockWise(this UnitDirection direction, int count)
        {
            UnitDirection rotated;
            if (count > 0)
            {
                rotated = direction switch
                {
                    UnitDirection.North => UnitDirection.East,
                    UnitDirection.East => UnitDirection.South,
                    UnitDirection.South => UnitDirection.West,
                    UnitDirection.West => UnitDirection.North,
                    _ => direction
                };
                return RotateClockWise(rotated, count - 1);
            }
            if (count < 0)
            {
                rotated = direction switch
                {
                    UnitDirection.North => UnitDirection.West,
                    UnitDirection.East => UnitDirection.North,
                    UnitDirection.South => UnitDirection.East,
                    UnitDirection.West => UnitDirection.South,
                    _ => direction
                };
                return RotateClockWise(rotated, count + 1);
            }
            return direction;
        }
    }
}
