// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, January 2022

using System.Collections.Generic;
using UnityEngine;

namespace RhythMage
{
    [System.Serializable]
    public class DirectionGameObjectPair
    {
        public Direction direction;
        public GameObject gameObject;
    }

    public class ConnectionListDisplay : MonoBehaviour
    {
        [NamedList]
        public List<DirectionGameObjectPair> connections;

        public void SetConnections(List<System.ValueTuple<Direction, GameObject>> connectionData)
        {
            connections = new List<DirectionGameObjectPair>();
            foreach (var (direction, entity) in connectionData)
            {
                DirectionGameObjectPair pair = new DirectionGameObjectPair
                {
                    direction = direction,
                    gameObject = entity
                };
                connections.Add(pair);
            }
        }
    }
}
