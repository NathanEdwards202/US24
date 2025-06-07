using TMPro;
using UnityEngine;
using UnityEngine.UI;



namespace UI.MainMenuScene
{
    public class CandidateStatPicker : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _text;
        public string _statName;
        public float _statValue;
        public Slider _statSlider;

        private void Start()
        {
            _statValue = 10;
            _statSlider.value = 10;

            SetText();

            _statSlider.onValueChanged.AddListener(OnSliderSlid);
        }

        public void SetValue(float value)
        {
            _statValue = value;
            _statSlider.value = value;
            SetText();
        }

        void OnSliderSlid(float value)
        {
            _statValue = value;

            SetText();
        }

        void SetText()
        {
            _text.text = $"{_statName}:\n{_statValue}";
        }
    }
}