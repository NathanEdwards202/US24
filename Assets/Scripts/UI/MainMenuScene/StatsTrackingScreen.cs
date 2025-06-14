using Data;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



namespace UI.MainMenuScene
{
    public class StatsTrackingScreen : MonoBehaviour
    {
        [SerializeField] GameObject _nextScreen;//, _lastScreen;
        [SerializeField] Button _forwardButton;//, _backButton;

        [SerializeField] GameObject _stateStatDisplay;
        [SerializeField] TMP_Dropdown _stateSelector;
        [SerializeField] GameObject _stateStatDisplayTextHolder;
        [SerializeField] TextMeshProUGUI _stateNameDisplay;
        TextMeshProUGUI[] _stateStatTextDisplays;

        [SerializeField] GameObject _topTwentyDisplay;
        [SerializeField] GameObject _topTwentyTextHolder;
        TextMeshProUGUI[] _topTwentyStatDisplays;

        void Start()
        {
            _forwardButton.onClick.AddListener(() => ChangeScreen(_nextScreen, gameObject));
            //_backButton.onClick.AddListener(() => ChangeScreen(_lastScreen));

            List<TMP_Dropdown.OptionData> options = new()
            {
                new("Overall State Data")
            };
            foreach (string stateName in StateData._currentData.Keys)
            {
                options.Add(new(stateName));
            }

            _stateSelector.AddOptions(options);

            _stateStatTextDisplays = _stateStatDisplayTextHolder.gameObject.GetComponentsInChildren<TextMeshProUGUI>();
            _topTwentyStatDisplays = _topTwentyTextHolder.gameObject.GetComponentsInChildren<TextMeshProUGUI>();


            if (_stateStatTextDisplays.Length != 16) Debug.LogError("Incorrect number of displays found for individual state displays");
            if (_topTwentyStatDisplays.Length != 20) Debug.LogError("Incorrect number of displays found for top twenty stats displays");

            SetStateToDisplay(0);
        }

        private void OnDestroy()
        {
            _forwardButton.onClick.RemoveAllListeners();
            //_backButton.onClick.RemoveAllListeners();

            _stateSelector.onValueChanged.RemoveAllListeners();
        }

        public void SetStateToDisplay(int value)
        {
            if (value == 0)
            {
                ChangeScreen(_topTwentyDisplay, _stateStatDisplay);
                SetTopTwentyText();
            }
            else
            {
                ChangeScreen(_stateStatDisplay, _topTwentyDisplay);
                SetStateText(_stateSelector.options[value].text);
            }
        }

        void ChangeScreen(GameObject newScreen, GameObject otherScreen)
        {
            newScreen.SetActive(true);
            otherScreen.SetActive(false);
        }

        /*struct DataComparison
        {
            public DataComparison(string name, string comparisonPoint, double newValue, double percentageChange)
            {
                _name = name;
                _comparisonPoint = comparisonPoint;
                _newValue = newValue;
                _percentageChange = percentageChange;
            }

            string _name;
            string _comparisonPoint;
            double _newValue;
            double _percentageChange;

            public string DataComparisonToString()
            {
                string preface = _percentageChange > 0 ? "+" : "";
                return $"{_name}'s {_comparisonPoint}: {_newValue:N0} ({preface}{_percentageChange:N2}%)";

            }
        }*/

