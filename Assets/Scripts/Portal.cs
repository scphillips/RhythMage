// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using UnityEngine;

namespace RhythMage
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

        void OnBeat()
        {
            bool isActive = m_avatar.currentCellIndex >= m_dungeon.GetCellCount() - 5;
            active.SetActive(isActive);
            inactive.SetActive(isActive == false);
        }
    }
}
