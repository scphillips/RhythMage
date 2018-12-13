using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    readonly float bps = 60.0f / 200.0f;
    float m_elapsed;

    public event EventHandler OnBeat;

    public SoundManager()
    {
        m_elapsed = 0.0f;
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