        void SetTopTwentyText()
        {
            List<(string, string, double, double, bool)> data = new(); // Name, Comparison point, raw number, % change, is displayed as a percentage

            foreach (string state in StateData._oldData.Keys)
            {
                StateDataStruct oldData = StateData._oldData[state];
                StateDataStruct newData = StateData._currentData[state];
                StateDataStruct dataChange = StateData._changesInData[state];

                data.Add(
                    new(
                        state,
                        "Electoral Votes",
                        newData._electoralVotes,
                        (float)dataChange._electoralVotes / (float)oldData._electoralVotes,
                        false
                        )
                    );

                data.Add(
                    new(
                        state,
                        "% Democratic Registration",
                        newData._demReg * 100f,
                        dataChange._demReg / oldData._demReg,
                        true
                        )
                    );
                data.Add(
                    new(
                        state,
                        "% Republican Registration",
                        newData._repReg * 100f,
                        dataChange._repReg / oldData._repReg,
                        true
                        )
                    );
                data.Add(
                    new(
                        state,
                        "Population",
                        newData._pop,
                        (float)dataChange._pop / (float)oldData._pop,
                        false
                        )
                    );

                data.Add(
                    new(
                        state,
                        "% Population Registered to Vote",
                        newData._regPopPercent * 100f,
                        dataChange._regPopPercent / oldData._regPopPercent,
                        true
                        )
                    );
                data.Add(
                    new(
                        state,
                        "% Population U30",
                        newData._youth * 100f,
                        dataChange._youth / oldData._youth,
                        true
                        )
                    );
                data.Add(
                    new(
                        state,
                        "% Population 30 - 44",
                        newData._lowerM * 100f,
                        dataChange._lowerM / oldData._lowerM,
                        true
                        )
                    );
                data.Add(
                    new(
                        state,
                        "% Population 45 - 59",
                        newData._upperM * 100f,
                        dataChange._upperM / oldData._upperM,
                        true
                        )
                    );
                data.Add(
                    new(
                        state,
                        "% Population 60+",
                        newData._eld * 100f,
                        dataChange._eld / oldData._eld,
                        true
                        )
                    );
                data.Add(
                    new(
                        state,
                        "% Population Urban",
                        newData._urb * 100f,
                        dataChange._urb / oldData._urb,
                        true
                        )
                    );
                data.Add(
                    new(
                        state,
                        "% Population Rural",
                        1 - newData._urb * 100f,
                        ((1 - newData._urb) - (1 - oldData._urb)) / (1 - oldData._urb),
                        true
                        )
                    );
                data.Add(
                    new(
                        state,
                        "% Population Working Class",
                        (1 - newData._mid - newData._rich) * 100f,
                        ((1 - newData._mid - newData._rich) - (1 - oldData._mid - oldData._rich)) / (1 - oldData._mid - oldData._rich),
                        true
                        )
                    );
                data.Add(
                    new(
                        state,
                        "% Population Middle Class",
                        newData._mid * 100f,
                        dataChange._mid / oldData._mid,
                        true
                        )
                    );
                data.Add(
                    new(
                        state,
                        "% Population Rich",
                        newData._rich * 100f,
                        dataChange._rich / oldData._rich,
                        true
                        )
                    );
                data.Add(
                    new(
                        state,
                        "% Population Non-PoC",
                        newData._nonPoC * 100f,
                        dataChange._nonPoC / oldData._nonPoC,
                        true
                        )
                    );
                data.Add(
                    new(
                        state,
                        "% Population PoC",
                        (1 - newData._nonPoC) * 100f,
                        ((1 - newData._nonPoC) - (1 - oldData._nonPoC)) / (1 - oldData._nonPoC),
                        true
                        )
                    );
            }


            data = data
                .Where(x => !double.IsNaN(x.Item4) && !double.IsInfinity(x.Item4) && !(x.Item4 == 0))
                .OrderByDescending(x => x.Item4)
                .ToList();

            string rawValueDisplay, percentageChangePreface;

            for(int i =  0; i < _topTwentyStatDisplays.Length/2; i++)
            {

                rawValueDisplay = data[i].Item5 == false ? data[i].Item3.ToString("N0") : data[i].Item3.ToString("N2") + "%";
                percentageChangePreface = data[i].Item4 >= 0 ? "+" : "";

                _topTwentyStatDisplays[i].text = $"{data[i].Item1}'s {data[i].Item2}: {rawValueDisplay} ({percentageChangePreface}{(data[i].Item4 * 100):N2}%)";
                _topTwentyStatDisplays[i].ForceMeshUpdate();
            }

            data.Reverse();

            int index;
            for (int i = _topTwentyStatDisplays.Length / 2; i < _topTwentyStatDisplays.Length; i++)
            {
                index = i - _topTwentyStatDisplays.Length / 2;
                rawValueDisplay = data[index].Item5 == false ? data[index].Item3.ToString("N0") : data[index].Item3.ToString("N2") + "%";
                percentageChangePreface = data[index].Item4 >= 0 ? "+" : "";

                _topTwentyStatDisplays[i].text = $"{data[index].Item1}'s {data[index].Item2}: {rawValueDisplay} ({percentageChangePreface}{(data[index].Item4 * 100):N2}%)";
                _topTwentyStatDisplays[i].ForceMeshUpdate();
            }
        }


