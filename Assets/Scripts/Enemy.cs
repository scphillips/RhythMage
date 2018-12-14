using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Outplay.RhythMage
{
    public class Enemy : MonoBehaviour
    {
        public enum EnemyType
        {
            Magic,
            Melee
        }

        [Serializable]
        public class Settings
        {
            public GameObject prefabEnemy;
            public List<Sprite> magicFrames;
            public List<Sprite> meleeFrames;
        }
        
        public Settings settings;
        public CameraProvider Camera;
        public SoundManager SoundMgr;

        int m_currentFrame;
        EnemyType m_type;

        void Start()
        {
            SoundMgr.OnBeat += OnBeat;
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
            transform.forward = Camera.transform.forward;
        }

        void UpdateAnimation()
        {
            var frames = (m_type == EnemyType.Magic) ? settings.magicFrames : settings.meleeFrames;
            m_currentFrame = (m_currentFrame + 1) % frames.Count;
            GetComponent<SpriteRenderer>().sprite = frames[m_currentFrame];
        }
    }
}
