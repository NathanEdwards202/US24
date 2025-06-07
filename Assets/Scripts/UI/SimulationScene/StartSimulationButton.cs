using Delegates;
using UnityEngine;
using UnityEngine.UI;

namespace UI.SimulationScene
{
    public class StartSimulationButton : MonoBehaviour
    {
        [SerializeField] Button btn;
    
        void Start()
        {
            btn.onClick.AddListener(OnClick);

            SetupDelegates();
        }

        private void OnDestroy()
        {
            btn?.onClick.RemoveListener(OnClick);

            RemoveDelegates();
        }

        void SetupDelegates()
        {
            GameFlowDelegates.onPollingEnd += DisableSelf;
        }

        void RemoveDelegates()
        {
            GameFlowDelegates.onPollingEnd -= DisableSelf;
        }

        void OnClick()
        {
            GameFlowDelegates.onPollingEnd?.Invoke();
        }

        void DisableSelf()
        {
            gameObject.SetActive(false);
        }
    }
}