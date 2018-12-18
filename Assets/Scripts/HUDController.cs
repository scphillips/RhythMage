using System;
using System.Collections;
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

        public List<Image> healthImages;
        public TextMeshProUGUI enemyCounter;
        public GameObject leftHand;
        public GameObject rightHand;

        public Image damageOverlayImage;

        float m_cooldown;

        void Start()
        {
            m_cooldown = 0.0f;

            UpdateHealthUI();
            UpdateEnemyCountUI();

            m_avatar.OnHealthChange += OnHealthChanged;
            m_dungeon.OnEnemyCountChange += OnEnemyCountChanged;
            m_gestureHandler.OnSwipe += OnSwipe;

            var camera = m_camera.Get();
            leftHand.transform.position = camera.ViewportToWorldPoint(new Vector3(0.125f, 0.25f, 0.25f));
            leftHand.transform.forward = camera.transform.forward;
            rightHand.transform.position = camera.ViewportToWorldPoint(new Vector3(0.875f, 0.25f, 0.25f));
            rightHand.transform.forward = camera.transform.forward;
        }

        void OnEnemyCountChanged(object sender, EventArgs e)
        {
            UpdateEnemyCountUI();
        }

        void OnHealthChanged(object sender, EventArgs e)
        {
            var args = (AvatarModel.HealthChangedEventArgs)e;
            if (args.HealthMod < 0)
            {
                StartCoroutine(ShowDamageOverlay(damageOverlayImage, 0.4f, 0.25f));
            }
            UpdateHealthUI();
        }

        void UpdateEnemyCountUI()
        {
            enemyCounter.text = "Kills: " + m_avatar.killCount;
        }

        void UpdateHealthUI()
        {
            for (int i = 0; i < healthImages.Count; ++i)
            {
                if (i < m_avatar.currentHealth)
                {
                    healthImages[i].sprite = m_settings.heartFull;
                }
                else
                {
                    healthImages[i].sprite = m_settings.heartBroken;
                }
            }
        }

        void OnSwipe(object sender, EventArgs e)
        {
            var args = (GestureHandler.GestureSwipeEventArgs)e;
            if (args.Direction == Direction.Left)
            {
                leftHand.GetComponent<SpriteRenderer>().sprite = m_settings.leftHandNormal;
                rightHand.GetComponent<SpriteRenderer>().sprite = m_settings.rightHandAttack;
            }
            else if (args.Direction == Direction.Right)
            {
                leftHand.GetComponent<SpriteRenderer>().sprite = m_settings.leftHandAttack;
                rightHand.GetComponent<SpriteRenderer>().sprite = m_settings.rightHandNormal;
            }
            m_cooldown = m_sound.GetBeatLength();
        }

        void Update()
        {
            m_cooldown -= Time.deltaTime;
            if (m_cooldown <= 0.0f)
            {
                leftHand.GetComponent<SpriteRenderer>().sprite = m_settings.leftHandNormal;
                rightHand.GetComponent<SpriteRenderer>().sprite = m_settings.rightHandNormal;
            }
        }

        IEnumerator ShowDamageOverlay(Image target, float opacity, float duration)
        {
            var color = target.color;

            float elapsedTime = 0.0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float mag = System.Math.Min(1.0f, elapsedTime / duration);
                color.a = (1.0f - mag) * opacity; // Linear fade out
                target.color = color;
                yield return null;
            }
        }
    }
}
