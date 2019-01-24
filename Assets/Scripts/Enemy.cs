using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Outplay.RhythMage
{
    public class Enemy : MonoBehaviour
    {
        public class Factory : Zenject.PlaceholderFactory<Cell, EnemyType, Enemy>
        {
        }
        
        [Zenject.Inject]
        readonly CameraProvider cameraProvider;

        public GameObject magic;
        public GameObject melee;

        public event EventHandler OnDeathTriggered;
        
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
                magic.SetActive(m_type == EnemyType.Magic);
                melee.SetActive(m_type == EnemyType.Melee);
            }
        }

        [Zenject.Inject]
        public void Construct(Cell cell, EnemyType type)
        {
            SetPosition(cell);
            EnemyType = type;
        }

        public void SetPosition(Cell cell)
        {
            transform.localPosition = new Vector3(cell.x, 0, cell.y);
        }

        public void Die()
        {
            if (OnDeathTriggered != null)
            {
                OnDeathTriggered(this, null);
            }
            transform.SetParent(cameraProvider.transform, true);
            int direction = (m_type == EnemyType.Magic) ? -1 : 1;
            StartCoroutine(DeathAnimation(transform, 360.0f * direction, 0.0f, 0.3f));
        }

        IEnumerator DeathAnimation(Transform transform, float angle, float scale, float duration)
        {
            float elapsedTime = 0.0f;
            float startScale = 1.0f;

            while (elapsedTime < duration)
            {
                elapsedTime = Math.Min(elapsedTime + Time.deltaTime, duration);
                float mag = elapsedTime / duration;
                float currentRotation = angle * mag;
                transform.localRotation = Quaternion.Euler(0, 0, currentRotation);
                float currentScale = startScale + (scale - startScale) * mag;
                transform.localScale = new Vector3(currentScale, currentScale, currentScale);
                yield return null;
            }
        }
    }
}
