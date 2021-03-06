﻿// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RhythMage
{
    public class HUDController : MonoBehaviour
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
        }

        [Zenject.Inject]
        readonly GameDifficulty.Settings m_difficultySettings;

        [Zenject.Inject]
        readonly Settings m_settings;

        [Zenject.Inject]
        AvatarModel m_avatar;

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
        public CanvasGroup portalOverlayImage;
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

        void OnBeat()
        {
            PopulateEnemyList(incomingEnemyTilesAhead);
            if (m_avatar.CurrentCellIndex == m_dungeon.GetCellCount() - 1)
            {
                StartCoroutine(ShowPortalOverlay(portalOverlayImage, 0.25f, 0.15f));
            }
        }

        void OnDungeonReset()
        {
            foreach (var entry in m_enemyData)
            {
                Destroy(entry.notch.gameObject);
            }
            m_enemyData.Clear();
        }

        void OnEnemyCountChanged(int count)
        {
            UpdateEnemyCountUI();
        }

        void OnHealthChanged(AvatarModel avatar, AvatarModel.HealthChangedEventArgs args)
        {
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
                if (i < m_avatar.CurrentHealth)
                {
                    healthImages[i].sprite = m_settings.heartFull;
                }
                else
                {
                    healthImages[i].sprite = m_settings.heartBroken;
                }
            }
        }

        void OnSwipe(GestureHandler.GestureSwipeEventArgs args)
        {
            if (args.Direction == Direction.Left || args.Direction == Direction.Backward)
            {
                leftHand.sprite = m_settings.leftHandNormal;
                rightHand.sprite = m_settings.rightHandAttack;
            }
            else if (args.Direction == Direction.Right || args.Direction == Direction.Forward)
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
                float mag = System.Math.Min(1.0f, elapsedTime / duration);
                color.a = (1.0f - mag) * opacity; // Linear fade out
                target.color = color;
                yield return null;
            }
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
        }

        void PopulateEnemyList(int distanceAhead)
        {
            int cellIndex = m_avatar.CurrentCellIndex + distanceAhead;
            if (cellIndex < m_dungeon.GetCellCount())
            {
                Cell cell = m_dungeon.GetCellAtIndex(cellIndex);
                if (m_dungeon.GetEnemyAtCell(cell, out Enemy enemy))
                {
                    // Add to tracker
                    EnemyData data;
                    data.cellIndex = cellIndex;
                    Image notch = null;
                    if (enemy.EnemyType == EnemyType.Flying)
                    {
                        notch = Instantiate(m_settings.prefabFlyingEnemyNotch);
                    }
                    else if (enemy.EnemyType == EnemyType.Magic)
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
            int currentCellIndex = m_avatar.CurrentCellIndex;
            int indexOffset = enemyData.cellIndex - currentCellIndex;
            double delay = System.Math.Min(1.0f, m_sound.TimeSinceLastBeat() / m_sound.GetBeatLength());
            float timeOffset = indexOffset - System.Convert.ToSingle(delay);
            float timeWindow = m_difficultySettings.maxInputTimeOffBeat * 2.0f;

            //var easeFunc = EasingFunction.GetEasingFunction(m_settings.notchScaleEaseType);
            //float easedMag = easeFunc(0.0f, 1.0f, mag);
            float mag = System.Math.Max(0.0f, (timeWindow - System.Math.Abs(timeOffset)) / timeWindow);
            float scale = 1.0f + mag;

            if (timeOffset > incomingEnemyTilesAhead - 1)
            {
                scale = incomingEnemyTilesAhead - timeOffset;
            }
            else if (timeOffset < 0.0f)
            {
                scale = System.Math.Max(0.0f, 1.0f + timeOffset);
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
                    StartCoroutine(DeathAnimation(m_enemyData[i].notch.transform, Vector2.zero, 0.3f));
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

            Destroy(transform.gameObject);
        }
    }
}
