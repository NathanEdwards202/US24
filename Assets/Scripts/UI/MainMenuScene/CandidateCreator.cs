using Candidates;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



namespace UI.MainMenuScene
{
    public class CandidateCreator : MonoBehaviour
    {
        [SerializeField] GameObject _nextScreen, _lastScreen;

        [SerializeField] CandidateSO _candidate;

        public CandidateStatPicker _radicalismPicker, _nameRecognitionPicker, _coveragePicker, _passionPicker, _speechGivingPicker, _neutralPullPicker, _youthPicker, _middleAgePicker, _elderlyPicker, _urbanPicker, _ruralPicker, _workingClassPicker, _middleClassPicker, _richPicker
            , _nonPoCPicker, _poCPicker;

        [SerializeField] Button _backButton, _forwardButton;
        [SerializeField] List<TemplateButton> _templateButtons;
        [SerializeField] TMP_InputField _nameInput;

        private void Start()
        {
            if (_backButton != null)
            {
                _backButton.onClick.AddListener(LastScreen);
            }

            if(_forwardButton != null)
            {
                _forwardButton.onClick.AddListener(SetUpCandidate);
            }


            foreach(TemplateButton button in _templateButtons)
            {
                button._button.onClick.AddListener(() => SetupFromTemplate(button._thisCandidate));
            }
        }

        private void OnDestroy()
        {
            if (_backButton != null)
            {
                _backButton.onClick.RemoveAllListeners();
            }

            if(_forwardButton != null)
            {
                _forwardButton.onClick.RemoveAllListeners();
            }


            foreach (TemplateButton button in _templateButtons)
            {
                button._button.onClick.RemoveAllListeners();
            }
        }


        public void SetupFromTemplate(CandidateSO template)
        {
            _nameInput.text = template._name;

            _radicalismPicker.SetValue(template._radicalismPercent);
            _nameRecognitionPicker.SetValue(template._nameRecognitionPercent);
            _coveragePicker.SetValue(template._coveragePercent);
            _passionPicker.SetValue(template._passionPercent);
            _speechGivingPicker.SetValue(template._speechGivingPercent);
            _neutralPullPicker.SetValue(template._neutralPullPercent);
            _youthPicker.SetValue(template._youthPercent);
            _middleAgePicker.SetValue(template._middleAgePercent);
            _elderlyPicker.SetValue(template._elderlyPercent);
            _urbanPicker.SetValue(template._urbanPercent);
            _ruralPicker.SetValue(template._ruralPercent);
            _workingClassPicker.SetValue(template._workingClassPercent);
            _middleClassPicker.SetValue(template._middleClassPercent);
            _richPicker.SetValue(template._richPercent);
            _nonPoCPicker.SetValue(template._nonPoCPercent);
            _poCPicker.SetValue(template._poCPercent);
        }


        void SetUpCandidate()
        {
            _candidate._name = _nameInput.text;

            _candidate._radicalismPercent = _radicalismPicker._statValue;
            _candidate._nameRecognitionPercent = _nameRecognitionPicker._statValue;
            _candidate._coveragePercent = _coveragePicker._statValue;
            _candidate._passionPercent = _passionPicker._statValue;
            _candidate._speechGivingPercent = _speechGivingPicker._statValue;
            _candidate._neutralPullPercent = _neutralPullPicker._statValue;
            _candidate._youthPercent = _youthPicker._statValue;
            _candidate._middleAgePercent = _middleAgePicker._statValue;
            _candidate._elderlyPercent = _elderlyPicker._statValue;
            _candidate._urbanPercent = _urbanPicker._statValue;
            _candidate._ruralPercent = _ruralPicker._statValue;
            _candidate._workingClassPercent = _workingClassPicker._statValue;
            _candidate._middleClassPercent = _middleClassPicker._statValue;
            _candidate._richPercent = _richPicker._statValue;
            _candidate._nonPoCPercent = _nonPoCPicker._statValue;
            _candidate._poCPercent = _poCPicker._statValue;

            NextScreen();
        }

        void NextScreen()
        {
            _nextScreen.SetActive(true);
            gameObject.SetActive(false);
        }

        void LastScreen()
        {
            _lastScreen.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}