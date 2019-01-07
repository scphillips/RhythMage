using System;
using UnityEngine;
using Zenject;

namespace Outplay.RhythMage
{
    public class GameSceneInstaller : MonoInstaller
    {
        public GameObject prefabEnemy;

        public override void InstallBindings()
        {
            Container.Bind<RandomNumberProvider>()
                .AsSingle()
                .WithArguments(Environment.TickCount);

            Container.Bind<AvatarModel>()
                .AsSingle();

            Container.Bind<DungeonModel>()
                .AsSingle();

            Container.BindFactory<Cell, EnemyType, Enemy, Enemy.Factory>()
                .FromComponentInNewPrefab(prefabEnemy);

            Container.BindInterfacesAndSelfTo<GameStateManager>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<SoundManager>()
                .AsSingle();
        }
    }
}
