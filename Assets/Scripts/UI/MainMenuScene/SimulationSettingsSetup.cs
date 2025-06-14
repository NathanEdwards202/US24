using Controllers.SimulationScene.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SimulationSettingsSetup : MonoBehaviour
{
    [SerializeField] SimulationSettingsSO _simulationSettings;

    [SerializeField] GameObject _lastScreen;
    [SerializeField] Button _backButton, _startButton, _resetButton;

    [SerializeField] Button _pollingToggleButton;
    [SerializeField] TextMeshProUGUI _pollingToggleButtonText;

    [SerializeField] Slider _simSpeedSlider, _voteVarianceSlider, _erraticnessSlider;
    [SerializeField] TextMeshProUGUI _simSpeedText, _voteVarianceText, _erraticnessText;

    private void Start()
    {
        SetToDefaults();

        _backButton.onClick.AddListener(BackButton);
        _startButton.onClick.AddListener(StartSimulation);
        _resetButton.onClick.AddListener(SetToDefaults);

        _pollingToggleButton.onClick.AddListener(SetPolling);

        _simSpeedSlider.onValueChanged.AddListener(SetTexts);
        _voteVarianceSlider.onValueChanged.AddListener(SetTexts);
        _erraticnessSlider.onValueChanged.AddListener(SetTexts);
    }

    private void OnDestroy()
    {
        _backButton.onClick.RemoveAllListeners();
        _startButton.onClick.RemoveAllListeners();
        _resetButton.onClick.RemoveAllListeners();

        _pollingToggleButton.onClick.RemoveAllListeners();

        _simSpeedSlider.onValueChanged.RemoveAllListeners();
        _voteVarianceSlider.onValueChanged.RemoveAllListeners();
        _erraticnessSlider.onValueChanged.RemoveAllListeners();
    }

    void SetToDefaults()
    {
        _simSpeedSlider.value = 1f;
        _voteVarianceSlider.value = 10f;
        _erraticnessSlider.value = 5f;

        SetTexts(0f);
    }

    void BackButton()
    {
        _lastScreen.SetActive(true);
        gameObject.SetActive(false);
    }

    void SetTexts(float unused)
    {
        _simSpeedText.text = $"SIMULATION SPEED: {_simSpeedSlider.value}";
        _voteVarianceText.text = $"VARIANCE: {_voteVarianceSlider.value}";
        _erraticnessText.text = $"ERRATICNESS: {_erraticnessSlider.value}";
        _pollingToggleButtonText.text =
            "Polling: " + (_simulationSettings._bypassPolling ? "OFF" : "ON");
    }

    void SetPolling()
    {
        _simulationSettings._bypassPolling = !_simulationSettings._bypassPolling;
        SetTexts(0);
    }

    void StartSimulation()
    {
        _simulationSettings._simulationSpeed = _simSpeedSlider.value;
        _simulationSettings._voteVariance = _voteVarianceSlider.value;
        _simulationSettings._simulationErraticness = _erraticnessSlider.value;

        SceneManager.LoadScene(1);
    }
}
