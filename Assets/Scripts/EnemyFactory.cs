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
            var enemy = (GameObject)GameObject.Instantiate(m_settings.prefabEnemy);

            var behavior = enemy.GetComponent<Enemy>();
            behavior.Camera = m_camera;
            behavior.settings = m_settings;
            behavior.SoundMgr = m_sound;

            behavior.SetPosition(cell);
            behavior.SetEnemyType(type);

            return behavior;
        }
    }
}
