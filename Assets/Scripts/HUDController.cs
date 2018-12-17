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

            public Sprite leftHandNormal;
            public Sprite leftHandAttack;
            public Sprite rightHandNormal;
            public Sprite rightHandAttack;
        }

        [Zenject.Inject]
        readonly Settings m_settings;

        [Zenject.Inject]
        AvatarModel m_avatar;

        [Zenject.Inject]
        CameraProvider m_camera;

        [Zenject.Inject]
        DungeonModel m_dungeon;

        [Zenject.Inject]
        GestureHandler m_gestureHandler;

        [Zenject.Inject]
        SoundManager m_sound;

        public List<GameObject> HealthPanels;
        public TextMeshProUGUI EnemyCounter;
        public GameObject LeftHand;
        public GameObject RightHand;

        List<Image> m_healthImages;

        float m_cooldown;

        void Start()
        {
            m_cooldown = 0.0f;

            m_healthImages = new List<Image>();
            for (int i = 0; i < HealthPanels.Count; ++i)
            {
                m_healthImages.Add(HealthPanels[i].GetComponent<Image>());
            }
            UpdateHealthUI();
            UpdateEnemyCountUI();

            m_avatar.OnHealthChange += OnHealthChanged;
            m_dungeon.OnEnemyCountChange += OnEnemyCountChanged;
            m_gestureHandler.OnSwipe += OnSwipe;

            var camera = m_camera.Get();
            LeftHand.transform.position = camera.ViewportToWorldPoint(new Vector3(0.125f, 0.25f, 0.25f));
            LeftHand.transform.forward = camera.transform.forward;
            RightHand.transform.position = camera.ViewportToWorldPoint(new Vector3(0.875f, 0.25f, 0.25f));
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
            EnemyCounter.text = "Kills: " + m_avatar.KillCount;
        }

        void UpdateHealthUI()
        {
            for (int i = 0; i < HealthPanels.Count; ++i)
            {
                if (i < m_avatar.CurrentHealth)
                {
                    m_healthImages[i].sprite = m_settings.heartFull;
                }
                else
                {
                    m_healthImages[i].sprite = m_settings.heartBroken;
                }
            }
        }

        void OnSwipe(object sender, EventArgs e)
        {
            var args = (GestureHandler.GestureSwipeEventArgs)e;
            if (args.Direction == Defs.Direction.Left)
            {
                LeftHand.GetComponent<SpriteRenderer>().sprite = m_settings.leftHandNormal;
                RightHand.GetComponent<SpriteRenderer>().sprite = m_settings.rightHandAttack;
            }
            else if (args.Direction == Defs.Direction.Right)
            {
                LeftHand.GetComponent<SpriteRenderer>().sprite = m_settings.leftHandAttack;
                RightHand.GetComponent<SpriteRenderer>().sprite = m_settings.rightHandNormal;
            }
            m_cooldown = m_sound.GetBeatLength();
        }

        void Update()
        {
            m_cooldown -= Time.deltaTime;
            if (m_cooldown <= 0.0f)
            {
                LeftHand.GetComponent<SpriteRenderer>().sprite = m_settings.leftHandNormal;
                RightHand.GetComponent<SpriteRenderer>().sprite = m_settings.rightHandNormal;
            }
        }
    }
}
