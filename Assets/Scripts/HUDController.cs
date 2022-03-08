// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RhythMage
{
    public class HUDController
    {
        struct EnemyData
        {
            public Enemy enemy;
            public Image notch;
            public int cellIndex;
        }

        [System.Serializable]
        public class Settings
        {
            public Sprite heartFull;
            public Sprite heartBroken;

            public Sprite leftHandNormal;
            public Sprite leftHandAttack;
            public Sprite rightHandNormal;
            public Sprite rightHandAttack;

            public Image prefabFlyingEnemyNotch;
            public Image prefabMagicEnemyNotch;
            public Image prefabMeleeEnemyNotch;

            public EasingFunction.Ease notchScaleEaseType;

            public int incomingEnemyTilesAhead;
        }

        [Zenject.Inject] readonly GameDifficulty.Settings m_difficultySettings;
        [Zenject.Inject] readonly Settings m_settings;
        [Zenject.Inject] GameUIElementProvider m_uiElementProvider;

        private readonly AvatarModel m_avatar;
        private readonly DungeonModel m_dungeon;
        private readonly GestureHandler m_gestureHandler;
        private readonly SoundManager m_sound;
        private readonly UpdateManager m_updateManager;

        List<EnemyData> m_enemyData;
        float m_timeToResetAttackGraphics;

        public HUDController(AvatarModel avatar, DungeonModel dungeon, GestureHandler gestureHandler, SoundManager sound, UpdateManager updateManager)
        {
            m_avatar = avatar;
            m_dungeon = dungeon;
            m_gestureHandler = gestureHandler;
            m_sound = sound;
            m_updateManager = updateManager;

            m_timeToResetAttackGraphics = 0.0f;
            m_enemyData = new List<EnemyData>();

            m_avatar.OnHealthChange += OnHealthChanged;
            m_dungeon.OnDungeonReset += OnDungeonReset;
            m_dungeon.OnEnemyCountChange += OnEnemyCountChanged;
            m_gestureHandler.OnSwipe += OnSwipe;
            m_avatar.OnMove += OnBeat;
            m_updateManager.OnUpdate += Update;
        }

        void OnBeat(AvatarModel avatar)
        {
            PopulateEnemyList(m_settings.incomingEnemyTilesAhead);
            if (avatar.CurrentCellIndex == m_dungeon.GetCellCount() - 1)
            {
                m_uiElementProvider.StartCoroutine(ShowPortalOverlay(m_uiElementProvider.PortalOverlayImage, 0.25f, 0.15f));
            }
        }

        void OnDungeonReset()
        {
            foreach (var entry in m_enemyData)
            {
                Object.Destroy(entry.notch.gameObject);
            }
            m_enemyData.Clear();

            UpdateHealthUI();
            UpdateEnemyCountUI();
        }

        void OnEnemyCountChanged(int count)
        {
            UpdateEnemyCountUI();
        }

        void OnHealthChanged(AvatarModel avatar, AvatarModel.HealthChangedEventArgs args)
        {
            if (args.HealthMod < 0)
            {
                float opacityFrom = 0.4f;
                float opacityTo = avatar.IsAlive ? 0.0f : 0.8f;
                m_uiElementProvider.StartCoroutine(ShowDamageOverlay(m_uiElementProvider.DamageOverlayImage, opacityFrom, opacityTo, 0.25f));
            }
            UpdateHealthUI();
        }

        void UpdateEnemyCountUI()
        {
            m_uiElementProvider.EnemyCounter.text = "Kills: " + m_avatar.killCount;
        }

        void UpdateHealthUI()
        {
            for (int i = 0; i < m_uiElementProvider.HealthImages.Count; ++i)
            {
                Sprite heartSprite = i < m_avatar.CurrentHealth ? m_settings.heartFull : m_settings.heartBroken;
                m_uiElementProvider.HealthImages[i].sprite = heartSprite;
            }
        }

        void OnSwipe(GestureHandler.GestureSwipeEventArgs args)
        {
            if (args.Direction == Direction.Left || args.Direction == Direction.Backward)
            {
                m_uiElementProvider.LeftHand.sprite = m_settings.leftHandNormal;
                m_uiElementProvider.RightHand.sprite = m_settings.rightHandAttack;
            }
            else if (args.Direction == Direction.Right || args.Direction == Direction.Forward)
            {
                m_uiElementProvider.LeftHand.sprite = m_settings.leftHandAttack;
                m_uiElementProvider.RightHand.sprite = m_settings.rightHandNormal;
            }
            m_timeToResetAttackGraphics = System.Convert.ToSingle(m_sound.GetBeatLength());
        }

        private void Update()
        {
            m_timeToResetAttackGraphics -= Time.deltaTime;
            if (m_timeToResetAttackGraphics <= 0.0f)
            {
                m_uiElementProvider.LeftHand.sprite = m_settings.leftHandNormal;
                m_uiElementProvider.RightHand.sprite = m_settings.rightHandNormal;
            }

            if (m_avatar.IsAlive)
            {
                foreach (var entry in m_enemyData)
                {
                    UpdateEnemyNotch(entry);
                }
            }
            else if (m_enemyData.Count > 0)
            {
                for (int i = 0; i < m_enemyData.Count; ++i)
                {
                    m_uiElementProvider.StartCoroutine(DeathAnimation(m_enemyData[i].notch.transform, Vector2.zero, 0.3f));
                }
                m_enemyData.Clear();
            }
        }

        IEnumerator ShowDamageOverlay(Image target, float opacityFrom, float opacityTo, float duration)
        {
            var color = target.color;

            float elapsedTime = 0.0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float mag = System.Math.Min(1.0f, elapsedTime / duration);
                color.a = opacityFrom + mag * (opacityTo - opacityFrom); // Linear fade out
                target.color = color;
                yield return null;
            }
            color.a = opacityTo;
            target.color = color;
        }

        IEnumerator ShowPortalOverlay(CanvasGroup target, float fadeTime, float duration)
        {
            float elapsedTime = 0.0f;
            float totalDuration = fadeTime * 2 + duration;
            while (elapsedTime < totalDuration)
            {
                elapsedTime += Time.deltaTime;
                float mag = 1.0f;
                if (elapsedTime < fadeTime)
                {
                    mag = System.Math.Min(1.0f, elapsedTime / fadeTime);
                }
                else if (elapsedTime >= fadeTime + duration)
                {
                    mag = 1.0f - System.Math.Min(1.0f, (elapsedTime - fadeTime - duration) / fadeTime);
                }
                target.alpha = mag;
                yield return null;
            }
            target.alpha = 0.0f;
        }

        void PopulateEnemyList(int distanceAhead)
        {
            int cellIndex = m_avatar.CurrentCellIndex + distanceAhead;
            if (cellIndex < m_dungeon.GetCellCount())
            {
                Cell cell = m_dungeon.GetPathAtIndex(cellIndex);
                if (m_dungeon.GetEnemyAtCell(cell, out Enemy enemy))
                {
                    // Add to tracker
                    EnemyData data;
                    data.cellIndex = cellIndex;
                    Image notch = null;
                    if (enemy.EnemyType == EnemyType.Flying)
                    {
                        notch = Object.Instantiate(m_settings.prefabFlyingEnemyNotch);
                    }
                    else if (enemy.EnemyType == EnemyType.Magic)
                    {
                        notch = Object.Instantiate(m_settings.prefabMagicEnemyNotch);
                    }
                    else if (enemy.EnemyType == EnemyType.Melee)
                    {
                        notch = Object.Instantiate(m_settings.prefabMeleeEnemyNotch);
                    }
                    notch.transform.SetParent(m_uiElementProvider.IncomingEnemyDisplay.transform, false);

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
            int currentCellIndex = m_avatar.CurrentCellIndex;
            int indexOffset = enemyData.cellIndex - currentCellIndex;
            if (indexOffset == m_settings.incomingEnemyTilesAhead)
            {
                Debug.Log(string.Format("Setting notch scale for enemy {0} from {1} tile{2} away", enemyData.cellIndex, indexOffset, indexOffset == 1 ? "" : "s"));
            }
            double delay = m_sound.GetTotalTime() / m_sound.GetBeatLength() % 1;
            float timeOffset = indexOffset - System.Convert.ToSingle(delay);
            float timeWindow = m_difficultySettings.maxInputTimeOffBeat * 2.0f;

            //var easeFunc = EasingFunction.GetEasingFunction(m_settings.notchScaleEaseType);
            //float easedMag = easeFunc(0.0f, 1.0f, mag);
            float mag = System.Math.Max(0.0f, (timeWindow - System.Math.Abs(timeOffset)) / timeWindow);
            float scale = 1.0f + mag;

            if (timeOffset >= m_settings.incomingEnemyTilesAhead - 1)
            {
                scale = m_settings.incomingEnemyTilesAhead - timeOffset;
            }
            else if (timeOffset < 0.0f)
            {
                scale = System.Math.Max(0.0f, 1.0f + timeOffset);
            }

            if (scale > 0)
            {
                Debug.Log(string.Format("Assigning enemy {0} to scale {1} at time offset {2}", enemyData.cellIndex, scale, timeOffset));
            }
            float xCoordinate = timeOffset * 100.0f;
            enemyData.notch.transform.localPosition = new Vector2(xCoordinate, 0.0f);
            enemyData.notch.transform.localScale = new Vector2(scale, scale);
        }

        void OnEnemyDeath(Enemy enemy)
        {
            for (int i = 0; i < m_enemyData.Count;)
            {
                if (m_enemyData[i].enemy == enemy)
                {
                    m_uiElementProvider.StartCoroutine(DeathAnimation(m_enemyData[i].notch.transform, Vector2.zero, 0.3f));
                    m_enemyData.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }
        }

        IEnumerator DeathAnimation(Transform transform, Vector2 scale, float duration)
        {
            float elapsedTime = 0.0f;
            Vector2 startScale = transform.localScale;

            while (elapsedTime < duration)
            {
                elapsedTime = System.Math.Min(elapsedTime + Time.deltaTime, duration);
                float mag = elapsedTime / duration;
                Vector2 currentScale = startScale + (scale - startScale) * mag;
                transform.localScale = new Vector3(currentScale.x, currentScale.y, 1.0f);
                yield return null;
            }

            Object.Destroy(transform.gameObject);
        }
    }
}
