using Candidates;
using System;
using TMPro;
using UnityEngine;



namespace UI.SimulationScene
{
    public class MainBar : MonoBehaviour
    {
        [SerializeField] Candidate _democrat, _republican;

        [SerializeField] GameObject _demConBar, _demLeanBar, _repLeanBar, _repConBar;
        [SerializeField] TextMeshProUGUI _demConBarText, _demLeanBarText, _repLeanBarText, _repConBarText;
        [SerializeField] TextMeshProUGUI _demCandidateText, _repCandidateText;

        const float TOTAL_BAR_WIDTH = 1900f;
        const float BAR_TOP_OFFSET = 65f, BAR_OFFSET_BOTTOM = 35f, BAR_STARTING_RIGHT_POSITION = 900f;
        const float BAR_DEFAULT_LENGTH = 100f;
        float perElectoralVoteWidthPercent;

        void Start()
        {
            perElectoralVoteWidthPercent = 1f / 538f;
        }

        public void UpdateText()
        {
            float demPopVote = MathF.Floor(_democrat.GetPopularVote());
            float repPopVote = MathF.Floor(_republican.GetPopularVote());

            if (demPopVote == 0 || repPopVote == 0)
            {
                OnZero();
                return;
            }

            _demCandidateText.text = $"{_democrat.GetName()}\nPOPULAR VOTE: {((double)demPopVote).ToString("N0")} ({(demPopVote / (demPopVote + repPopVote) * 100f).ToString("N2")}%)";
            _repCandidateText.text = $"{_republican.GetName()}\nPOPULAR VOTE: {((double)repPopVote).ToString("N0")} ({(repPopVote / (demPopVote + repPopVote) * 100f).ToString("N2")}%)";

            if (_democrat.GetConfirmedElectoralVotes() > 0)
            {
                _demConBarText.text = $"{_democrat.GetConfirmedElectoralVotes()}";
            }
            else
            {
                _demConBarText.text = "";
            }

            if (_democrat.GetLeaningElectoralVotes() > 0)
            {
                _demLeanBarText.text = $"{_democrat.GetLeaningElectoralVotes()}";
            }
            else
            {
                _demLeanBarText.text = "";
            }

            if (_republican.GetConfirmedElectoralVotes() > 0)
            {
                _repConBarText.text = $"{_republican.GetConfirmedElectoralVotes()}";
            }
            else
            {
                _repConBarText.text = "";
            }

            if (_republican.GetLeaningElectoralVotes() > 0)
            {
                _repLeanBarText.text = $"{_republican.GetLeaningElectoralVotes()}";
            }
            else
            {
                _repLeanBarText.text = "";
            }
        }

        void OnZero()
        {
            _repLeanBarText.text = "";
            _demConBarText.text = "";

            _demCandidateText.text = $"{_democrat.GetName()}\nPOPULAR VOTE: 0 (0%)";
            _repCandidateText.text = $"{_republican.GetName()}\nPOPULAR VOTE: 0 (0%)";
        }

        public void UpdateBars()
        {
            float demConfirmedBarWidthPercent = 0, demLeanBarWidthPercent = 0, repLeanBarWidthPercent = 0, repConfirmedBarWidthPercent;

            demConfirmedBarWidthPercent = perElectoralVoteWidthPercent * _democrat.GetConfirmedElectoralVotes();
            demLeanBarWidthPercent = perElectoralVoteWidthPercent * _democrat.GetLeaningElectoralVotes();

            repLeanBarWidthPercent = perElectoralVoteWidthPercent * _republican.GetLeaningElectoralVotes();
            repConfirmedBarWidthPercent = perElectoralVoteWidthPercent * _republican.GetConfirmedElectoralVotes();


            // Democrat Confimed EVs
            float startingPosition = -BAR_STARTING_RIGHT_POSITION;
            float distance = TOTAL_BAR_WIDTH * demConfirmedBarWidthPercent;
            float endingPosition = startingPosition + distance - BAR_DEFAULT_LENGTH;

            _demConBar.GetComponent<RectTransform>().offsetMin = new Vector2(startingPosition, BAR_OFFSET_BOTTOM); // Position left, Position Bottom
            _demConBar.GetComponent<RectTransform>().offsetMax = new Vector2(endingPosition, BAR_TOP_OFFSET); // Position Right, Position Top

            // Republican Leaning EVs
            startingPosition = endingPosition + BAR_DEFAULT_LENGTH;
            distance = TOTAL_BAR_WIDTH * demLeanBarWidthPercent;
            endingPosition = startingPosition + distance - BAR_DEFAULT_LENGTH;

            _demLeanBar.GetComponent<RectTransform>().offsetMin = new Vector2(startingPosition, BAR_OFFSET_BOTTOM); // Position left, Position Bottom
            _demLeanBar.GetComponent<RectTransform>().offsetMax = new Vector2(endingPosition, BAR_TOP_OFFSET); // Position Right, Position Top



            // Democrat Confimed EVs
            startingPosition = -BAR_STARTING_RIGHT_POSITION;
            distance = TOTAL_BAR_WIDTH * repConfirmedBarWidthPercent;
            endingPosition = startingPosition + distance - BAR_DEFAULT_LENGTH;

            _repConBar.GetComponent<RectTransform>().offsetMin = new Vector2(-endingPosition, BAR_OFFSET_BOTTOM); // Position left, Position Bottom
            _repConBar.GetComponent<RectTransform>().offsetMax = new Vector2(-startingPosition, BAR_TOP_OFFSET); // Position Right, Position Top

            // Republican Leaning EVs
            startingPosition = endingPosition + BAR_DEFAULT_LENGTH;
            distance = TOTAL_BAR_WIDTH * repLeanBarWidthPercent;
            endingPosition = startingPosition + distance - BAR_DEFAULT_LENGTH;

            _repLeanBar.GetComponent<RectTransform>().offsetMin = new Vector2(-endingPosition, BAR_OFFSET_BOTTOM); // Position left, Position Bottom
            _repLeanBar.GetComponent<RectTransform>().offsetMax = new Vector2(-startingPosition, BAR_TOP_OFFSET); // Position Right, Position Top
        }
    }
}