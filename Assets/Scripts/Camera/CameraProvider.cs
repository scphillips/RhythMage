// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using UnityEngine;

namespace RhythMage
{
    public class CameraProvider : MonoBehaviour
    {
        public Camera m_camera;
        public Camera Camera => m_camera;
        public Camera Get() => Camera ?? GetComponent<Camera>();
    }
}
