using UnityEngine;
using Zenject;

public class DependencyInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<Outplay.RandomNumberProvider>()
            .AsSingle()
            .WithArguments(1);  // Fixed seed

        Container.Bind<Outplay.RhythMage.AvatarModel>()
            .AsSingle();

        Container.Bind<Outplay.RhythMage.DungeonModel>()
            .AsSingle();
    }
}
