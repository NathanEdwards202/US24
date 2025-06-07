using Controllers.SimulationScene.Simulation;
using Delegates;
using System.Globalization;
using TMPro;
using UnityEngine;



namespace UI.SimulationScene
{
    public class TimeDisplay : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _dayText, _timeText, _timescaleText;
        [SerializeField] TMP_InputField _inputField;

        void OnEnable()
        {
            _inputField.onDeselect.AddListener(ResetInputField);
            _inputField.onSubmit.AddListener(OnInputFieldUpdate);
        }

        void OnDisable()
        {
            _inputField.onSubmit.RemoveAllListeners();
        }

        public void FrameUpdate(float currentTime, SimulationSettingsSO simulationSettings)
        {
            UpdateTimeText(currentTime);
            UpdateTimescaleText(simulationSettings);
        }

        void UpdateTimeText(float currentTime)
        {
            int days = (int)System.MathF.Floor(currentTime / 86400);

            _dayText.text = $"Day: {days + 1}";

            // Seperated for formatiing purposes
            _timeText.text = GetHMSTimeStringFromFloat(currentTime);
        }

        void UpdateTimescaleText(SimulationSettingsSO simulationSettings)
        {
            _timescaleText.text = $"TIMESCALE: {simulationSettings._simulationSpeed}x";
        }

        public static string GetHMSTimeStringFromFloat(float time)
        {
            time %= 86400;

            int hrs = (int)System.Math.Floor((time) / 3600);
            int mins = (int)System.Math.Floor((time - hrs * 3600) / 60);
            int secs = (int)System.Math.Floor((time - hrs * 3600 - mins * 60));

            string returnString = "";
            returnString += (hrs >= 10) ? hrs.ToString() : "0" + hrs.ToString();
            returnString += ":";
            returnString += (mins >= 10) ? mins.ToString() : "0" + mins.ToString();
            returnString += ":";
            returnString += (secs >= 10) ? secs.ToString() : "0" + secs.ToString();

            return returnString;
        }

        public static string GetHMTimeStringFromFloat(float time)
        {
            time %= 86400;

            int hrs = (int)System.Math.Floor((time) / 3600);
            int mins = (int)System.Math.Floor((time - hrs * 3600) / 60);

            string returnString = "";
            returnString += (hrs >= 10) ? hrs.ToString() : "0" + hrs.ToString();
            returnString += ":";
            returnString += (mins >= 10) ? mins.ToString() : "0" + mins.ToString();

            return returnString;
        }

        void OnInputFieldUpdate(string updatedText)
        {
            if (_inputField.wasCanceled)
            {
                _inputField.text = "";
                return;
            }

            float newTimescale = 1;

            try
            {
                newTimescale = float.Parse(updatedText, NumberStyles.Any);

                if(newTimescale < 1f) 
                    newTimescale = 1f;
                else if (newTimescale > 1800f) 
                    newTimescale = 1800f;
            }
            catch
            {
                _inputField.text = "";
                return;
            }

            UIDelegates.onTimescaleUpdate?.Invoke(newTimescale);
        }

        void ResetInputField(string unused)
        {
            _inputField.text = "";
        }
    }
}