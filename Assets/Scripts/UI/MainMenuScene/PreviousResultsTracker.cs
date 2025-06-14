using Controllers.MainScene.ResultsTracker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



namespace UI.MainMenuScene
{
    public class PreviousResultsTracker : MonoBehaviour
    {
        [SerializeField] ResultsTrackerSO _resultsTracker;
        [SerializeField] TextMeshProUGUI[] _resultsText;

        [SerializeField] GameObject _nextScreen, _lastScreen;
        [SerializeField] Button _forwardButton, _backButton;

        static bool _firstRun = true;

        void Start()
        {
            if (_firstRun) 
            {
                _resultsTracker.StartSelf();
                _firstRun = false;
            }
            SetText();

            _forwardButton.onClick.AddListener(() => ChangeScreen(_nextScreen));
            _backButton.onClick.AddListener(() => ChangeScreen(_lastScreen));
        }

        private void OnDestroy()
        {
            _forwardButton.onClick.RemoveAllListeners();
            _backButton.onClick.RemoveAllListeners();
        }

        void SetText()
        {
            for (int i = 0; i < _resultsTracker._results.Count; i++)
            {
                _resultsText[i].text = _resultsTracker._results[i].GetResultDisplayTextFromThis() ?? string.Empty;
            }
        }

        void ChangeScreen(GameObject newScreen)
        {
            newScreen.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}