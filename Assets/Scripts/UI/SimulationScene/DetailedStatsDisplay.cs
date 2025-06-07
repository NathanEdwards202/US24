using Candidates;
using Delegates;
using States;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



namespace UI.SimulationScene
{
    public class DetailedStatsDisplay : MonoBehaviour
    {
        [SerializeField] Candidate _democrat, _republican;
        State _state = null;

        [SerializeField] Button _closeButton;
        [SerializeField] TextMeshProUGUI _stateNameText, _demCandidateText, _repCandidateText, _demVotesText, _repVotesText, _demVotesPercentText, _repVotesPercentText, _votesCountedText;
        [SerializeField] GameObject _demBar, _repBar;
        [SerializeField] Image _backgroundImage;


        const float TOTAL_BAR_WIDTH = 1440f;
        const float BAR_TOP_OFFSET = 0f, BAR_OFFSET_BOTTOM = 0f, BAR_STARTING_RIGHT_POSITION = 0f;
        const float BAR_DEFAULT_LENGTH = 0f;

        private void Start()
        {
            _closeButton.onClick.AddListener(CloseSelf);

            UIDelegates.onStateButtonClicked += UpdateState;

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveAllListeners();

            UIDelegates.onStateButtonClicked -= UpdateState;
        }

        void UpdateState(State state, bool polling = false)
        {
            if (!state.GetCountingStarted() && !polling) return;

            _state = state;
            _stateNameText.text = state.GetName() + $" [{state._electoralVotes}]";
            _demCandidateText.text = _democrat.GetName();
            _repCandidateText.text = _republican.GetName();

            if (!gameObject.activeSelf) gameObject.SetActive(true);

            UpdateValues();
        }

        public void UpdateValues()
        {
            if (!gameObject.activeSelf) return;

            float demPopVote = MathF.Floor(_state.GetCurrentVotes()[0]);
            float repPopVote = MathF.Floor(_state.GetCurrentVotes()[1]);

            if(demPopVote == 0 || repPopVote == 0) 
            {
                OnZero();
                return;
            }

            float leadPercent = MathF.Abs((demPopVote - repPopVote) / (demPopVote + repPopVote));



            _demVotesText.text = $"{demPopVote.ToString("N0")}";
            _repVotesText.text = $"{repPopVote.ToString("N0")}";

            _demVotesPercentText.text = $"({((demPopVote / (demPopVote + repPopVote)) * 100f).ToString("N2")}%)" + ((demPopVote > repPopVote) ? $"\t(+{(leadPercent * 100f).ToString("N2")}%)" : $"");
            _repVotesPercentText.text = ((repPopVote > demPopVote) ? $"\t(+{(leadPercent * 100f).ToString("N2")}%)\t" : $"") + $"({((repPopVote / (demPopVote + repPopVote)) * 100f).ToString("N2")}%)";

            _votesCountedText.text = $"{MathF.Floor((demPopVote + repPopVote) / _state.GetTotalVotes() * 100)}% IN";


            // Title background
            _backgroundImage.color = _state.GetImage().color;


            // Dem vote bar
            float startingPosition = -BAR_STARTING_RIGHT_POSITION;
            float distance = TOTAL_BAR_WIDTH * (1 - (demPopVote / (demPopVote + repPopVote))) * -1;
            float endingPosition = startingPosition + distance - BAR_DEFAULT_LENGTH;

            _demBar.GetComponent<RectTransform>().offsetMin = new Vector2(startingPosition, BAR_OFFSET_BOTTOM); // Position left, Position Bottom
            _demBar.GetComponent<RectTransform>().offsetMax = new Vector2(endingPosition, BAR_TOP_OFFSET); // Position Right, Position Top


            // Rep vote bar
            startingPosition = -BAR_STARTING_RIGHT_POSITION;
            distance = TOTAL_BAR_WIDTH * (1 - (repPopVote / (demPopVote + repPopVote))) * -1;
            endingPosition = startingPosition + distance - BAR_DEFAULT_LENGTH;

            _repBar.GetComponent<RectTransform>().offsetMin = new Vector2(-endingPosition, BAR_OFFSET_BOTTOM); // Position left, Position Bottom
            _repBar.GetComponent<RectTransform>().offsetMax = new Vector2(-startingPosition, BAR_TOP_OFFSET); // Position Right, Position Top
        }

        void OnZero()
        {
            _demVotesText.text = $"0";
            _repVotesText.text = $"0";

            _demVotesPercentText.text = $"(0%)";
            _repVotesPercentText.text = $"(0%)";

            // Dem vote bar
            float startingPosition = -BAR_STARTING_RIGHT_POSITION;
            float distance = 0;
            float endingPosition = startingPosition + distance - BAR_DEFAULT_LENGTH;

            _demBar.GetComponent<RectTransform>().offsetMin = new Vector2(startingPosition, BAR_OFFSET_BOTTOM); // Position left, Position Bottom
            _demBar.GetComponent<RectTransform>().offsetMax = new Vector2(endingPosition, BAR_TOP_OFFSET); // Position Right, Position Top


            // Rep vote bar
            startingPosition = -BAR_STARTING_RIGHT_POSITION;
            distance = 0;
            endingPosition = startingPosition + distance - BAR_DEFAULT_LENGTH;

            _repBar.GetComponent<RectTransform>().offsetMin = new Vector2(-endingPosition, BAR_OFFSET_BOTTOM); // Position left, Position Bottom
            _repBar.GetComponent<RectTransform>().offsetMax = new Vector2(-startingPosition, BAR_TOP_OFFSET); // Position Right, Position Top
        }

        void CloseSelf()
        {
            gameObject.SetActive(false);
        }
    }
}