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

        [Serializable]
        public class Settings
        {
            public List<Sprite> magicFrames;
            public List<Sprite> meleeFrames;
        }

        [Zenject.Inject]
        readonly Settings settings;

        [Zenject.Inject]
        readonly CameraProvider cameraProvider;

        [Zenject.Inject]
        readonly SoundManager soundManager;

        public event EventHandler OnDeathTriggered;

        int m_currentFrame;
        EnemyType m_type;

        [Zenject.Inject]
        public void Construct(Cell cell, EnemyType type)
        {
            SetPosition(cell);
            SetEnemyType(type);
        }

        void Start()
        {
            soundManager.OnBeat += OnBeat;
        }

        public EnemyType GetEnemyType()
        {
            return m_type;
        }

        public void SetEnemyType(EnemyType type)
        {
            m_type = type;
            UpdateAnimation();
        }

        public void SetPosition(Cell cell)
        {
            transform.localPosition = new Vector3(cell.x, 0, cell.y);
        }

        void OnBeat(object sender, EventArgs e)
        {
            UpdateAnimation();
        }

        void Update()
        {
            transform.forward = cameraProvider.transform.forward;
        }

        void UpdateAnimation()
        {
            var frames = (m_type == EnemyType.Magic) ? settings.magicFrames : settings.meleeFrames;
            m_currentFrame = (m_currentFrame + 1) % frames.Count;
            GetComponent<SpriteRenderer>().sprite = frames[m_currentFrame];
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
                elapsedTime = System.Math.Min(elapsedTime + Time.deltaTime, duration);
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
