using UnityEngine;
using Zenject;

public class GameSceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<Outplay.RandomNumberProvider>()
            .AsSingle();

        Container.Bind<Outplay.RhythMage.AvatarModel>()
            .AsSingle();

        Container.Bind<Outplay.RhythMage.DungeonModel>()
            .AsSingle();

        Container.Bind<Outplay.RhythMage.EnemyFactory>()
            .AsSingle();

        Container.Bind<Outplay.RhythMage.GameStateManager>()
            .AsSingle()
            .NonLazy();
    }
}
