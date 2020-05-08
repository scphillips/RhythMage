// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using System.Collections.Generic;
using Zenject;

namespace RhythMage
{
    public class SoundManager : IInitializable, ITickable
    {
        [System.Serializable]
        public struct AudioTiming
        {
            public UnityEngine.AudioClip clip;
            public double bpm;
        }

        [System.Serializable]
        public class Settings
        {
            public List<AudioTiming> timings;
        }

        public event System.Action OnTrackChanged;
        public event System.Action OnBeat;
        public event System.Action OnHalfBeat;

        [Inject]
        UnityEngine.AudioSource m_audioSource;

        [Inject]
        RandomNumberProvider m_rng;

        [Inject]
        readonly Settings m_settings;

        double m_bpm;
        double m_beatLength;
        double m_halfBeatLength;
        int m_beatsInTrack;

        double m_lastSeenTime;
        
        public void Initialize()
        {
            PickNextTrack();
        }

        public float GetTrackLength()
        {
            return m_audioSource.clip.length;
        }

        public double GetBeatLength()
        {
            return m_beatLength;
        }

        public int GetTotalBeatsInTrack()
        {
            return m_beatsInTrack;
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
            return System.Math.Min(TimeSinceLastBeat(), TimeToNextBeat());
        }

        public bool WillBeatThisFrame(double beatMultiplier = 1.0)
        {
            double currentElapsed = m_audioSource.time;
            int currentBeatIndex = (int)(currentElapsed * m_bpm * beatMultiplier / 60.0);
            int previousBeatIndex = (int)(m_lastSeenTime * m_bpm * beatMultiplier / 60.0);
            return currentBeatIndex != previousBeatIndex;
        }

        public void Tick()
        {
            if (m_audioSource.time >= m_audioSource.clip.length || m_audioSource.isPlaying == false)
            {
                PickNextTrack();
                OnBeat?.Invoke();
            }
            else if (WillBeatThisFrame())
            {
                OnBeat?.Invoke();
            }
            else if (WillBeatThisFrame(2.0))
            {
                OnHalfBeat?.Invoke();
            }

            m_lastSeenTime = m_audioSource.time;
        }

        void PickNextTrack()
        {
            int trackIndex = m_rng.Next(m_settings.timings.Count);
            var timingData = m_settings.timings[trackIndex];
            m_audioSource.clip = timingData.clip;
            m_audioSource.Play();
            m_bpm = timingData.bpm;
            m_beatLength = m_bpm != 0.0 ? 60.0 / m_bpm : 0.0;
            m_halfBeatLength = m_beatLength * 0.5;
            m_beatsInTrack = System.Convert.ToInt32(GetTrackLength() / GetBeatLength());

            OnTrackChanged?.Invoke();
        }
    }
}
