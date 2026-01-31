// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RhythMage
{
    public class SoundManager : MonoBehaviour
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
            public AudioTiming menuTiming;
            public AudioTiming passLevelTiming;
            public List<AudioTiming> timings;
        }

        public event System.Action OnTrackChanged;
        public event System.Action OnBeat;
        public event System.Action OnHalfBeat;

        private UnityEngine.AudioSource m_audioSource;
        GameSettings m_settings;


        double m_bpm;
        double m_beatLength;
        double m_halfBeatLength;
        int m_beatsInTrack;

        double m_lastSeenTime;

        void Start()
        {
            m_settings = Utils.GetOrCreateGameSettings();

            SceneManager.sceneLoaded += FindAudioSource;
            FindAudioSource(SceneManager.GetActiveScene(), default);
        }

        void FindAudioSource(Scene scene, LoadSceneMode mode)
        {
            m_audioSource = null;

            GameObject[] gameObjects;
            gameObjects = GameObject.FindGameObjectsWithTag("PrimaryAudioSource");
            if (gameObjects.Length > 0)
            {
                m_audioSource = gameObjects[0].GetComponent<AudioSource>();
            }

            PlayNextTrack();
        }

        public void PlayOneShot(AudioClip clip)
        {
            m_audioSource.PlayOneShot(clip);
        }

        public float GetTrackLength()
        {
            return m_audioSource.clip?.length ?? 0.0f;
        }

        public double GetBeatLength()
        {
            return m_beatLength;
        }

        public double GetMaxTimeOffBeat()
        {
            return m_settings.GameDifficultySettings.maxInputTimeOffBeat;
        }

        public double GetTotalTime()
        {
            return m_audioSource.time;
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
            var timeSinceLastBeat = TimeSinceLastBeat();
            var timeToNextBeat = m_beatLength - timeSinceLastBeat;
            return System.Math.Min(timeSinceLastBeat, timeToNextBeat);
        }

        public bool WillBeatThisFrame(double beatMultiplier = 1.0)
        {
            double currentElapsed = m_audioSource.time;
            int currentBeatIndex = (int)(currentElapsed * m_bpm * beatMultiplier / 60.0);
            int previousBeatIndex = (int)(m_lastSeenTime * m_bpm * beatMultiplier / 60.0);
            return currentBeatIndex != previousBeatIndex;
        }

        private void Update()
        {
            if (m_audioSource.isPlaying == false || m_audioSource.clip == null || m_audioSource.time >= m_audioSource.clip.length)
            {
                PlayNextTrack();
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

        public void PlayNextTrack()
        {
            var timingData = GetNextTrack();
            if (timingData.clip)
            {
                Debug.Log(string.Format("Playing {0} at {1} bpm", timingData.clip, timingData.bpm));
                m_audioSource.clip = timingData.clip;
                m_audioSource.Play();
                m_bpm = timingData.bpm;
                m_beatLength = m_bpm != 0.0 ? 60.0 / m_bpm : 0.0;
                m_halfBeatLength = m_beatLength * 0.5;
                m_beatsInTrack = System.Convert.ToInt32(GetTrackLength() / GetBeatLength());

                OnTrackChanged?.Invoke();
            }
        }

        private AudioTiming GetNextTrack()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name == "MenuScene")
            {
                return m_settings.SoundManagerSettings.menuTiming;
            }
            else if (scene.name == "PassLevelScene")
            {
                return m_settings.SoundManagerSettings.passLevelTiming;
            }
            else if (scene.name == "GameScene")
            {
                GameStateManager gameStateManager = GetComponent<GameStateManager>();
                int trackIndex = Utils.GetRng().Next(m_settings.SoundManagerSettings.timings.Count);
                return m_settings.SoundManagerSettings.timings[trackIndex];
            }
            return default;
        }
    }
}
