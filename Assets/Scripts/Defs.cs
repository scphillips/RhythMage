using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Outplay.RhythMage
{
    public enum Direction
    {
        Forwards,
        Right,
        Backwards,
        Left,
        None
    }

    public class Defs
    {
        public static Dictionary<Direction, CoordinateOffset> Facings = new Dictionary<Direction, CoordinateOffset>()
        {
            { Direction.Forwards, CoordinateOffset.Create(0, 1) },
            { Direction.Right, CoordinateOffset.Create(1, 0) },
            { Direction.Backwards, CoordinateOffset.Create(0, -1) },
            { Direction.Left, CoordinateOffset.Create(-1, 0) }
        };

        public static Direction GetOffsetDirection(ref CoordinateOffset offset)
        {
            foreach (var entry in Facings)
            {
                if (entry.Value == offset)
                {
                    return entry.Key;
                }
            }
            return Direction.None;
        }
    }
}
