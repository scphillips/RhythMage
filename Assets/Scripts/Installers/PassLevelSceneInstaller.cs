// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, January 2022

using Zenject;

namespace RhythMage
{
    public class PassLevelSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<RandomNumberProvider>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<SoundManager>()
                .AsSingle();
        }
    }
}
