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
            public Enemy enemy;
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
        public Image leftHand;
        public Image rightHand;

        public Image damageOverlayImage;
        public Image incomingEnemyDisplay;
        public int incomingEnemyTilesAhead = 2;

        List<EnemyData> m_enemyData;
        float m_timeToResetAttackGraphics;

        void Start()
        {
            m_timeToResetAttackGraphics = 0.0f;
            m_enemyData = new List<EnemyData>();

            UpdateHealthUI();
            UpdateEnemyCountUI();

            m_avatar.OnHealthChange += OnHealthChanged;
            m_dungeon.OnDungeonReset += OnDungeonReset;
            m_dungeon.OnEnemyCountChange += OnEnemyCountChanged;
            m_gestureHandler.OnSwipe += OnSwipe;
            m_sound.OnBeat += OnBeat;
        }

        void OnBeat(object sender, EventArgs e)
        {
            PopulateEnemyList(incomingEnemyTilesAhead);
        }

        void OnDungeonReset(object sender, EventArgs e)
        {
            foreach (var entry in m_enemyData)
            {
                Destroy(entry.notch.gameObject);
            }
            m_enemyData.Clear();
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
                leftHand.sprite = m_settings.leftHandNormal;
                rightHand.sprite = m_settings.rightHandAttack;
            }
            else if (args.Direction == Direction.Right)
            {
                leftHand.sprite = m_settings.leftHandAttack;
                rightHand.sprite = m_settings.rightHandNormal;
            }
            m_timeToResetAttackGraphics = System.Convert.ToSingle(m_sound.GetBeatLength());
        }

        void Update()
        {
            m_timeToResetAttackGraphics -= Time.deltaTime;
            if (m_timeToResetAttackGraphics <= 0.0f)
            {
                leftHand.sprite = m_settings.leftHandNormal;
                rightHand.sprite = m_settings.rightHandNormal;
            }
            
            foreach (var entry in m_enemyData)
            {
                UpdateEnemyNotch(entry);
            }
        }

        IEnumerator ShowDamageOverlay(Image target, float opacity, float duration)
        {
            var color = target.color;

            float elapsedTime = 0.0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float mag = Math.Min(1.0f, elapsedTime / duration);
                color.a = (1.0f - mag) * opacity; // Linear fade out
                target.color = color;
                yield return null;
            }
        }

        void PopulateEnemyList(int distanceAhead)
        {
            int cellIndex = m_avatar.currentCellIndex + distanceAhead;
            if (cellIndex < m_dungeon.GetCellCount())
            {
                Cell cell = m_dungeon.GetCellAtIndex(cellIndex);
                Enemy enemy = m_dungeon.GetEnemyAtCell(cell);
                if (enemy != null)
                {
                    // Add to tracker
                    EnemyData data;
                    data.cellIndex = cellIndex;
                    Image notch = null;
                    if (enemy.EnemyType == EnemyType.Magic)
                    {
                        notch = Instantiate(m_settings.prefabMagicEnemyNotch);
                    }
                    else if (enemy.EnemyType == EnemyType.Melee)
                    {
                        notch = Instantiate(m_settings.prefabMeleeEnemyNotch);
                    }
                    notch.transform.SetParent(incomingEnemyDisplay.transform, false);

                    data.enemy = enemy;
                    data.notch = notch;
                    m_enemyData.Add(data);
                    enemy.OnDeathTriggered += OnEnemyDeath;
                    UpdateEnemyNotch(data);
                }
            }
        }

        void UpdateEnemyNotch(EnemyData enemyData)
        {
            int currentCellIndex = m_avatar.currentCellIndex;
            int indexOffset = enemyData.cellIndex - currentCellIndex;
            double delay = m_sound.TimeSinceLastBeat() / m_sound.GetBeatLength();
            float timeOffset = indexOffset - System.Convert.ToSingle(delay);
            float timeWindow = m_difficultySettings.maxInputTimeOffBeat * 2.0f;
            float mag = Math.Max(0.0f, (timeWindow - Math.Abs(timeOffset)) / timeWindow);
            float scale = 1.0f + mag;
            if (timeOffset > incomingEnemyTilesAhead - 1)
            {
                scale = incomingEnemyTilesAhead - timeOffset;
            }
            else if (timeOffset < 0.0f)
            {
                scale = Math.Max(0.0f, 1.0f + timeOffset);
            }

            float xCoordinate = timeOffset * 100.0f;
            enemyData.notch.transform.localPosition = new Vector2(xCoordinate, 0.0f);
            enemyData.notch.transform.localScale = new Vector2(scale, scale);
        }

        void OnEnemyDeath(object sender, EventArgs e)
        {
            var enemy = (Enemy)sender;
            for (int i = 0; i < m_enemyData.Count;)
            {
                if (m_enemyData[i].enemy == enemy)
                {
                    StartCoroutine(DeathAnimation(m_enemyData[i].notch.transform, 0.0f, 0.3f));
                    m_enemyData.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }
        }

        IEnumerator DeathAnimation(Transform transform, float scale, float duration)
        {
            float elapsedTime = 0.0f;
            float startScale = transform.localScale.x;

            while (elapsedTime < duration)
            {
                elapsedTime = Math.Min(elapsedTime + Time.deltaTime, duration);
                float mag = elapsedTime / duration;
                float currentScale = startScale + (scale - startScale) * mag;
                transform.localScale = new Vector3(currentScale, currentScale, currentScale);
                yield return null;
            }

            Destroy(transform.gameObject);
        }
    }
}
