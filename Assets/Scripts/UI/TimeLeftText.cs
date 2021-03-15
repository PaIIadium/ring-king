using GameUtils;
using TMPro;
using UnityEngine;
using Zenject;

namespace UI
{
    public class TimeLeftText : MonoBehaviour
    {
        [Inject] 
        private SignalBus signalBus;

        [Inject] 
        private LevelState levelState;

        private TMP_Text tmp;
        
        private void Start()
        {
            tmp = GetComponent<TMP_Text>();
        }

        void Update()
        { 
           var leftTimeString = ((int)levelState.TimeLeft).ToString();
           tmp.SetText(leftTimeString);
        }
    }
}
