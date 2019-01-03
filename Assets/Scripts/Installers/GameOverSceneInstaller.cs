using Zenject;

namespace Outplay.RhythMage
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
