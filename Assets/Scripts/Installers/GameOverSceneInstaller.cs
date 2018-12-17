using UnityEngine;
using Zenject;

public class GameOverSceneInstaller : MonoInstaller
{
    [InjectOptional]
    public int FinalScore = 0;

    public override void InstallBindings()
    {
        Container.Bind<Outplay.RhythMage.GameOverController>()
            .AsSingle()
            .WithArguments(FinalScore);
    }
}
