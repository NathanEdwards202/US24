using States;
using UnityEngine;



namespace Delegates
{
    public class UIDelegates : MonoBehaviour
    {
        public delegate void OnTimescaleUpdate(float updatedTimescale);
        public static OnTimescaleUpdate onTimescaleUpdate;

        public delegate void OnStateButtonClicked(State state, bool polling = false);
        public static OnStateButtonClicked onStateButtonClicked;

        public delegate void OnBroadcastMessage(string messageToBroadcast);
        public static OnBroadcastMessage onBroadcastMessage;

        public delegate void OnUpdatedViewType();
        public static OnUpdatedViewType onUpdatedViewType;
    }
}