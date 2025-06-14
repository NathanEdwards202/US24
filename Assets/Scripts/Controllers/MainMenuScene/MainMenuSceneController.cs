using Candidates;
using System.Collections;
using UI.MainMenuScene;
using UnityEngine;
using UnityEngine.UI;



namespace Controllers.MainScene
{
    public class MainMenuSceneController : MonoBehaviour
    {
        [SerializeField] CandidateSO _democratCandidate;
        [SerializeField] CandidateSO _republicanCandidate;

        [SerializeField] CandidateCreator _demCreator, _repCreator;

        // _previousResultsTrackerBackButton IS NEEDED SO YOU CANNOT GO BACK TO PREVIOUS STATS FROM THE FIRST SIMULATION
        // The others are needed to determine the correct results screen to go to.
        [SerializeField] GameObject _previousResultsScreen, _statChangeTrackingScreen, _previousResultsTrackerBackButton;

        private void Start()
        {
            // Fuck IDK
            // You have to do this the frame after it activates or it doesn't work
            // Probably something to do with messing up the ordering of Awake and Start in other files, this works, no complaints so far
            _demCreator.gameObject.SetActive(true);
            _repCreator.gameObject.SetActive(true);
            StartCoroutine(DoStuffInASec());

            if (MainMenuSetupControls._firstSim)
            {
                _statChangeTrackingScreen.SetActive(false);
                _previousResultsTrackerBackButton.SetActive(false);
            }
            else
            {
                _previousResultsScreen.SetActive(false);
            }
        }

        IEnumerator DoStuffInASec()
        {
            yield return null;
            
            PopulateCandidates();
            _demCreator.gameObject.SetActive(false);
            _repCreator.gameObject.SetActive(false);
        }

        void PopulateCandidates()
        {
            _demCreator.SetupFromTemplate(_democratCandidate);
            _repCreator.SetupFromTemplate(_republicanCandidate);
        }
    }

    public static class MainMenuSetupControls
    {
        public static bool _firstSim { get; private set; } = true;

        public static void OnSimulationFinish() { _firstSim = false; }
    }
}