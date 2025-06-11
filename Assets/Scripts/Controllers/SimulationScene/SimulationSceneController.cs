using Candidates;
using Controllers.MainScene;
using Controllers.MainScene.ResultsTracker;
using Controllers.SimulationScene.Simulation;
using Controllers.SimulationScene.UI;
using Data;
using Delegates;
using States;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.SimulationScene;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Controllers.SimulationScene
{
    public class SimulationSceneController : MonoBehaviour
    {
        // Simulation Settings
        [SerializeField] SimulationSettingsSO _simulationSettings;
        bool _simulating = false;
        bool _finished = false;
        bool _winnerDetermined;
        bool _draw;

        // Results Holder
        [SerializeField] ResultsTrackerSO _resultsTracker;


        // Time handling
        float _currentTime;


        // Simulation Objects
        [SerializeField] GameObject _candidateHolder, _statesHolder;
        List<Candidate> _candidates;
        Candidate _confirmedWinner;
        List<State> _states;

        bool polling = true;

        bool _pollsToClose = true;
        List<bool> _pollClosures = new List<bool> { true, true, true, true, true, true, true, true, true, true, true }; // 19:00, 19:30, 20:00, 20:15, 20:30, 21:00, 22:00, 22:30, 23:00, 00:00, 01:00
                                                                                                                  // 11 bools, for each time period
        bool _allStatesCalled = false;


        // UI
        [SerializeField] SimulationSceneUIController _UIController;

        void Start()
        {
            _winnerDetermined = false;
            _draw = false;


            _candidates = _candidateHolder.transform.GetComponentsInChildren<Candidate>().OrderBy(candidate => candidate.GetParty()).ToList(); // Ensure sorted so that democrat is first

            _states = _statesHolder.transform.GetComponentsInChildren<State>().ToList(); // Order makes no difference

            SetupData();

            if (_simulationSettings._bypassPolling)
            {
                StartPolling();

                // Yeah, important stuff is setup in polling
                // I could move it... But no
                // But I'm not sure if immediately calling this breaks things or not, we'll see
                // Might have to rework some shit
                GameFlowDelegates.onPollingEnd.Invoke();
                //EndPolling();
            }
            else
            {
                StartPolling();
            }
        }

        void OnDisable()
        {
            // Delegate removal
            UIDelegates.onTimescaleUpdate -= OnTimescaleUpdate;
            UIDelegates.onUpdatedViewType -= OnViewTypeUpdate;

            UIDelegates.onStateButtonClicked -= TEST;

            GameFlowDelegates.onPollingEnd -= EndPolling;

            GameFlowDelegates.onSkip -= OnSkip;
            GameFlowDelegates.onEveryVoteCounted -= OnAllCounted;
            GameFlowDelegates.onReturnToMainMenu -= OnReturnToMainMenu;
        }

        void StartPolling()
        {
            Parallel.ForEach(_states, state =>
            {
                state.CalculateVotesPolling(_candidates[0], _candidates[1]);
                state.SetCalled(true);
            });

            SetCandidatesElectoralVotes();

            // Initial pass on UI
            foreach (State state in _states)
            {
                state.SetLean(ViewMode.MARGINS);
                state.SetColour();
            }
            _UIController.PollingUpdate();

            // Delegates
            UIDelegates.onStateButtonClicked += TEST;
            GameFlowDelegates.onPollingEnd += EndPolling;
        }

        void EndPolling()
        {
            StartSimulation();
            polling = false;
        }

        void SetupData()
        {
            StateData.InitialSetup();

            foreach(State state in _states)
            {
                state.SetupFromStateData(StateData._currentData[state.Name]);
            }
        }

        void StartSimulation()
        {
            // Setup votes
            Parallel.ForEach(_states, state =>
            {
                state.CalculateVotes(_candidates[0], _candidates[1], _simulationSettings._voteVariance);
                state.SetCalled(false);
            });
            SetCandidatesElectoralVotes();
            _UIController.PollingUpdate();

            int demWin = 0;
            int repWin = 0;

            foreach(State state in _states)
            {
                if (state._demVotes >= state._repVotes)
                {
                    demWin += state._electoralVotes;
                }
                else
                {
                    repWin += state._electoralVotes;
                }
            }

            if (demWin >= 270)
            {
                _confirmedWinner = _candidates[0];
            }
            else if (repWin >= 270)
            {
                _confirmedWinner = _candidates[1];
            }
            else
            {
                _confirmedWinner = null;
                _draw = true;
            }

            // Initial pass on UI
            foreach (State state in _states)
            {
                state.SetLean(ViewMode.MARGINS);
                state.SetColour();
            }
            _UIController.FrameUpdate(_currentTime, _simulationSettings);

            //_simulating = true;
            _currentTime = 68400; // 9pm, 19:00, 19hours in seconds.
            _currentTime -= 15;

            // Delegates
            UIDelegates.onTimescaleUpdate += OnTimescaleUpdate;
            UIDelegates.onUpdatedViewType += OnViewTypeUpdate;

            GameFlowDelegates.onSkip += OnSkip;
            GameFlowDelegates.onEveryVoteCounted += OnAllCounted;
            GameFlowDelegates.onReturnToMainMenu += OnReturnToMainMenu;

            GameFlowDelegates.onPollingEnd -= EndPolling;
        }

        void TEST(State state, bool polling = false)
        {
            Debug.Log($"{state.GetName()} clicked!");
        }

        void Update()
        {
            // Return if not simulating
            if (_finished || polling)
                return;

            //Debug.Log(1 / Time.deltaTime); 

            // Update Time
            float deltaTime = Time.deltaTime;
            _currentTime += _simulationSettings._simulationSpeed * deltaTime;

            // Check State Availability
            foreach(State state in _states)
            {
                state.SetStart(_currentTime);
            }

            if (_pollsToClose) CheckPollClosureForBroadcast();

            if (!_simulating)
            {
                _UIController.FrameUpdate(_currentTime, _simulationSettings);
            }

            // Update each state
            Parallel.ForEach(_states, state =>
            {
                state.FrameUpdate(deltaTime, _simulationSettings._simulationErraticness, _simulationSettings._simulationSpeed, _simulationSettings._viewMode);
            });

            // This part has to be done in series due to thread access abilities
            foreach (State state in _states)
            {
                int stateCall = state.CheckStateCalled();

                if (stateCall != 0 && !state.GetIsCalled())
                {
                    if (stateCall <= 1) // Call for Dem
                        UIDelegates.onBroadcastMessage?.Invoke($"{TimeDisplay.GetHMTimeStringFromFloat(_currentTime)} - {state.GetName()} has been called for {_candidates[0].GetName()} with its {state.GetElectoralVotes()} electoral votes.");

                    else // Call for Reps
                        UIDelegates.onBroadcastMessage?.Invoke($"{TimeDisplay.GetHMTimeStringFromFloat(_currentTime)} - {state.GetName()} has been called for {_candidates[1].GetName()} with its {state.GetElectoralVotes()} electoral votes.");

                    state.SetCalled(true);
                }

                else if (!state.GetCountringFinished() && state.GetIsCalled())
                {
                    if (stateCall < 3 && state.GetCallDirection() > 0) // Rep win retracted
                    {
                        UIDelegates.onBroadcastMessage?.Invoke($"{TimeDisplay.GetHMTimeStringFromFloat(_currentTime)} - {state.GetName()}'s call for {_candidates[1].GetName()} has been retracted.");
                        state.SetCalled(false);
                    }

                    else if(stateCall > -3 && state.GetCallDirection() < 0) // Dem win retracted
                    {
                        UIDelegates.onBroadcastMessage?.Invoke($"{TimeDisplay.GetHMTimeStringFromFloat(_currentTime)} - {state.GetName()}'s call for {_candidates[0].GetName()} has been retracted.");
                        state.SetCalled(false);
                    }
                }
            }
            if (!_allStatesCalled) CheckAllStatesCalled();
            
            SetCandidatesElectoralVotes(); // Set stats needed for UI, after the state leanings checks take place 


            // Update UI
            foreach (State state in _states)
            {
                state.SetColour();
            }
            _UIController.FrameUpdate(_currentTime, _simulationSettings);

            // Check if finished
            bool anyUnfinished = false;
            Parallel.ForEach(_states, state =>
            {
                if (state.GetCountringFinished() == false) anyUnfinished = true;
            });

            if (!_winnerDetermined &&
                (_candidates[0]._confirmedElectoralVotes >= 270 && _confirmedWinner == _candidates[0]
                || _candidates[1]._confirmedElectoralVotes >= 270 && _confirmedWinner == _candidates[1]
                || (_draw && _candidates[0]._confirmedElectoralVotes == 269 && _candidates[1]._confirmedElectoralVotes == 269)))
            {
                _winnerDetermined = true;
                GameFlowDelegates.onWinnerHasBeenDetermined?.Invoke();
            }

            if (!anyUnfinished)
            {
                GameFlowDelegates.onEveryVoteCounted?.Invoke();
                _finished = true;
            }                
        }

        void OnTimescaleUpdate(float newTimescale)
        {
            _simulationSettings._simulationSpeed = newTimescale;
        }

        void OnViewTypeUpdate()
        {
            foreach (State state in _states)
            {
                state.SetLean(_simulationSettings._viewMode);

                state.SetColour();
            }
            _UIController.FrameUpdate(_currentTime, _simulationSettings);
        }

        void CheckPollClosureForBroadcast()
        {
            if (_currentTime > 68400 && _pollClosures[0] == true) // 19:00
            {
                _pollClosures[0] = false;
                _simulating = true;

                UIDelegates.onBroadcastMessage?.Invoke($"{TimeDisplay.GetHMTimeStringFromFloat(_currentTime)} - Georgia, Indiana, Kentucky, South Carolina, Vermont and Virginia polls closed.");
            }

            else if (_currentTime > 70200 && _pollClosures[1] == true) // 19:30
            {
                _pollClosures[1] = false;

                UIDelegates.onBroadcastMessage?.Invoke($"{TimeDisplay.GetHMTimeStringFromFloat(_currentTime)} - Florida, New Hampshire, Ohio and West Virginia polls closed.");
            }

            else if(_currentTime > 72000 && _pollClosures[2] == true) // 20:00
            {
                _pollClosures[2] = false;

                UIDelegates.onBroadcastMessage?.Invoke($"{TimeDisplay.GetHMTimeStringFromFloat(_currentTime)} - Alabama, Colorado, Connecticut, Delaware, Washington DC, Illinois, Maine, Maryland, Massachusetts, Mississippi, Missouri, New Jersey, Oklahoma, Pennsylvania, Rhode Island and Tennessee polls closed.");
            }

            else if(_currentTime > 72900 && _pollClosures[3] == true) // 20:15
            {
                _pollClosures[3] = false;

                UIDelegates.onBroadcastMessage?.Invoke($"{TimeDisplay.GetHMTimeStringFromFloat(_currentTime)} - North Carolina polls closed.");
            }

            else if(_currentTime > 73800 && _pollClosures[4] == true) // 20:30
            {
                _pollClosures[4] = false;

                UIDelegates.onBroadcastMessage?.Invoke($"{TimeDisplay.GetHMTimeStringFromFloat(_currentTime)} - Arkansas, Michigan, North Dakota. South Dakota and Texas polls closed.");
            }

            else if(_currentTime > 75600 && _pollClosures[5] == true) // 21:00
            {
                _pollClosures[5] = false;

                UIDelegates.onBroadcastMessage?.Invoke($"{TimeDisplay.GetHMTimeStringFromFloat(_currentTime)} - Arizona, Kansas, Louisiana, Minnesota, Nebraska, New Mexico, New York, Wisconsin and Wyoming polls closed.");
            }

            else if(_currentTime > 79200 && _pollClosures[6] == true) // 22:00
            {
                _pollClosures[6] = false;

                UIDelegates.onBroadcastMessage?.Invoke($"{TimeDisplay.GetHMTimeStringFromFloat(_currentTime)} - Iowa, Montana, Nevada and Utah polls closed.");
            }

            else if (_currentTime > 81000 && _pollClosures[7] == true) // 22:30
            {
                _pollClosures[7] = false;

                UIDelegates.onBroadcastMessage?.Invoke($"{TimeDisplay.GetHMTimeStringFromFloat(_currentTime)} - Idaho and Oregon polls closed.");
            }

            else if(_currentTime > 82800 && _pollClosures[8] == true) // 23:00
            {
                _pollClosures[8] = false;

                UIDelegates.onBroadcastMessage?.Invoke($"{TimeDisplay.GetHMTimeStringFromFloat(_currentTime)} - California and Washington polls closed.");
            }

            else if(_currentTime > 86400 && _pollClosures[9] == true) // 00:00
            {
                _pollClosures[9] = false;

                UIDelegates.onBroadcastMessage?.Invoke($"{TimeDisplay.GetHMTimeStringFromFloat(_currentTime)} - Hawaii polls closed.");
            }

            else if(_currentTime > 90000 && _pollClosures[10] == true) // 01:00
            {
                _pollClosures[10] = false;
                _pollsToClose = false;

                UIDelegates.onBroadcastMessage?.Invoke($"{TimeDisplay.GetHMTimeStringFromFloat(_currentTime)} - Alaska polls closed. All polls closed.");
            }
        }

        void CheckAllStatesCalled()
        {
            foreach(State state in _states)
            {
                if (!state.GetIsCalled())
                {
                    return;
                }
            }

            _allStatesCalled = true;

            UIDelegates.onBroadcastMessage($"{TimeDisplay.GetHMTimeStringFromFloat(_currentTime)} - All states have been called, counting will continue but results are now known.");
        }

        void SetCandidatesElectoralVotes()
        {
            float demPopVote = 0, repPopVote = 0;
            int demConfirmed = 0, demLean = 0, repLean = 0, repConfirmed = 0;

            foreach (State state in _states)
            {
                demPopVote += state.GetCurrentVotes()[0];
                repPopVote += state.GetCurrentVotes()[1];

                if (state.GetIsCalled())
                {
                    if (state.GetDemLean() > 0)
                    {
                        demConfirmed += state.GetElectoralVotes();
                        continue;
                    }
                    else
                    {
                        repConfirmed += state.GetElectoralVotes();
                        continue;
                    }
                }

                float[] votingData = state.GetCurrentVotingData();

                switch (state.GetLeanLikelihood(votingData[0], votingData[1], votingData[2]))
                {
                    case LeanState.LEAN_DEM or LeanState.LIKELY_DEM or LeanState.CERTAIN_DEM:
                        demLean += state.GetElectoralVotes();
                        continue;
                    case LeanState.LEAN_REP or LeanState.LIKELY_REP or LeanState.CERTAIN_REP:
                        repLean += state.GetElectoralVotes();
                        continue;
                }
            }

            _candidates[0].SetPopularVote(demPopVote);
            _candidates[0].SetConfimedElectoralVotes(demConfirmed);
            _candidates[0].SetLeaningElectoralVotes(demLean);
            _candidates[1].SetPopularVote(repPopVote);
            _candidates[1].SetConfimedElectoralVotes(repConfirmed);
            _candidates[1].SetLeaningElectoralVotes(repLean);
        }


        void OnSkip()
        {
            Parallel.ForEach(_states, state =>
            {
                state.Skip();
            });
        }

        void OnAllCounted()
        {
            _resultsTracker.OnNewResult(_simulationSettings.year,
                _candidates[0].GetName(),
                _candidates[1].GetName(),
                _candidates[0]._confirmedElectoralVotes,
                _candidates[1]._confirmedElectoralVotes);
        }

        void OnReturnToMainMenu()
        {
            StateData.UpdateSelf(_states);
            _simulationSettings.year += 4;
            if((_simulationSettings.year + 4) % 12 == 0) 
            {
                StateData.UpdateElectoralVotes();
            }

            MainMenuSetupControls.OnSimulationFinish();

            SceneManager.LoadScene(0);
        }
    }
}