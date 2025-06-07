using Delegates;
using System.Collections.Generic;
using TMPro;
using UnityEngine;



namespace UI.SimulationScene
{
    public class EventRelayer : MonoBehaviour
    {
        [SerializeField] List<TextMeshProUGUI> _messages; // Make sure these are in order 1->6

        private void Start()
        {
            foreach (TextMeshProUGUI message in _messages)
            {
                message.text = "";
            }

            UIDelegates.onBroadcastMessage += OnNewBroadcast;

            OnNewBroadcast($"Simulation Begin!");
        }

        private void OnDestroy()
        {
            UIDelegates.onBroadcastMessage -= OnNewBroadcast;
        }

        void OnNewBroadcast(string newBroadcast)
        {
            for (int i = _messages.Count - 1; i > 0; i--)
            {
                _messages[i].text = _messages[i - 1].text;
            }

            _messages[0].text = newBroadcast;
        }
    }
}