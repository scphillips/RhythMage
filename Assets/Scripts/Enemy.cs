// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using System.Collections;
using UnityEngine;

namespace RhythMage
{
    public class Enemy : MonoBehaviour
    {
        public class Factory
        {
            Enemy m_prefab;

            public Factory(Enemy prefab)
            {
                m_prefab = prefab;
            }
            
            public Enemy Create(Cell cell, EnemyType type)
            {
                Enemy newEnemy = Instantiate(m_prefab);
                newEnemy.Reset(cell, type);
                return newEnemy;
            }
        }

        public GameObject bat;
        public GameObject goblin;
        public GameObject rat;
        public GameObject slime;

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

                GameObject activeSprite = ActiveSprite;
                bat.SetActive(activeSprite == bat);
                goblin.SetActive(activeSprite == goblin);
                rat.SetActive(activeSprite == rat);
                slime.SetActive(activeSprite == slime);
            }
        }

        GameObject ActiveSprite
        {
            get
            {
                switch (EnemyType)
                {
                    case EnemyType.Bat: return bat;
                    case EnemyType.Goblin: return goblin;
                    case EnemyType.Rat: return rat;
                    case EnemyType.Slime: return slime;
                    default: return null;
                }
            }
        }

        public void Reset(Cell cell, EnemyType type)
        {
            transform.localPosition = new Vector3(cell.x, 0.0f, cell.y);
            ActiveSprite.transform.localRotation = Quaternion.Euler(0, 0, 0);
            ActiveSprite.transform.localScale = Vector3.one;
            EnemyType = type;
        }

        public void SetPosition(Cell cell)
        {
            transform.localPosition = new Vector3(cell.x, 0.0f, cell.y);
        }

        public void Die()
        {
            OnDeathTriggered?.Invoke(this);
            CameraProvider cameraProvider = Utils.FindCameraProvider();
            if (cameraProvider != null)
            {
                transform.SetParent(cameraProvider.transform, true);
            }
            int direction = (m_type == EnemyType.Rat || m_type == EnemyType.Slime) ? -1 : 1;
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
                float phase = elapsedTime / duration;
                float currentRotation = angle * phase;
                transform.localRotation = Quaternion.Euler(0, 0, currentRotation);
                float currentScale = startScale + (endScale - startScale) * phase;
                transform.localScale = new Vector3(currentScale, currentScale, currentScale);
                yield return null;
            }

            transform.localScale = new Vector3(endScale, endScale, endScale);
        }
    }
}
