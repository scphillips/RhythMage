// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using UnityEngine;

namespace RhythMage
{
    [ExecuteInEditMode]
    public class CameraController : MonoBehaviour
    {
        public ICameraBehavior Behavior { get; set; }
        public GameObject Camera { get; set; }

        public void LateTick()
        {
            if (Behavior != null)
            {
                Behavior.Resolve(Camera);
            }
        }
    }
}
