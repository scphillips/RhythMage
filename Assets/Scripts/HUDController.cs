using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Outplay.RhythMage
{
    public class HUDController : MonoBehaviour
    {
        [Serializable]
        public class Settings
        {
            public Sprite heartFull;
            public Sprite heartBroken;
        }

        [Zenject.Inject]
        readonly Settings m_settings;

        [Zenject.Inject]
        AvatarModel m_avatar;

        [Zenject.Inject]
        public CameraProvider m_camera;

        public List<GameObject> HealthPanels;
        public GameObject LeftHand;
        public GameObject RightHand;

        void Start()
        {
            UpdateHealthUI();
            m_avatar.OnHealthChange += OnHealthChanged;

            var camera = m_camera.Get();
            LeftHand.transform.position = camera.ViewportToWorldPoint(new Vector3(0.25f, 0.14f, 0.25f));
            RightHand.transform.position = camera.ViewportToWorldPoint(new Vector3(0.75f, 0.14f, 0.25f));
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
