using Controllers.SimulationScene.Simulation;
using UI.SimulationScene;
using UnityEngine;



namespace Controllers.SimulationScene.UI
{
    public class SimulationSceneUIController : MonoBehaviour
    {
        // Need frame updates
        [SerializeField] TimeDisplay _timeDisplay;
        [SerializeField] EventRelayer _eventRelayer;
        [SerializeField] MainBar _mainBar;
        [SerializeField] DetailedStatsDisplay _detailedStatsDisplay;

        [SerializeField] StartSimulationButton _startSimulationButton;
        [SerializeField] EndSimulationButtons _endSimulationButtons;

        // Do not need frame updates
        [SerializeField] ViewModeDisplay _viewModeDisplay;

        void Start()
        {
        }

        public void PollingUpdate()
        {
            _mainBar.UpdateText();
            _mainBar.UpdateBars();
            _detailedStatsDisplay.UpdateValues();
        }

        public void FrameUpdate(float currentTime, SimulationSettingsSO simulationSettings)
        {
            _timeDisplay.FrameUpdate(currentTime, simulationSettings);

            _mainBar.UpdateText();
            _mainBar.UpdateBars();
            _detailedStatsDisplay.UpdateValues();
        }
    }
}