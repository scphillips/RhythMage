// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using UnityEngine;

namespace RhythMage
{
    public class GameStateManager : MonoBehaviour
    {
        public GameSettings settings;

        [System.Serializable]
        public class Settings
        {
            public SceneReference gameOverScene;
            public SceneReference gameScene;
            public SceneReference menuScene;
            public SceneReference passLevelScene;

            public float delayTransitionToGameOverDuration;
        }
        
        public bool IsGameRunning { get; set; }

        public GestureHandler gestureHandler = new GestureHandler();

        AvatarModel m_avatar = new AvatarModel();
        public AvatarModel Avatar { get => m_avatar; }

        public AvatarSettings avatarSettings;

        void Start()
        {
            GetComponent<UpdateManager>().OnUpdate += gestureHandler.Update;
            ResetAvatar();
        }

        public void ResetAvatar()
        {
            m_avatar.Init(avatarSettings);
        }
    }
}
