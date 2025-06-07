using Candidates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



namespace UI.MainMenuScene
{
    public class TemplateButton : MonoBehaviour
    {
        [SerializeField] public Button _button;
        public TextMeshProUGUI _text;
        public CandidateSO _thisCandidate;

        private void Start()
        {
            _text.text = _thisCandidate._name;
        }
    }
}