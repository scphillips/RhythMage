﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Outplay.RhythMage
{
    public class AvatarController : MonoBehaviour
    {
        [Serializable]
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

        public AudioSource audioSource;

        void Start()
        {
            m_sound.OnBeat += OnBeat;

            m_gestureHandler.OnSwipe += OnSwipe;
        }

        void OnBeat(object sender, EventArgs e)
        {
            int cellIndex = m_avatar.currentCellIndex;
            cellIndex = (cellIndex + 1) % m_dungeon.GetCellCount();
            m_avatar.currentCellIndex = cellIndex;

            Cell currentCell = m_dungeon.GetCellAtIndex(cellIndex);
            if (cellIndex == 0)
            {
                transform.localPosition = new Vector3(currentCell.x, 0.25f, currentCell.y);
                m_dungeonBuilder.BuildDungeon();
            }

            float targetAngle = transform.localRotation.eulerAngles.y;
            if (cellIndex < m_dungeon.GetCellCount() - 1)
            {
                Cell nextCell = m_dungeon.GetCellAtIndex(cellIndex + 1);
                CoordinateOffset offset = CoordinateOffset.Create(nextCell.x - currentCell.x, nextCell.y - currentCell.y);
                Direction direction = Direction.Forward;
                foreach (var entry in Defs.facings)
                {
                    if (entry.Value == offset)
                    {
                        direction = entry.Key;
                        break;
                    }
                }
                
                switch (direction)
                {
                    case Direction.Forward:
                        targetAngle = 0.0f;
                        break;
                    case Direction.Right:
                        targetAngle = 90.0f;
                        break;
                    case Direction.Backward:
                        targetAngle = 180.0f;
                        break;
                    case Direction.Left:
                        targetAngle = 270.0f;
                        break;
                }
            }
            StartCoroutine(MoveTo(transform, new Vector3(currentCell.x, 0.25f, currentCell.y), targetAngle, 0.125f));
        }

        void OnSwipe(object sender, EventArgs e)
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
                if (m_sound.TimeToNextBeat() < m_sound.TimeSinceLastBeat())
                {
                    ++targetCellIndex;
                }

                var currentCell = m_dungeon.GetCellAtIndex(targetCellIndex);
                if (m_dungeon.HasEnemyAtCell(currentCell))
                {
                    var enemy = m_dungeon.GetEnemyAtCell(currentCell);
                    if ((enemy.EnemyType == EnemyType.Magic && args.Direction == Direction.Right)
                        || (enemy.EnemyType == EnemyType.Melee && args.Direction == Direction.Left))
                    {
                        // Valid combination, destroy the enemy
                        ++m_avatar.killCount;
                        enemy.Die();
                        m_dungeon.RemoveEnemyAtCell(currentCell);

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
                elapsedTime = elapsedTime + Time.deltaTime;
                float mag = Math.Min(1.0f, elapsedTime / duration);
                transform.localPosition = startPosition + offset * mag;
                transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, mag);
                yield return null;
            }

            // Finished moving, check for enemy collisions
            Cell currentCell = m_dungeon.GetCellAtIndex(m_avatar.currentCellIndex);
            if (m_dungeon.HasEnemyAtCell(currentCell))
            {
                // Take damage
                m_avatar.TakeDamage();
                audioSource.PlayOneShot(m_settings.HeartLostClip);
            }
        }
    }
}
