using System;
using UnityEngine;

namespace Outplay.RhythMage
{
    public class Portal : MonoBehaviour
    {
        [Zenject.Inject]
        readonly AvatarModel m_avatar;

        [Zenject.Inject]
        readonly DungeonModel m_dungeon;

        [Zenject.Inject]
        readonly SoundManager soundManager;

        public GameObject active;
        public GameObject inactive;
        
        void Start()
        {
            soundManager.OnBeat += OnBeat;
        }

        void OnBeat(object sender, EventArgs e)
        {
            bool isActive = (m_avatar.currentCellIndex >= m_dungeon.GetCellCount() - 5);
            active.SetActive(isActive);
            inactive.SetActive(isActive == false);
        }
    }
}
