using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Installers/GameSettings")]
public class GameSettings : ScriptableObjectInstaller<GameSettings>
{
    public Outplay.RhythMage.AvatarController.Settings AvatarControllerSettings;
    public Outplay.RhythMage.DungeonBuilder.Settings DungeonBuilderSettings;
    public Outplay.RhythMage.Enemy.Settings EnemySettings;
    public Outplay.RhythMage.GameDifficulty.Settings GameDifficultySettings;
    public Outplay.RhythMage.GameStateManager.Settings GameStateManagerSettings;
    public Outplay.RhythMage.HUDController.Settings HUDControllerSettings;
    public Outplay.RhythMage.SoundManager.Settings SoundManagerSettings;

    public override void InstallBindings()
    {
        Container.BindInstance(AvatarControllerSettings);
        Container.BindInstance(DungeonBuilderSettings);
        Container.BindInstance(EnemySettings);
        Container.BindInstance(GameDifficultySettings);
        Container.BindInstance(GameStateManagerSettings);
        Container.BindInstance(HUDControllerSettings);
        Container.BindInstance(SoundManagerSettings);
    }
}
