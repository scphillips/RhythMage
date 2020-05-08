// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using System.Collections.Generic;

namespace RhythMage
{
    public enum Direction
    {
        Forward,
        Right,
        Backward,
        Left,
        None
    }

    public enum EnemyType
    {
        Magic,
        Melee
    }

    public class Defs
    {
        public static readonly int enemyTypeCount = System.Enum.GetValues(typeof(EnemyType)).Length;

        public static IReadOnlyDictionary<Direction, CoordinateOffset> facings = new Dictionary<Direction, CoordinateOffset>()
        {
            { Direction.Forward, CoordinateOffset.Create(0, 1) },
            { Direction.Right, CoordinateOffset.Create(1, 0) },
            { Direction.Backward, CoordinateOffset.Create(0, -1) },
            { Direction.Left, CoordinateOffset.Create(-1, 0) }
        };

        public static Direction GetOffsetDirection(ref CoordinateOffset offset)
        {
            foreach (var entry in facings)
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
