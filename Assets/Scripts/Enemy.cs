using System.Collections;
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
            OnDeathTriggered?.Invoke(this);
            transform.SetParent(cameraProvider.transform, true);
            int direction = (m_type == EnemyType.Magic) ? -1 : 1;
            StartCoroutine(DeathAnimation(transform, 360.0f * direction, 0.3f));
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
