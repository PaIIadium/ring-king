using Gameplay;
using Levels;
using Signals;
using UnityEngine;
using Zenject;

namespace GameUtils
{
    public class LevelState
    {
        private SignalBus signalBus;
        private Timer timer;
        private Level currentLevel;

        public int RingsLeft { get; private set; }
        
        public float TimeLeft
        {
            get => timer.TimeLeft;
            private set => timer.StartTimer(value, new LevelFailedSignal());
        }
        
        public LevelState(SignalBus signalBus)
        {
            this.signalBus = signalBus;
            timer = Object.FindObjectsOfType<Timer>()[0];
            signalBus.Subscribe<RingDestroyedSignal>(OnRingDestroy);
        }

        private void OnRingDestroy()
        {
            if (--RingsLeft == 0) signalBus.Fire<LevelCompletedSignal>();
        }

        public void Initialize(Level level)
        {
            RingsLeft = level.levelPrefab.GetComponentsInChildren<RingDestroyer>().Length;
            TimeLeft = level.time;
        }

        public void Reset()
        {
            timer.StopTimer();
        }
    }
}
