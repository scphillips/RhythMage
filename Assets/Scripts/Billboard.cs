// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using UnityEngine;

namespace RhythMage
{
    public class Billboard : MonoBehaviour
    {
        [Zenject.Inject]
        readonly CameraProvider cameraProvider;

        void Update()
        {
            transform.forward = cameraProvider.transform.forward;
        }
    }
}
