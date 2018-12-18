﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Outplay.RhythMage
{
    public class SoundManager : Zenject.ITickable
    {
        [Serializable]
        public struct AudioTiming
        {
            public AudioClip clip;
            public float bps;
        }

        [Serializable]
        public class Settings
        {
            public List<AudioTiming> timings;
        }

        public event EventHandler OnBeat;
        
        AudioSource m_audioSource;
        RandomNumberProvider m_rng;
        readonly Settings m_settings;

        float m_bps;
        float m_halfBPS;

        float m_lastSeenTime;
        int m_lastBeatIndex;

        [Zenject.Inject]
        public SoundManager(AudioSource audioSource, RandomNumberProvider rng, Settings settings)
        {
            m_audioSource = audioSource;
            m_rng = rng;
            m_settings = settings;

            int trackIndex = m_rng.Next(m_settings.timings.Count);
            var timingData = m_settings.timings[trackIndex];
            m_audioSource.clip = timingData.clip;
            m_audioSource.Play();
            m_bps = timingData.bps;
            m_halfBPS = m_bps * 0.5f;
        }

        public float GetBeatLength()
        {
            return m_bps;
        }

        public float TimeOffBeat()
        {
            return m_halfBPS - Math.Abs(m_halfBPS - m_lastSeenTime);
        }

        public void Tick()
        {
            float currentElapsed = m_audioSource.time;
            int currentBeatIndex = (int)(currentElapsed / m_bps);
            if (currentBeatIndex != m_lastBeatIndex)
            {
                OnBeat(this, null);
            }
            m_lastSeenTime = currentElapsed;
            m_lastBeatIndex = currentBeatIndex;
        }
    }
}
