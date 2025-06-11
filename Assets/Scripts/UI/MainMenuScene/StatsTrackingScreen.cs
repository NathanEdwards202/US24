using UnityEngine;
using UnityEngine.UI;



namespace UI.MainMenuScene
{
    public class StatsTrackingScreen : MonoBehaviour
    {
        [SerializeField] GameObject _nextScreen;//, _lastScreen;
        [SerializeField] Button _forwardButton;//, _backButton;

        void Start()
        {
            _forwardButton.onClick.AddListener(() => ChangeScreen(_nextScreen));
            //_backButton.onClick.AddListener(() => ChangeScreen(_lastScreen));
        }

        private void OnDestroy()
        {
            _forwardButton.onClick.RemoveAllListeners();
            //_backButton.onClick.RemoveAllListeners();
        }


        void ChangeScreen(GameObject newScreen)
        {
            newScreen.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}