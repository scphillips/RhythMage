﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Outplay.RhythMage
{
    public class AvatarController : MonoBehaviour
    {
        [System.Serializable]
        public class Settings
        {
            public List<AudioClip> LeftHitClips;
            public AudioClip LeftSwipeClip;
            public List<AudioClip> RightHitClips;
            public AudioClip RightSwipeClip;

            public AudioClip HeartLostClip;
        }

        [Zenject.Inject]
        readonly Settings m_settings;

        [Zenject.Inject]
        readonly GameDifficulty.Settings m_difficultySettings;

        [Zenject.Inject]
        DungeonBuilder m_dungeonBuilder;

        [Zenject.Inject]
        DungeonModel m_dungeon;

        [Zenject.Inject]
        AvatarModel m_avatar;

        [Zenject.Inject]
        GestureHandler m_gestureHandler;

        [Zenject.Inject]
        RandomNumberProvider m_rng;

        [Zenject.Inject]
        SoundManager m_sound;

        [Zenject.Inject]
        AudioSource audioSource;

        int m_lastCheckedIndex;

        void Start()
        {
            m_lastCheckedIndex = 0;

            m_sound.OnBeat += OnBeat;
            m_gestureHandler.OnSwipe += OnSwipe;
        }

        void Update()
        {
            if (m_lastCheckedIndex != m_avatar.currentCellIndex
                && m_sound.TimeSinceLastBeat() > m_difficultySettings.maxInputTimeOffBeat)
            {
                // Beat finished, check for enemy collisions
                Cell currentCell = m_dungeon.GetCellAtIndex(m_avatar.currentCellIndex);
                if (m_dungeon.HasEnemyAtCell(currentCell))
                {
                    // Take damage
                    m_avatar.TakeDamage();
                    audioSource.PlayOneShot(m_settings.HeartLostClip);
                }

                m_lastCheckedIndex = m_avatar.currentCellIndex;
            }
        }

        void OnBeat()
        {
            int cellIndex = m_avatar.currentCellIndex + 1;

            if (cellIndex == m_dungeon.GetCellCount())
            {
                cellIndex = 0;
                m_dungeonBuilder.BuildDungeon();
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
            
            m_avatar.currentCellIndex = cellIndex;
        }

        void OnSwipe(object sender, System.EventArgs e)
        {
            var args = (GestureHandler.GestureSwipeEventArgs)e;
            if (args.Direction == Direction.Left)
            {
                audioSource.PlayOneShot(m_settings.LeftSwipeClip);
            }
            else if (args.Direction == Direction.Right)
            {
                audioSource.PlayOneShot(m_settings.RightSwipeClip);
            }
            
            if (m_sound.TimeOffBeat() <= m_difficultySettings.maxInputTimeOffBeat)
            {
                // Valid swipe, test enemy type
                int targetCellIndex = m_avatar.currentCellIndex;
                var targetCell = m_dungeon.GetCellAtIndex(targetCellIndex);
                if ((m_sound.WillBeatThisFrame()
                    || m_sound.TimeToNextBeat() <= m_difficultySettings.maxInputTimeOffBeat)
                        && targetCellIndex < m_dungeon.GetCellCount() - 1)
                {
                    ++targetCellIndex;
                    targetCell = m_dungeon.GetCellAtIndex(targetCellIndex);
                }

                if (m_dungeon.GetEnemyAtCell(targetCell, out Enemy enemy))
                {
                    if ((enemy.EnemyType == EnemyType.Magic && args.Direction == Direction.Right)
                        || (enemy.EnemyType == EnemyType.Melee && args.Direction == Direction.Left))
                    {
                        // Valid combination, destroy the enemy
                        ++m_avatar.killCount;
                        enemy.Die();
                        m_dungeon.RemoveEnemyAtCell(targetCell);

                        if (args.Direction == Direction.Left)
                        {
                            int index = m_rng.Next(m_settings.LeftHitClips.Count);
                            audioSource.PlayOneShot(m_settings.LeftHitClips[index]);
                        }
                        else if (args.Direction == Direction.Right)
                        {
                            int index = m_rng.Next(m_settings.RightHitClips.Count);
                            audioSource.PlayOneShot(m_settings.RightHitClips[index]);
                        }
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
