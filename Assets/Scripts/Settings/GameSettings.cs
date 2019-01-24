using UnityEngine;
using Zenject;

namespace Outplay.RhythMage
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Installers/GameSettings")]
    public class GameSettings : ScriptableObjectInstaller<GameSettings>
    {
        public AvatarController.Settings AvatarControllerSettings;
        public DungeonBuilder.Settings DungeonBuilderSettings;
        public GameDifficulty.Settings GameDifficultySettings;
        public GameStateManager.Settings GameStateManagerSettings;
        public HUDController.Settings HUDControllerSettings;
        public SoundManager.Settings SoundManagerSettings;

        public override void InstallBindings()
        {
            Container.BindInstance(AvatarControllerSettings);
            Container.BindInstance(DungeonBuilderSettings);
            Container.BindInstance(GameDifficultySettings);
            Container.BindInstance(GameStateManagerSettings);
            Container.BindInstance(HUDControllerSettings);
            Container.BindInstance(SoundManagerSettings);
        }
    }
}
