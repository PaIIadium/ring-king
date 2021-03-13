using Signals;
using UnityEngine;
using Zenject;

namespace GameUtils
{
    public class GameInitializer : MonoBehaviour
    {
        [Inject] 
        private SignalBus signalBus;
        private void Start()
        {
            signalBus.Fire<GameLaunchedSignal>();
        }
    }
}
