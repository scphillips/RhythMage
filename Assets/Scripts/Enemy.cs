using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Outplay.RhythMage
{
    public class Enemy : MonoBehaviour
    {
        public CameraProvider Camera;
        public SoundManager SoundMgr;

        Cell m_location;

        void Start()
        {
            SoundMgr.OnBeat += OnBeat;
        }

        public void SetPosition(Cell cell)
        {
            m_location = cell;
            transform.localPosition = new Vector3(cell.x, 0, cell.y);
        }

        void OnBeat(object sender, EventArgs e)
        {
            // Update animation
            transform.localScale = new Vector2(transform.localScale.x * -1.0f, 1.0f);
        }

        void Update()
        {
            transform.forward = Camera.transform.forward;
        }
    }
}
