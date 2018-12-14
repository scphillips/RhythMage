using System;
using System.Collections.Generic;
using TMPro;
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
        CameraProvider m_camera;

        [Zenject.Inject]
        DungeonModel m_dungeon;

        public List<GameObject> HealthPanels;
        public TextMeshProUGUI EnemyCounter;
        public GameObject LeftHand;
        public GameObject RightHand;

        void Start()
        {
            UpdateHealthUI();
            UpdateEnemyCountUI();

            m_avatar.OnHealthChange += OnHealthChanged;
            m_dungeon.OnEnemyCountChange += OnEnemyCountChanged;

            var camera = m_camera.Get();
            LeftHand.transform.position = camera.ViewportToWorldPoint(new Vector3(0.25f, 0.14f, 0.25f));
            LeftHand.transform.forward = camera.transform.forward;
            RightHand.transform.position = camera.ViewportToWorldPoint(new Vector3(0.75f, 0.14f, 0.25f));
            RightHand.transform.forward = camera.transform.forward;
        }

        void OnEnemyCountChanged(object sender, EventArgs e)
        {
            UpdateEnemyCountUI();
        }

        void OnHealthChanged(object sender, EventArgs e)
        {
            UpdateHealthUI();
        }

        void UpdateEnemyCountUI()
        {
            EnemyCounter.text = "Enemies: " + m_dungeon.GetEnemyCount();
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
