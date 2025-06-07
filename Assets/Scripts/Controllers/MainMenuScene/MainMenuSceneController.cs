using Candidates;
using System.Collections;
using UI.MainMenuScene;
using UnityEngine;



namespace Controllers.MainScene
{
    public class MainMenuSceneController : MonoBehaviour
    {
        [SerializeField] CandidateSO _democratCandidate;
        [SerializeField] CandidateSO _republicanCandidate;

        [SerializeField] CandidateCreator _demCreator, _repCreator;

        private void Start()
        {
            // Fuck IDK
            // You have to do this the frame after it activates or it doesn't work
            _demCreator.gameObject.SetActive(true);
            _repCreator.gameObject.SetActive(true);
            StartCoroutine(DoStuffInASec());
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
}