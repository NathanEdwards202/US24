using Delegates;
using UnityEngine;
using UnityEngine.UI;

namespace UI.SimulationScene
{
    public class EndSimulationButtons : MonoBehaviour
    {
        [SerializeField] Button _skipToEndButton;
        [SerializeField] Button _returnToMainMenuButton;

        private void Start()
        {
            SetupDelegates();

            _skipToEndButton.gameObject.SetActive(false);
            _returnToMainMenuButton.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            RemoveDelegates();
        }

        void SetupDelegates()
        {
            GameFlowDelegates.onWinnerHasBeenDetermined += EnableSkip;
            GameFlowDelegates.onEveryVoteCounted += EnableReturn;

            _skipToEndButton.onClick.AddListener(OnClickSkip);
            _returnToMainMenuButton.onClick.AddListener(OnClickReturn);
        }

        void RemoveDelegates()
        {
            GameFlowDelegates.onWinnerHasBeenDetermined -= EnableSkip;
            GameFlowDelegates.onEveryVoteCounted -= EnableReturn;

            _skipToEndButton.onClick.RemoveAllListeners();
            _returnToMainMenuButton.onClick.RemoveAllListeners();
        }

        void EnableSkip()
        {
            _skipToEndButton.gameObject.SetActive(true);
        }

        void EnableReturn()
        {
            _skipToEndButton.gameObject.SetActive(false);
            _returnToMainMenuButton.gameObject.SetActive(true);
        }

        void OnClickSkip()
        {
            GameFlowDelegates.onSkip?.Invoke();
        }

        void OnClickReturn()
        {
            GameFlowDelegates.onReturnToMainMenu?.Invoke();
            _returnToMainMenuButton.gameObject.SetActive(false);
        }
    }
}