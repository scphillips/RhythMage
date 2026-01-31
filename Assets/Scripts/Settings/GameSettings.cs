// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using UnityEngine;

namespace RhythMage
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Settings/GameSettings")]
    public class GameSettings : ScriptableObject
    {
        public AvatarController.Settings AvatarControllerSettings;
        public DungeonAmbientController.Settings DungeonAmbientControllerSettings;
        public DungeonBuilder.Settings DungeonBuilderSettings;
        public GameDifficulty.Settings GameDifficultySettings;
        public GameStateManager.Settings GameStateManagerSettings;
        public HUDController.Settings HUDControllerSettings;
        public LevelBuilder.Settings LevelBuilderSettings;
        public PathBuilder.Settings PathBuilderSettings;
        public SoundManager.Settings SoundManagerSettings;
    }
}
