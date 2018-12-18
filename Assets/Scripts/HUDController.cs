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
        struct EnemyData
        {
            public Image notch;
            public int cellIndex;
        }

        [Serializable]
        public class Settings
        {
            public Sprite heartFull;
            public Sprite heartBroken;

            public Sprite leftHandNormal;
            public Sprite leftHandAttack;
            public Sprite rightHandNormal;
            public Sprite rightHandAttack;

            public Image prefabMagicEnemyNotch;
            public Image prefabMeleeEnemyNotch;
        }

        [Zenject.Inject]
        readonly GameDifficulty.Settings m_difficultySettings;

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
        public Image incomingEnemyDisplay;

        int m_lastSeenCellIndex;
        Dictionary<Enemy, EnemyData> m_enemyNotchImages;
        float m_timeToResetAttackGraphics;

        void Start()
        {
            m_lastSeenCellIndex = 0;
            m_timeToResetAttackGraphics = 0.0f;
            m_enemyNotchImages = new Dictionary<Enemy, EnemyData>();

            UpdateHealthUI();
            UpdateEnemyCountUI();

            m_avatar.OnHealthChange += OnHealthChanged;
            m_dungeon.OnDungeonReset += OnDungeonReset;
            m_dungeon.OnEnemyCountChange += OnEnemyCountChanged;
            m_gestureHandler.OnSwipe += OnSwipe;

            var camera = m_camera.Get();
            leftHand.transform.position = camera.ViewportToWorldPoint(new Vector3(0.125f, 0.25f, 0.25f));
            leftHand.transform.forward = camera.transform.forward;
            rightHand.transform.position = camera.ViewportToWorldPoint(new Vector3(0.875f, 0.25f, 0.25f));
            rightHand.transform.forward = camera.transform.forward;

            PopulateEnemyList();
        }

        void OnDungeonReset(object sender, EventArgs e)
        {
            foreach (var entry in m_enemyNotchImages)
            {
                Destroy(entry.Value.notch.gameObject);
            }
            m_enemyNotchImages.Clear();
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
            m_timeToResetAttackGraphics = m_sound.GetBeatLength();
        }

        void Update()
        {
            m_timeToResetAttackGraphics -= Time.deltaTime;
            if (m_timeToResetAttackGraphics <= 0.0f)
            {
                leftHand.GetComponent<SpriteRenderer>().sprite = m_settings.leftHandNormal;
                rightHand.GetComponent<SpriteRenderer>().sprite = m_settings.rightHandNormal;
            }

            int cellIndex = m_avatar.currentCellIndex;
            foreach (var entry in m_enemyNotchImages)
            {
                UpdateEnemyNotch(entry.Key, entry.Value);
            }
            if (m_lastSeenCellIndex != cellIndex)
            {
                PopulateEnemyList();
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

        void PopulateEnemyList()
        {
            int cellIndex = m_avatar.currentCellIndex;
            Enemy enemy;
            for (int i = 0; i < 3; ++i)
            {
                int currentIndex = cellIndex + i;
                if (currentIndex >= m_dungeon.GetCellCount())
                {
                    break;
                }

                Cell cell = m_dungeon.GetCellAtIndex(currentIndex);
                enemy = m_dungeon.GetEnemyAtCell(cell);
                if (enemy != null && m_enemyNotchImages.ContainsKey(enemy) == false)
                {
                    // Add to tracker
                    EnemyData data;
                    data.cellIndex = currentIndex;
                    Image notch = null;
                    if (enemy.GetEnemyType() == Enemy.EnemyType.Magic)
                    {
                        notch = (Image)Instantiate(m_settings.prefabMagicEnemyNotch);
                    }
                    else if (enemy.GetEnemyType() == Enemy.EnemyType.Melee)
                    {
                        notch = (Image)Instantiate(m_settings.prefabMeleeEnemyNotch);
                    }
                    notch.transform.SetParent(incomingEnemyDisplay.transform, false);

                    data.notch = notch;
                    m_enemyNotchImages.Add(enemy, data);
                    enemy.OnDeathTriggered += OnEnemyDeath;
                    UpdateEnemyNotch(enemy, data);
                }
            }
        }

        void UpdateEnemyNotch(Enemy enemy, EnemyData enemyData)
        {
            int currentCellIndex = m_avatar.currentCellIndex;
            int indexOffset = enemyData.cellIndex - currentCellIndex;
            float delay = m_sound.TimeSinceLastBeat() / m_sound.GetBeatLength();
            float timeOffset = indexOffset - delay;
            float timeWindow = m_difficultySettings.maxInputTimeOffBeat * 2.0f;
            float mag = System.Math.Max(0.0f, (timeWindow - System.Math.Abs(timeOffset)) / timeWindow);
            float scale = 1.0f + mag;

            float xCoordinate = timeOffset * 100.0f;
            enemyData.notch.transform.localPosition = new Vector2(xCoordinate, 0.0f);
            enemyData.notch.transform.localScale = new Vector2(scale, scale);
        }

        void OnEnemyDeath(object sender, EventArgs e)
        {
            var enemy = (Enemy)sender;
            EnemyData data;
            if (m_enemyNotchImages.TryGetValue(enemy, out data))
            {
                StartCoroutine(DeathAnimation(data.notch.transform, 0.0f, 0.3f));
            }
            m_enemyNotchImages.Remove(enemy);
        }

        IEnumerator DeathAnimation(Transform transform, float scale, float duration)
        {
            float elapsedTime = 0.0f;
            float startScale = transform.localScale.x;

            while (elapsedTime < duration)
            {
                elapsedTime = System.Math.Min(elapsedTime + Time.deltaTime, duration);
                float mag = elapsedTime / duration;
                float currentScale = startScale + (scale - startScale) * mag;
                transform.localScale = new Vector3(currentScale, currentScale, currentScale);
                yield return null;
            }

            Destroy(transform.gameObject);
        }
    }
}
