// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using UnityEngine;
using Zenject;

namespace RhythMage
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Installers/GameSettings")]
    public class GameSettings : ScriptableObjectInstaller<GameSettings>
    {
        public AvatarController.Settings AvatarControllerSettings;
        public DungeonAmbientController.Settings DungeonAmbientControllerSettings;
        public DungeonBuilder.Settings DungeonBuilderSettings;
        public GameDifficulty.Settings GameDifficultySettings;
        public GameStateManager.Settings GameStateManagerSettings;
        public HUDController.Settings HUDControllerSettings;
        public SoundManager.Settings SoundManagerSettings;

        public override void InstallBindings()
        {
            Container.BindInstance(AvatarControllerSettings);
            Container.BindInstance(DungeonAmbientControllerSettings);
            Container.BindInstance(DungeonBuilderSettings);
            Container.BindInstance(GameDifficultySettings);
            Container.BindInstance(GameStateManagerSettings);
            Container.BindInstance(HUDControllerSettings);
            Container.BindInstance(SoundManagerSettings);
        }
    }
}
