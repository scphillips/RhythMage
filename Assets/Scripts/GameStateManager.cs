// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

namespace RhythMage
{
    public class GameStateManager
    {
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
    }
}
