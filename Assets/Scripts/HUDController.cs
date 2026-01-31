// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using System.Collections;
using System.Collections.Generic;
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
            
            public Image prefabEnemyBatNotch;
            public Image prefabEnemyGoblinNotch;
            public Image prefabEnemyRatNotch;
            public Image prefabEnemySlimeNotch;

            public EasingFunction.Ease notchScaleEaseType;

            public int incomingEnemyTilesAhead;
        }

        public GameUIElementProvider m_uiElementProvider;

        GameDifficulty.Settings m_difficultySettings;
        Settings m_settings;
        AvatarModel m_avatar;
        DungeonModel m_dungeon;
        SoundManager m_sound;

        List<EnemyData> m_enemyData;
        float m_timeToResetAttackGraphics;

        public void Start()
        {
            m_settings = Utils.GetOrCreateGameSettings().HUDControllerSettings;
            m_difficultySettings = Utils.GetOrCreateGameSettings().GameDifficultySettings;
            m_avatar = Utils.FindAvatarModel();
            m_dungeon = Utils.FindDungeonModel();
            m_sound = Utils.FindSoundManager();

            m_timeToResetAttackGraphics = 0.0f;
            m_enemyData = new List<EnemyData>();
            m_uiElementProvider = GetComponent<GameUIElementProvider>();

            m_avatar.OnHealthChange += OnHealthChanged;
            m_dungeon.OnDungeonReset += OnDungeonReset;
            m_dungeon.OnEnemyCountChange += OnEnemyCountChanged;
            m_avatar.OnMove += OnAvatarMove;
            Utils.GetOrCreateGestureHandler().OnSwipe += OnSwipe;
            Utils.FindUpdateManager().OnUpdate += Update;
        }

        void OnDestroy()
        {
            m_avatar.OnHealthChange -= OnHealthChanged;
            m_avatar.OnMove -= OnAvatarMove;
            GestureHandler gestureHandler = Utils.FindGestureHandler();
            if (gestureHandler != null)
            {
                gestureHandler.OnSwipe -= OnSwipe;
            }
        }

        void OnAvatarMove(AvatarModel avatar)
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
            if (!m_avatar.IsAlive)
            {
                return;
            }

            if (args.Direction == Direction.Left || args.Direction == Direction.Down)
            {
                m_uiElementProvider.LeftHand.sprite = m_settings.leftHandNormal;
                m_uiElementProvider.RightHand.sprite = m_settings.rightHandAttack;
            }
            else if (args.Direction == Direction.Right || args.Direction == Direction.Up)
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
                    if (enemy.EnemyType == EnemyType.Bat)
                    {
                        notch = Object.Instantiate(m_settings.prefabEnemyBatNotch);
                    }
                    else if (enemy.EnemyType == EnemyType.Goblin)
                    {
                        notch = Object.Instantiate(m_settings.prefabEnemyGoblinNotch);
                    }
                    else if (enemy.EnemyType == EnemyType.Rat)
                    {
                        notch = Object.Instantiate(m_settings.prefabEnemyRatNotch);
                    }
                    else if (enemy.EnemyType == EnemyType.Slime)
                    {
                        notch = Object.Instantiate(m_settings.prefabEnemySlimeNotch);
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
            double current = m_sound.GetTotalTime() / m_sound.GetBeatLength();
            double delay = current - System.Math.Truncate(current);
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