        // This violates DRY so badly and should absolutely be replaced
        // While I'm at it, the string in this, for SetTopTwentyText should absolutely be Enums
        void SetStateText(string stateName)
        {
            List<(string, double, double, bool)> data = new(); // Comparison point, raw number, % change, is displayed as a percentage

            StateDataStruct oldData = StateData._oldData[stateName];
            StateDataStruct newData = StateData._currentData[stateName];
            StateDataStruct dataChange = StateData._changesInData[stateName];

            data.Add(
                    new(
                        "Electoral Votes",
                        newData._electoralVotes,
                        (float)dataChange._electoralVotes / (float)oldData._electoralVotes,
                        false
                        )
                    );

            data.Add(
                new(
                    "% Democratic Registration",
                    newData._demReg * 100f,
                    dataChange._demReg / oldData._demReg,
                    true
                    )
                );
            data.Add(
                new(
                    "% Republican Registration",
                    newData._repReg * 100f,
                    dataChange._repReg / oldData._repReg,
                    true
                    )
                );
            data.Add(
                new(
                    "Population",
                    newData._pop,
                    (float)dataChange._pop / (float)oldData._pop,
                    false
                    )
                );

            data.Add(
                new(
                    "% Population Registered to Vote",
                    newData._regPopPercent * 100f,
                    dataChange._regPopPercent / oldData._regPopPercent,
                    true
                    )
                );
            data.Add(
                new(
                    "% Population U30",
                    newData._youth * 100f,
                    dataChange._youth / oldData._youth,
                    true
                    )
                );
            data.Add(
                new(
                    "% Population 30 - 44",
                    newData._lowerM * 100f,
                    dataChange._lowerM / oldData._lowerM,
                    true
                    )
                );
            data.Add(
                new(
                    "% Population 45 - 59",
                    newData._upperM * 100f,
                    dataChange._upperM / oldData._upperM,
                    true
                    )
                );
            data.Add(
                new(
                    "% Population 60+",
                    newData._eld * 100f,
                    dataChange._eld / oldData._eld,
                    true
                    )
                );
            data.Add(
                new(
                    "% Population Urban",
                    newData._urb * 100f,
                    dataChange._urb / oldData._urb,
                    true
                    )
                );
            data.Add(
                new(
                    "% Population Rural",
                    1 - newData._urb * 100f,
                    ((1 - newData._urb) - (1 - oldData._urb)) / (1 - oldData._urb),
                    true
                    )
                );
            data.Add(
                new(
                    "% Population Working Class",
                    (1 - newData._mid - newData._rich) * 100f,
                    ((1 - newData._mid - newData._rich) - (1 - oldData._mid - oldData._rich)) / (1 - oldData._mid - oldData._rich),
                    true
                    )
                );
            data.Add(
                new(
                    "% Population Middle Class",
                    newData._mid * 100f,
                    dataChange._mid / oldData._mid,
                    true
                    )
                );
            data.Add(
                new(
                    "% Population Rich",
                    newData._rich * 100f,
                    dataChange._rich / oldData._rich,
                    true
                    )
                );
            data.Add(
                new(
                    "% Population Non-PoC",
                    newData._nonPoC * 100f,
                    dataChange._nonPoC / oldData._nonPoC,
                    true
                    )
                );
            data.Add(
                new(
                    "% Population PoC",
                    (1 - newData._nonPoC) * 100f,
                    ((1 - newData._nonPoC) - (1 - oldData._nonPoC)) / (1 - oldData._nonPoC),
                    true
                    )
                );

            _stateNameDisplay.text = stateName;
            _stateNameDisplay.ForceMeshUpdate();

            // 16 points of data.
            string rawValueDisplay, percentageChangePreface;

            for (int i = 0; i < _stateStatTextDisplays.Length; i++)
            {
                rawValueDisplay = data[i].Item4 == false ? data[i].Item3.ToString("N0") : data[i].Item3.ToString("N2") + "%";
                percentageChangePreface = data[i].Item3 >= 0 ? "+" : "";

                _stateStatTextDisplays[i].text = $"{stateName}'s {data[i].Item1}: {rawValueDisplay} ({percentageChangePreface}{(data[i].Item3 * 100):N2}%)";
                _stateStatTextDisplays[i].ForceMeshUpdate();
            }
        }
    }
}