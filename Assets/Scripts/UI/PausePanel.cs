using UnityEngine;

namespace UI
{
    public class PausePanel : MonoBehaviour
    {
        public void Start()
        {
            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Time.timeScale = 0;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            Time.timeScale = 1;
        }
    }
}