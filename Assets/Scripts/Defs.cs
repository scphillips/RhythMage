using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Outplay.RhythMage
{
    public class Defs
    {
        public enum Direction
        {
            Forwards,
            Right,
            Backwards,
            Left
        }

        public static Dictionary<Direction, CoordinateOffset> Facings = new Dictionary<Direction, CoordinateOffset>()
        {
            { Direction.Forwards, CoordinateOffset.Create(0, 1) },
            { Direction.Right, CoordinateOffset.Create(1, 0) },
            { Direction.Backwards, CoordinateOffset.Create(0, -1) },
            { Direction.Left, CoordinateOffset.Create(-1, 0) }
        };
    }
}
