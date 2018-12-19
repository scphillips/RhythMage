using System;
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
            public double bpm;
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

        double m_bpm;
        double m_beatLength;
        double m_halfBeatLength;

        double m_lastSeenTime;
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
            m_bpm = timingData.bpm;
            m_beatLength = 60.0 / m_bpm;
            m_halfBeatLength = m_beatLength * 0.5;
        }

        public float GetTrackLength()
        {
            return m_audioSource.clip.length;
        }

        public double GetBeatLength()
        {
            return m_beatLength;
        }

        public double TimeSinceLastBeat()
        {
            return m_audioSource.time % m_beatLength;
        }

        public double TimeToNextBeat()
        {
            return m_beatLength - TimeSinceLastBeat();
        }

        public double TimeOffBeat()
        {
            return m_halfBeatLength - Math.Abs(m_halfBeatLength - TimeSinceLastBeat());
        }

        public void Tick()
        {
            double currentElapsed = m_audioSource.time;
            double beatCounter = (int)(currentElapsed / 60.0) * m_bpm;
            beatCounter += ((currentElapsed % 60.0) / m_beatLength);
            int currentBeatIndex = (int)beatCounter;
            if (currentBeatIndex != m_lastBeatIndex)
            {
                // Ensure we don't trigger an additional beat due to rounding error on loop
                if (currentBeatIndex > m_lastBeatIndex
                    || (m_lastSeenTime - m_beatLength * m_lastBeatIndex) > m_halfBeatLength)
                {
                    OnBeat(this, null);
                }
            }
            m_lastSeenTime = currentElapsed;
            m_lastBeatIndex = currentBeatIndex;
        }
    }
}
