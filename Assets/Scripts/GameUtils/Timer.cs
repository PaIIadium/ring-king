using UnityEngine;
using Zenject;

namespace GameUtils
{
    public class Timer : MonoBehaviour
    {
        [Inject] 
        private SignalBus signalBus;
        public float TimeLeft { get; private set; }
        private bool isRunning;

        private object finalSignal;
        
        public void StartTimer(float time, object signal)
        {
           TimeLeft = time;
           isRunning = true;
           finalSignal = signal;
        }

        public void StopTimer()
        {
            isRunning = false;
        }
        private void Update()
        {
            if (isRunning && TimeLeft < 0)
            {
                TimeLeft = 0;
                isRunning = false;
                signalBus.Fire(finalSignal);
            } 
            if (isRunning) TimeLeft -= Time.deltaTime;
        }
    }
}