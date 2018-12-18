using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Outplay.RhythMage
{
    public class EnemyFactory
    {
        [Zenject.Inject]
        CameraProvider m_camera;

        [Zenject.Inject]
        Enemy.Settings m_settings;

        [Zenject.Inject]
        SoundManager m_sound;

        public Enemy CreateEnemy(Cell cell, Enemy.EnemyType type)
        {
            GameObject entity = GameObject.Instantiate(m_settings.prefabEnemy);
            var enemy = entity.GetComponent<Enemy>();

            enemy.cameraProvider = m_camera;
            enemy.settings = m_settings;
            enemy.soundManager = m_sound;

            enemy.SetPosition(cell);
            enemy.SetEnemyType(type);

            return enemy;
        }
    }
}
