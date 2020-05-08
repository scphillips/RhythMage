using System;
using System.Collections.Generic;
using UnityEngine;

namespace Outplay.RhythMage
{
    public class DanceAnimation : MonoBehaviour
    {
        [Serializable]
        public struct AnimationEntry
        {
            public SpriteRenderer target;
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
                entry.target.sprite = entry.animationFrames[m_currentFrame];
            }
        }
    }
}
