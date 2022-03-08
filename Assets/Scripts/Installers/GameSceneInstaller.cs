// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using UnityEngine;
using Zenject;

namespace RhythMage
{
    public class GameSceneInstaller : MonoInstaller
    {
        public GameObject prefabEnemy;

        public override void InstallBindings()
        {
            Container.Bind<RandomNumberProvider>()
                .AsSingle();

            Container.Bind<AvatarModel>()
                .AsSingle();

            Container.Bind<DungeonModel>()
                .AsSingle();

            Container.BindFactory<Cell, EnemyType, Enemy, Enemy.Factory>()
                .FromComponentInNewPrefab(prefabEnemy);

            Container.BindInterfacesAndSelfTo<HUDController>()
                .AsSingle()
                .NonLazy();

            Container.Bind<GameStateManager>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<GestureHandler>()
                .AsSingle();

            Container.Bind<LevelBuilder>()
                .AsSingle();

            Container.Bind<PathBuilder>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<SoundManager>()
                .AsSingle();
        }
    }
}
