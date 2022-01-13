// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using System.Collections;
using UnityEngine;

namespace RhythMage
{
    public class Enemy : MonoBehaviour
    {
        public class Factory : Zenject.PlaceholderFactory<Cell, EnemyType, Enemy>
        {
        }
        
        [Zenject.Inject]
        readonly CameraProvider cameraProvider;

        public GameObject flying;
        public GameObject magic;
        public GameObject melee;

        public event System.Action<Enemy> OnDeathTriggered;
        
        EnemyType m_type;

        public EnemyType EnemyType
        {
            get
            {
                return m_type;
            }
            set
            {
                m_type = value;
                flying.SetActive(m_type == EnemyType.Flying);
                magic.SetActive(m_type == EnemyType.Magic);
                melee.SetActive(m_type == EnemyType.Melee);
            }
        }

        GameObject ActiveSprite
        {
            get
            {
                switch (EnemyType)
                {
                    case EnemyType.Flying: return flying;
                    case EnemyType.Magic: return magic;
                    case EnemyType.Melee: return melee;
                    default: return null;
                }
            }
        }

        [Zenject.Inject]
        public void Construct(Cell cell, EnemyType type)
        {
            Reset(cell);
            EnemyType = type;
        }

        public void Reset(Cell cell)
        {
            transform.localPosition = new Vector3(cell.x, 0.0f, cell.y);
            ActiveSprite.transform.localRotation = Quaternion.Euler(0, 0, 0);
            ActiveSprite.transform.localScale = Vector3.one;
        }

        public void SetPosition(Cell cell)
        {
            transform.localPosition = new Vector3(cell.x, 0.0f, cell.y);
        }

        public void Die()
        {
            OnDeathTriggered?.Invoke(this);
            transform.SetParent(cameraProvider.transform, true);
            int direction = (m_type == EnemyType.Magic) ? -1 : 1;
            StartCoroutine(DeathAnimation(ActiveSprite.transform, 360.0f * direction, 0.3f));
        }

        IEnumerator DeathAnimation(Transform transform, float angle, float duration)
        {
            float elapsedTime = 0.0f;
            float startScale = 1.0f;
            float endScale = 0.0f;

            while (elapsedTime < duration)
            {
                elapsedTime = System.Math.Min(elapsedTime + Time.deltaTime, duration);
                float mag = elapsedTime / duration;
                float currentRotation = angle * mag;
                transform.localRotation = Quaternion.Euler(0, 0, currentRotation);
                float currentScale = startScale + (endScale - startScale) * mag;
                transform.localScale = new Vector3(currentScale, currentScale, currentScale);
                yield return null;
            }
        }
    }
}
