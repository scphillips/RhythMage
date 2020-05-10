// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using UnityEngine;

namespace RhythMage
{
    public class GestureHandler : Zenject.ITickable
    {
        public float MinThreshold = 20.0f;

        public class GestureSwipeEventArgs : System.EventArgs
        {
            public Direction Direction { get; set; }
        }

        public event System.Action<GestureSwipeEventArgs> OnSwipe;

        Vector2 m_startTouchPos;
        bool m_canSwipe;

        void Start()
        {
            m_canSwipe = false;
        }

        public void Tick()
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
                OnSwipe?.Invoke(new GestureSwipeEventArgs
                {
                    Direction = Direction.Right
                });
            }
            else if (Input.GetKeyDown("right"))
            {
                OnSwipe?.Invoke(new GestureSwipeEventArgs
                {
                    Direction = Direction.Left
                });
            }
            else if (Input.GetKeyDown("up"))
            {
                OnSwipe?.Invoke(new GestureSwipeEventArgs
                {
                    Direction = Direction.Forward
                });
            }
            else if (Input.GetKeyDown("down"))
            {
                OnSwipe?.Invoke(new GestureSwipeEventArgs
                {
                    Direction = Direction.Backward
                });
            }
        }

        void CheckForSwipe(Vector2 position)
        {
            Vector2 offset = position - m_startTouchPos;

            if (offset.SqrMagnitude() >= MinThreshold * MinThreshold)
            {
                // Swipe detected
                Direction direction;
                if (System.Math.Abs(offset.x) > System.Math.Abs(offset.y))
                {
                    direction = (offset.x < 0) ? Direction.Left : Direction.Right;
                }
                else
                {
                    direction = (offset.y < 0) ? Direction.Backward : Direction.Forward;
                }

                var args = new GestureSwipeEventArgs
                {
                    Direction = direction
                };
                OnSwipe?.Invoke(args);
                m_canSwipe = false;
            }
        }
    }
}
