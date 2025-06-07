using System;
using UnityEngine;

namespace Controllers.SimulationScene.Simulation
{
    public enum ViewMode
    {
        LIKELIHOOD = 0,
        MARGINS = 1
    }

    [Serializable]
    [CreateAssetMenu(fileName = "SimulationSettings", menuName = "ScriptableObjects/SimulationScene/SimulationSettings")]
    public class SimulationSettingsSO : ScriptableObject
    {
        [SerializeField] public float _simulationSpeed;
        [SerializeField] public float _voteVariance;
        [SerializeField] public float _simulationErraticness;
        [SerializeField] public bool _bypassPolling;
        [SerializeField] public ViewMode _viewMode;

        private int startingYear = 2028;

        [SerializeField] public int year;

        private void Awake()
        {
            year = startingYear;
        }

        public void UpdateViewMode()
        {
            if(_viewMode == ViewMode.LIKELIHOOD) _viewMode = ViewMode.MARGINS;
            else _viewMode = ViewMode.LIKELIHOOD;
        }
    }
}