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

        [SerializeField] GameObject _nextScreen;
        [SerializeField] Button _forwardButton;

        static bool _firstRun = true;

        void Start()
        {
            if (_firstRun) 
            {
                _resultsTracker.StartSelf();
                _firstRun = false;
            }
            SetText();

            _forwardButton.onClick.AddListener(NextScreen);
        }

        private void OnDestroy()
        {
            _forwardButton.onClick.RemoveAllListeners();
        }

        void SetText()
        {
            for (int i = 0; i < _resultsTracker._results.Count; i++)
            {
                _resultsText[i].text = _resultsTracker._results[i].GetResultDisplayTextFromThis() ?? string.Empty;
            }
        }

        void NextScreen()
        {
            _nextScreen.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}