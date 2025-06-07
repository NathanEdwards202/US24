using Controllers.SimulationScene.Simulation;
using Delegates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



namespace UI.SimulationScene
{
    public class ViewModeDisplay : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _displayTypeText;
        [SerializeField] Button _displayTypeChangerButton;

        [SerializeField] SimulationSettingsSO _simSettings;

        private void OnEnable()
        {
            _displayTypeChangerButton.onClick.AddListener(OnClick);

            UpdateText();
        }

        private void OnDisable()
        {
            _displayTypeChangerButton.onClick.RemoveAllListeners();
        }

        void OnClick()
        {
            _simSettings.UpdateViewMode();
            UpdateText();

            UIDelegates.onUpdatedViewType?.Invoke();
        }

        void UpdateText()
        {
            if (_simSettings._viewMode == ViewMode.LIKELIHOOD)
            {
                _displayTypeText.text = "Likelihood to win";
            }

            else
            {
                _displayTypeText.text = "Margins";
            }
        }
    }
}