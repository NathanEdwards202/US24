using Delegates;
using NUnit.Framework.Constraints;
using States;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;



namespace UI.SimulationScene
{
    public class StateButton : MonoBehaviour
    {
        [SerializeField] State _thisState;
        Button _thisButton;
        Sprite _thisSprite;

        private void Start()
        {
            _thisButton = GetComponent<Button>();
            SetupOnPollingStart();

            //_thisButton.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.001f; // Require a proper click
            _thisButton.GetComponent<RectTransform>().sizeDelta = new Vector2(1920f, 1080f); // It was defaulting to 1920x1 for some reason

            _thisSprite = GetComponent<Image>().sprite;            
            GetComponent<Image>().sprite = _thisSprite;
            GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;

            GameFlowDelegates.onPollingEnd += SetupOnPollingEnd;
        }

        private void OnDestroy()
        {
            GameFlowDelegates.onPollingEnd -= SetupOnPollingEnd;
            _thisButton.onClick.RemoveAllListeners();
        }

        void SetupOnPollingStart()
        {
            _thisButton.onClick.AddListener(OnClickPolling);
        }

        void SetupOnPollingEnd()
        {
            _thisButton.onClick.RemoveAllListeners();
            _thisButton.onClick.AddListener(OnClick);
        }

        void OnClickPolling()
        {
            Debug.Log("Nya");
            UIDelegates.onStateButtonClicked?.Invoke(_thisState, true);
        }

        void OnClick()
        {
            Debug.Log("Nya");
            UIDelegates.onStateButtonClicked?.Invoke(_thisState);
        }
    }
}