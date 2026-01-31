// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
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
            public AttackAudioSettings SwipeDownSettings;

            public AudioClip HeartLostClip;
            public AudioClip DeathClip;
        }

        AvatarModel m_avatar;
        DungeonModel m_dungeon;
        SoundManager m_sound;
        GameSettings m_settings;

        int m_lastCheckedIndex;

        void Start()
        {
            m_lastCheckedIndex = 0;

            m_settings = Utils.FindGameSettings();
            m_avatar = Utils.FindAvatarModel();
            m_dungeon = Utils.FindDungeonModel();
            m_dungeon.OnPathChanged += OnPathChanged;
            m_sound = Utils.FindSoundManager();
            m_sound.OnBeat += OnBeat;
            Utils.FindGameStateManager().gestureHandler.OnSwipe += OnSwipe;

            OnPathChanged();
        }

        void OnDestroy()
        {
            m_sound.OnBeat -= OnBeat;
            Utils.FindGameStateManager().gestureHandler.OnSwipe -= OnSwipe;
        }

        void Update()
        {
            if (m_lastCheckedIndex != m_avatar.CurrentCellIndex
                && m_sound.TimeSinceLastBeat() > m_sound.GetMaxTimeOffBeat())
            {
                // Beat finished, check for enemy collisions
                Cell currentCell = m_dungeon.GetPathAtIndex(m_avatar.CurrentCellIndex);
                if (m_dungeon.HasEnemyAtCell(currentCell))
                {
                    // Take damage
                    m_avatar.TakeDamage();
                    AudioClip damageClip = m_avatar.CurrentHealth <= 0 ? m_settings.AvatarControllerSettings.DeathClip : m_settings.AvatarControllerSettings.HeartLostClip;
                    m_sound.PlayOneShot(damageClip);
                }

                m_lastCheckedIndex = m_avatar.CurrentCellIndex;
            }
        }

        void OnPathChanged()
        {
            Cell currentCell = m_dungeon.GetPathAtIndex(0);
            Cell nextCell = m_dungeon.GetPathAtIndex(1);
            CoordinateOffset offset = CoordinateOffset.Distance(currentCell, nextCell);
            Direction direction = Defs.GetOffsetDirection(offset);

            float targetAngle = transform.localEulerAngles.y;
            if (direction != Direction.None)
            {
                targetAngle = 90.0f * (int)direction;
            }
            transform.localPosition = new Vector3(currentCell.x, 0.0f, currentCell.y);
            transform.localRotation = Quaternion.AngleAxis(targetAngle, Vector3.up);
        }

        void OnBeat()
        {
            if (m_avatar.IsAlive)
            {
                int cellIndex = m_avatar.CurrentCellIndex + 1;

                bool nextLevel = cellIndex >= m_dungeon.GetCellCount();
                if (nextLevel)
                {
                    cellIndex = 0;
                    Utils.FindGameSceneController().BuildLevel();
                }
                
                Cell currentCell = m_dungeon.GetPathAtIndex(cellIndex);
                float targetAngle = transform.localEulerAngles.y;
                if (cellIndex < m_dungeon.GetCellCount() - 1)
                {
                    Cell nextCell = m_dungeon.GetPathAtIndex(cellIndex + 1);
                    CoordinateOffset offset = CoordinateOffset.Create(nextCell.x - currentCell.x, nextCell.y - currentCell.y);
                    Direction direction = Defs.GetOffsetDirection(offset);
                    targetAngle = Defs.DirectionToAngle(direction);
                }

                if (nextLevel)
                {
                    transform.localPosition = new Vector3(currentCell.x, 0.0f, currentCell.y);
                    transform.localRotation = Quaternion.AngleAxis(targetAngle, Vector3.up);
                }
                else
                {
                    StartCoroutine(MoveTo(transform, new Vector3(currentCell.x, 0.0f, currentCell.y), targetAngle, 0.1875f));
                }

                m_avatar.CurrentCellIndex = cellIndex;
            }
        }

        void OnSwipe(GestureHandler.GestureSwipeEventArgs args)
        {
            if (!m_avatar.IsAlive)
            {
                return;
            }

            if (args.Direction == Direction.Left)
            {
                m_sound.PlayOneShot(m_settings.AvatarControllerSettings.SwipeLeftSettings.SwipeClip);
            }
            else if (args.Direction == Direction.Right)
            {
                m_sound.PlayOneShot(m_settings.AvatarControllerSettings.SwipeRightSettings.SwipeClip);
            }
            else if (args.Direction == Direction.Up)
            {
                m_sound.PlayOneShot(m_settings.AvatarControllerSettings.SwipeUpSettings.SwipeClip);
            }
            else if (args.Direction == Direction.Down)
            {
                m_sound.PlayOneShot(m_settings.AvatarControllerSettings.SwipeDownSettings.SwipeClip);
            }

            if (m_sound.TimeOffBeat() <= m_sound.GetMaxTimeOffBeat())
            {
                // Valid swipe, test enemy type
                int targetCellIndex = m_avatar.CurrentCellIndex;
                var targetCell = m_dungeon.GetPathAtIndex(targetCellIndex);
                if (targetCellIndex < m_dungeon.GetCellCount() - 1
                    && (m_sound.WillBeatThisFrame()
                        || m_sound.TimeToNextBeat() <= m_sound.GetMaxTimeOffBeat()))
                {
                    ++targetCellIndex;
                    targetCell = m_dungeon.GetPathAtIndex(targetCellIndex);
                }

                if (m_dungeon.GetEnemyAtCell(targetCell, out Enemy enemy)
                    && ((enemy.EnemyType == EnemyType.Bat && args.Direction == Direction.Up)
                        || (enemy.EnemyType == EnemyType.Goblin && args.Direction == Direction.Left)
                        || (enemy.EnemyType == EnemyType.Rat && args.Direction == Direction.Down)
                        || (enemy.EnemyType == EnemyType.Slime && args.Direction == Direction.Right)))
                {
                    // Valid combination, destroy the enemy
                    ++m_avatar.killCount;
                    enemy.Die();
                    m_dungeon.RemoveEnemyAtCell(targetCell);
                    RandomNumberProvider rng = Utils.GetRng();
                    if (args.Direction == Direction.Left)
                    {
                        int index = rng.Next(m_settings.AvatarControllerSettings.SwipeLeftSettings.HitClips.Count);
                        m_sound.PlayOneShot(m_settings.AvatarControllerSettings.SwipeLeftSettings.HitClips[index]);
                    }
                    else if (args.Direction == Direction.Right)
                    {
                        int index = rng.Next(m_settings.AvatarControllerSettings.SwipeRightSettings.HitClips.Count);
                        m_sound.PlayOneShot(m_settings.AvatarControllerSettings.SwipeRightSettings.HitClips[index]);
                    }
                    else if (args.Direction == Direction.Up)
                    {
                        int index = rng.Next(m_settings.AvatarControllerSettings.SwipeUpSettings.HitClips.Count);
                        m_sound.PlayOneShot(m_settings.AvatarControllerSettings.SwipeUpSettings.HitClips[index]);
                    }
                    else if (args.Direction == Direction.Down)
                    {
                        int index = rng.Next(m_settings.AvatarControllerSettings.SwipeDownSettings.HitClips.Count);
                        m_sound.PlayOneShot(m_settings.AvatarControllerSettings.SwipeDownSettings.HitClips[index]);
                    }
                }
                else if (enemy != null)
                {
                    Debug.Log(string.Format("Wrong attack type {0} versus {1}", args.Direction, enemy.EnemyType));
                }
                else
                {
                    Debug.Log(string.Format("No enemy found at {0}", targetCell));
                }
            }
            else
            {
                Debug.Log(string.Format("Missed beat by {0}/{1}", m_sound.TimeOffBeat(), m_sound.GetMaxTimeOffBeat()));
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
                float phase = elapsedTime / duration;
                float rotPhaseDelay = 0.5f;
                float magRot = Defs.Clamp(phase * (1.0f + rotPhaseDelay) - rotPhaseDelay, 0.0f, 1.0f);
                transform.localPosition = startPosition + offset * phase;
                transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, magRot);
                yield return null;
            }

            transform.localPosition = target;
            transform.localRotation = targetRotation;
        }
    }
}
