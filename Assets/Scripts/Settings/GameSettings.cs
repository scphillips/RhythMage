using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Installers/GameSettings")]
public class GameSettings : ScriptableObjectInstaller<GameSettings>
{
    public Outplay.RhythMage.DungeonBuilder.Settings DungeonBuilderSettings;
    public Outplay.RhythMage.Enemy.Settings EnemySettings;
    public Outplay.RhythMage.HUDController.Settings HUDControllerSettings;

    public override void InstallBindings()
    {
        Container.BindInstance(DungeonBuilderSettings);
        Container.BindInstance(EnemySettings);
        Container.BindInstance(HUDControllerSettings);
    }
}
