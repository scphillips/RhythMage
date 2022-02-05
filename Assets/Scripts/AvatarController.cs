﻿// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RhythMage
{
    public class AvatarController : MonoBehaviour
    {
        [System.Serializable]
        public class AttackAudioSettings
        {
            public List<AudioClip> HitClips;
            public AudioClip SwipeClip;
        }

        [System.Serializable]
        public class Settings
        {
            public AttackAudioSettings SwipeLeftSettings;
            public AttackAudioSettings SwipeRightSettings;
            public AttackAudioSettings SwipeUpSettings;

            public AudioClip HeartLostClip;
        }

        [Zenject.Inject] readonly Settings m_settings;
        [Zenject.Inject] readonly AudioSource audioSource;
        [Zenject.Inject] readonly AvatarModel m_avatar;
        [Zenject.Inject] readonly GameDifficulty.Settings m_difficultySettings;
        [Zenject.Inject] readonly GestureHandler m_gestureHandler;
        [Zenject.Inject] readonly DungeonModel m_dungeon;
        [Zenject.Inject] readonly LevelBuilder m_levelBuilder;
        [Zenject.Inject] readonly RandomNumberProvider m_rng;
        [Zenject.Inject] readonly SoundManager m_sound;

        int m_lastCheckedIndex;

        void Start()
        {
            m_lastCheckedIndex = 0;

            m_sound.OnBeat += OnBeat;
            m_gestureHandler.OnSwipe += OnSwipe;
        }

        void Update()
        {
            if (m_lastCheckedIndex != m_avatar.CurrentCellIndex
                && m_sound.TimeSinceLastBeat() > m_sound.GetMaxTimeOffBeat())
            {
                // Beat finished, check for enemy collisions
                Cell currentCell = m_dungeon.GetCellAtIndex(m_avatar.CurrentCellIndex);
                if (m_dungeon.HasEnemyAtCell(currentCell))
                {
                    // Take damage
                    m_avatar.TakeDamage();
                    audioSource.PlayOneShot(m_settings.HeartLostClip);
                }

                m_lastCheckedIndex = m_avatar.CurrentCellIndex;
            }
        }

        void OnBeat()
        {
            int cellIndex = m_avatar.CurrentCellIndex + 1;

            if (cellIndex >= m_dungeon.GetCellCount())
            {
                cellIndex = 0;
                m_levelBuilder.BuildLevel(m_dungeon, transform);
                Cell currentCell = m_dungeon.GetCellAtIndex(cellIndex);
                transform.localPosition = new Vector3(currentCell.x, 0.0f, currentCell.y);
                transform.localRotation = Quaternion.AngleAxis(0.0f, Vector3.up);
            }
            else
            {
                Cell currentCell = m_dungeon.GetCellAtIndex(cellIndex);
                float targetAngle = transform.localEulerAngles.y;
                if (cellIndex < m_dungeon.GetCellCount() - 1)
                {
                    Cell nextCell = m_dungeon.GetCellAtIndex(cellIndex + 1);
                    CoordinateOffset offset = CoordinateOffset.Create(nextCell.x - currentCell.x, nextCell.y - currentCell.y);
                    Direction direction = Direction.None;
                    foreach (var entry in Defs.facings)
                    {
                        if (entry.Value == offset)
                        {
                            direction = entry.Key;
                            break;
                        }
                    }

                    if (direction != Direction.None)
                    {
                        targetAngle = 90.0f * (int)direction;
                    }
                }
                StartCoroutine(MoveTo(transform, new Vector3(currentCell.x, 0.0f, currentCell.y), targetAngle, 0.125f));
            }
            
            m_avatar.CurrentCellIndex = cellIndex;
        }

        void OnSwipe(GestureHandler.GestureSwipeEventArgs args)
        {
            if (args.Direction == Direction.Left)
            {
                audioSource.PlayOneShot(m_settings.SwipeLeftSettings.SwipeClip);
            }
            else if (args.Direction == Direction.Right)
            {
                audioSource.PlayOneShot(m_settings.SwipeRightSettings.SwipeClip);
            }
            else if (args.Direction == Direction.Forward)
            {
                audioSource.PlayOneShot(m_settings.SwipeUpSettings.SwipeClip);
            }

            if (m_sound.TimeOffBeat() <= m_sound.GetMaxTimeOffBeat())
            {
                // Valid swipe, test enemy type
                int targetCellIndex = m_avatar.CurrentCellIndex;
                var targetCell = m_dungeon.GetCellAtIndex(targetCellIndex);
                if (targetCellIndex < m_dungeon.GetCellCount() - 1
                    && (m_sound.WillBeatThisFrame()
                        || m_sound.TimeToNextBeat() <= m_sound.GetMaxTimeOffBeat()))
                {
                    ++targetCellIndex;
                    targetCell = m_dungeon.GetCellAtIndex(targetCellIndex);
                }

                if (m_dungeon.GetEnemyAtCell(targetCell, out Enemy enemy)
                    && ((enemy.EnemyType == EnemyType.Flying && args.Direction == Direction.Forward)
                        || (enemy.EnemyType == EnemyType.Magic && args.Direction == Direction.Right)
                        || (enemy.EnemyType == EnemyType.Melee && args.Direction == Direction.Left)))
                {
                    // Valid combination, destroy the enemy
                    ++m_avatar.killCount;
                    enemy.Die();
                    m_dungeon.RemoveEnemyAtCell(targetCell);

                    Debug.Log("Swipe " + args.Direction.ToString());
                    if (args.Direction == Direction.Left)
                    {
                        int index = m_rng.Next(m_settings.SwipeLeftSettings.HitClips.Count);
                        audioSource.PlayOneShot(m_settings.SwipeLeftSettings.HitClips[index]);
                    }
                    else if (args.Direction == Direction.Right)
                    {
                        int index = m_rng.Next(m_settings.SwipeRightSettings.HitClips.Count);
                        audioSource.PlayOneShot(m_settings.SwipeRightSettings.HitClips[index]);
                    }
                    else if (args.Direction == Direction.Forward)
                    {
                        int index = m_rng.Next(m_settings.SwipeUpSettings.HitClips.Count);
                        audioSource.PlayOneShot(m_settings.SwipeUpSettings.HitClips[index]);
                    }
                }
            }
        }

        IEnumerator MoveTo(Transform transform, Vector3 target, float angle, float duration)
        {
            Vector3 startPosition = transform.localPosition;
            Vector3 offset = target - startPosition;

            Quaternion startRotation = transform.localRotation;
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.up);

            float elapsedTime = 0.0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float mag = System.Math.Min(1.0f, elapsedTime / duration);
                transform.localPosition = startPosition + offset * mag;
                transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, mag);
                yield return null;
            }

            transform.localPosition = target;
            transform.localRotation = targetRotation;
        }
    }
}
