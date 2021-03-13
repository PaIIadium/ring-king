using Signals;
using UnityEngine;
using Zenject;

namespace UI
{
    public class ButtonsScripts : MonoBehaviour
    {
        [Inject] 
        private SignalBus signalBus;

        private PausePanel pausePanel;


        public void Start()
        {
            pausePanel = FindObjectOfType<PausePanel>();
        }

        public void Update()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                ShowPausePanel();
            }
        }

        public void OnPlayOnClick()
        {
            signalBus.Fire<PlayButtonClickedSignal>();
        }
        
        private void ShowPausePanel()
        {
            pausePanel.Show();
        }

        public void OnContinueOnClick()
        {
            pausePanel.Hide();
        }

        public void OnReloadLevelOnClick()
        {
            pausePanel.Hide();
            signalBus.Fire<LevelFailedSignal>();
        }

        public void OnMainMenuOnClick()
        {
            pausePanel.Hide();
            signalBus.Fire<GameLaunchedSignal>();
        }
    }
}