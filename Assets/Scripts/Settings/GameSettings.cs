using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Installers/GameSettings")]
public class GameSettings : ScriptableObjectInstaller<GameSettings>
{
    public DungeonBuilder.Settings DungeonBuilderSettings;
    public override void InstallBindings()
    {
        Container.BindInstance(DungeonBuilderSettings);
    }
}