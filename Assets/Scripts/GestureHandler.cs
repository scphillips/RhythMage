using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Outplay.RhythMage
{
    public class GestureHandler : MonoBehaviour
    {
        Vector2 m_startTouchPos;
        bool m_canSwipe;

        public float MinThreshold = 20.0f;

        public class GestureSwipeEventArgs : EventArgs
        {
            public Direction Direction { get; set; }
        }

        public event EventHandler OnSwipe;

        void Start()
        {
            m_canSwipe = false;
        }

        void Update()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    m_startTouchPos = touch.position;
                    m_canSwipe = true;
                }
                else if (m_canSwipe == true && touch.phase == TouchPhase.Moved)
                {
                    CheckForSwipe(touch.position);
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                m_canSwipe = true;
                m_startTouchPos = Input.mousePosition;
            }
            else if (m_canSwipe == true && Input.GetMouseButton(0))
            {
                CheckForSwipe(Input.mousePosition);
            }
            else if (Input.GetKeyDown("left"))
            {
                OnSwipe(this, new GestureSwipeEventArgs
                {
                    Direction = Direction.Right
                });
            }
            else if (Input.GetKeyDown("right"))
            {
                OnSwipe(this, new GestureSwipeEventArgs
                {
                    Direction = Direction.Left
                });
            }
        }

        void CheckForSwipe(Vector2 position)
        {
            Vector2 offset = position - m_startTouchPos;

            if (offset.SqrMagnitude() >= MinThreshold * MinThreshold)
            {
                // Swipe detected
                var args = new GestureSwipeEventArgs
                {
                    Direction = (offset.x < 0) ? Direction.Left : Direction.Right
                };
                OnSwipe(this, args);
                m_canSwipe = false;
            }
        }
    }
}
