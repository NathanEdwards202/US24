using States;
using UnityEngine;



namespace Delegates
{
    public class GameFlowDelegates : MonoBehaviour
    {
        public delegate void OnPollingEnd();
        public static OnPollingEnd onPollingEnd;

        public delegate void OnWinnerHasBeenDetermined();
        public static OnWinnerHasBeenDetermined onWinnerHasBeenDetermined;

        public delegate void OnSkip();
        public static OnSkip onSkip;

        public delegate void OnEveryVoteCounted();
        public static OnEveryVoteCounted onEveryVoteCounted;

        public delegate void OnReturnToMainMenu();
        public static OnReturnToMainMenu onReturnToMainMenu;
    }
}