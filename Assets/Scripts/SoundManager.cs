using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    readonly float bps = 0.45f;
    readonly float halfBPS = 0.45f * 0.5f;
    float m_elapsed;

    public event EventHandler OnBeat;

    public SoundManager()
    {
        m_elapsed = 0.0f;
    }

    public float GetBeatLength()
    {
        return bps;
    }

    public float TimeOffBeat()
    {
        return halfBPS - Math.Abs(halfBPS - m_elapsed);
    }

    void Update()
    {
        m_elapsed += Time.deltaTime;
        if (m_elapsed >= bps)
        {
            m_elapsed -= bps;
            OnBeat(this, null);
        }
    }
}
