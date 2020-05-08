// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using Zenject;

namespace RhythMage
{
    public class GameOverSceneInstaller : MonoInstaller
    {
        [InjectOptional]
        public int finalScore = 0;

        public override void InstallBindings()
        {
            Container.Bind<GameOverController>()
                .AsSingle()
                .WithArguments(finalScore);
        }
    }
}
