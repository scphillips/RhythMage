// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RhythMage
{
    public class TitleSceneController : MonoBehaviour
    {
        [Zenject.Inject]
        readonly GameStateManager.Settings m_settings;

        public List<Image> fadeEntities;
        public AudioSource audioSource;

        readonly float m_fadeSpeed = 2.0f;
        float m_opacity;
        bool m_isEnding;

        void Start()
        {
            m_opacity = 0.0f;
            m_isEnding = false;
        }

        void Update()
        {
            if (m_isEnding == false && m_opacity == 1.0f)
            {
                m_isEnding = (Input.anyKeyDown || audioSource.isPlaying == false);
            }

            float mag = Time.deltaTime * m_fadeSpeed;
            float delta = (m_isEnding) ? -mag : mag;
            m_opacity = Defs.Clamp(m_opacity + delta, 0.0f, 1.0f);
            foreach (var entry in fadeEntities)
            {
                var image = entry.GetComponent<Image>();
                var color = image.color;
                color.a = m_opacity;
                image.color = color;
            }

            if (m_isEnding && m_opacity == 0.0f)
            {
                SceneManager.LoadScene(m_settings.menuScene.ScenePath);
            }
        }
    }
}
