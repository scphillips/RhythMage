using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Zenject.Inject]
    public AudioSource audioSource;

    readonly float bps = 0.45f;
    readonly float halfBPS = 0.45f * 0.5f;
    float m_lastSeenTime;

    public event EventHandler OnBeat;

    public SoundManager()
    {
        m_lastSeenTime = 0.0f;
    }

    public float GetBeatLength()
    {
        return bps;
    }

    public float TimeOffBeat()
    {
        return halfBPS - Math.Abs(halfBPS - m_lastSeenTime);
    }

    void Update()
    {
        float currentElapsed = audioSource.time;
        int currentBeatIndex = (int)(currentElapsed / bps);
        int lastBeatIndex = (int)(m_lastSeenTime / bps);
        if (currentBeatIndex != lastBeatIndex)
        {
            OnBeat(this, null);
        }
        m_lastSeenTime = currentElapsed;
    }
}
