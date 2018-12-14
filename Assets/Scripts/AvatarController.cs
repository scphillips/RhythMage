﻿using System;
using System.Collections;
using UnityEngine;

namespace Outplay.RhythMage
{
    public class AvatarController : MonoBehaviour
    {
        [Zenject.Inject]
        DungeonModel m_dungeon;

        [Zenject.Inject]
        AvatarModel m_avatar;

        [Zenject.Inject]
        GestureHandler m_gestureHandler;

        [Zenject.Inject]
        SoundManager m_sound;
        
        int m_currentCellIndex;

        void Start()
        {
            m_sound.OnBeat += OnBeat;
            m_currentCellIndex = 0;

            m_gestureHandler.OnSwipe += OnSwipe;
        }

        void OnBeat(object sender, EventArgs e)
        {
            ++m_currentCellIndex;
            if (m_currentCellIndex >= m_dungeon.GetCellCount() - 2)
            {
                m_currentCellIndex = 0;
            }

            Cell currentCell = m_dungeon.GetCellAtIndex(m_currentCellIndex);
            
            StartCoroutine(MoveTo(transform, new Vector3(currentCell.x, 0.25f, currentCell.y), 0.125f));

            Cell nextCell = m_dungeon.GetCellAtIndex(m_currentCellIndex + 1);
            CoordinateOffset offset = CoordinateOffset.Create(nextCell.x - currentCell.x, nextCell.y - currentCell.y);
            Defs.Direction direction = Defs.Direction.Forwards;
            foreach (var entry in Defs.Facings)
            {
                if (entry.Value == offset)
                {
                    direction = entry.Key;
                    break;
                }
            }

            float targetAngle = 0.0f;
            switch (direction)
            {
                case Defs.Direction.Forwards:
                    targetAngle = 0.0f;
                    break;
                case Defs.Direction.Right:
                    targetAngle = 90.0f;
                    break;
                case Defs.Direction.Backwards:
                    targetAngle = 180.0f;
                    break;
                case Defs.Direction.Left:
                    targetAngle = 270.0f;
                    break;
            }

            StartCoroutine(RotateTo(transform, targetAngle, 0.125f));
        }

        void OnSwipe(object sender, EventArgs e)
        {
            var args = (GestureHandler.GestureSwipeEventArgs)e;
            Debug.Log("Swipe: " + args.Direction.ToString());
            Debug.Log("Time off beat: " + m_sound.TimeOffBeat());
            if (m_sound.TimeOffBeat() < 0.2f)
            {
                // Valid swipe, test enemy type
                var currentCell = m_dungeon.GetCellAtIndex(m_currentCellIndex);
                if (m_dungeon.HasEnemyAtCell(currentCell))
                {
                    var enemy = m_dungeon.GetEnemyAtCell(currentCell);
                    if (enemy.GetEnemyType() == Enemy.EnemyType.Magic && args.Direction == Defs.Direction.Left
                        || enemy.GetEnemyType() == Enemy.EnemyType.Melee && args.Direction == Defs.Direction.Right)
                    {
                        // Valid combination, destroy the enemy
                        m_dungeon.RemoveEnemyAtCell(currentCell);
                    }
                }
            }
        }

        IEnumerator MoveTo(Transform transform, Vector3 target, float duration)
        {
            float elapsedTime = 0.0f;
            Vector3 startPosition = transform.localPosition;
            Vector3 offset = target - startPosition;

            while (elapsedTime < duration)
            {
                elapsedTime = System.Math.Min(elapsedTime + Time.deltaTime, duration);
                float mag = elapsedTime / duration;
                transform.localPosition = startPosition + offset * mag;
                yield return null;
            }

            // Finished moving, check for enemy collisions
            Cell currentCell = m_dungeon.GetCellAtIndex(m_currentCellIndex);
            if (m_dungeon.HasEnemyAtCell(currentCell))
            {
                // Take damage
                m_avatar.TakeDamage();
            }
        }

        IEnumerator RotateTo(Transform transform, float angle, float duration)
        {
            float elapsedTime = 0.0f;
            Quaternion startRotation = transform.localRotation;
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.up);

            while (elapsedTime < duration)
            {
                elapsedTime = System.Math.Min(elapsedTime + Time.deltaTime, duration);
                float mag = elapsedTime / duration;
                transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, mag);
                yield return null;
            }
        }
    }
}
