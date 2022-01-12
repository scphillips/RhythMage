// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RhythMage
{
    public class DanceAnimation : MonoBehaviour
    {
        [System.Serializable]
        public struct AnimationEntry
        {
            public SpriteRenderer target;
            public Image ui_target;
            public Sprite[] animationFrames;
        }

        [Zenject.Inject]
        readonly SoundManager soundManager;

        public List<AnimationEntry> animationEntries;

        [field: SerializeField]
        public bool UseHalfBeat { get; set; }

        int m_frameCount;
        int m_currentFrame;

        void Start()
        {
            if (animationEntries.Count > 0)
            {
                m_frameCount = animationEntries[0].animationFrames.Length;
                soundManager.OnBeat += OnBeat;
                soundManager.OnHalfBeat += OnHalfBeat;
            }
        }

        void OnBeat()
        {
            UpdateAnimation();
        }

        void OnHalfBeat()
        {
            if (UseHalfBeat)
            {
                UpdateAnimation();
            }
        }

        void UpdateAnimation()
        {
            m_currentFrame = (m_currentFrame + 1) % m_frameCount;
            foreach (var entry in animationEntries)
            {
                if (entry.target != null)
                {
                    entry.target.sprite = entry.animationFrames[m_currentFrame];
                }
                if (entry.ui_target != null)
                {
                    entry.ui_target.sprite = entry.animationFrames[m_currentFrame];
                }
            }
        }
    }
}
