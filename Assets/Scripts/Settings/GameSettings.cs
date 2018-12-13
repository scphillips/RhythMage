using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Installers/GameSettings")]
public class GameSettings : ScriptableObjectInstaller<GameSettings>
{
    public Outplay.RhythMage.DungeonBuilder.Settings DungeonBuilderSettings;
    public override void InstallBindings()
    {
        Container.BindInstance(DungeonBuilderSettings);
    }
}
