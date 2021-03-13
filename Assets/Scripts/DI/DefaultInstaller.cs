using GameUtils;
using Signals;
using Zenject;

namespace DI
{
    public class DefaultInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<PlayerCollidedWithRingSignal>();
            Container.DeclareSignal<SwipeDetectedSignal>();
            Container.DeclareSignal<PlayButtonClickedSignal>();
            Container.DeclareSignal<RingDestroyedSignal>();
            Container.DeclareSignal<GameLaunchedSignal>();
            Container.DeclareSignal<LevelFailedSignal>();
            Container.DeclareSignal<LevelCompletedSignal>();
            
            Container.Bind<LevelProvider>().AsSingle();
            Container.Bind<Timer>().AsSingle();
            Container.Bind<LevelState>().AsSingle();
            Container.Bind<LevelLoader>().AsSingle();
        }
    }
}