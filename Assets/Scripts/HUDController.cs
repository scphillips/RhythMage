using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Outplay.RhythMage
{
    public class HUDController : MonoBehaviour
    {
        [System.Serializable]
        public class Settings
        {
            public Sprite heartFull;
            public Sprite heartBroken;
        }

        [Zenject.Inject]
        readonly Settings m_settings;

        [Zenject.Inject]
        AvatarModel m_avatar;

        public List<GameObject> HealthPanels;

        void Start()
        {
            UpdateHealthUI();
            m_avatar.OnHealthChange += OnHealthChanged;
        }

        void OnHealthChanged(object sender, EventArgs e)
        {
            UpdateHealthUI();
        }

        void UpdateHealthUI()
        {
            for (int i = 0; i < HealthPanels.Count; ++i)
            {
                if (i < m_avatar.CurrentHealth)
                {
                    HealthPanels[i].GetComponent<Image>().sprite = m_settings.heartFull;
                }
                else
                {
                    HealthPanels[i].GetComponent<Image>().sprite = m_settings.heartBroken;
                }
            }
        }
    }
}
