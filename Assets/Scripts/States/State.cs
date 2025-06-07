using Candidates;
using Controllers.SimulationScene.Simulation;
using Data;
using System;
using UnityEngine;
using UnityEngine.UI;


namespace States
{
    public enum PreviousElectionLeaning
    {
        REP = -2,
        DEM = 2
    }

    public enum LeanState
    {
        CONFIRMED_DEM = -5,
        CERTAIN_DEM = -4,
        LIKELY_DEM = -3,
        LEAN_DEM = -2,
        TILT_DEM = -1,

        NEUTRAL = 0, 

        TILT_REP = 1,
        LEAN_REP = 2,
        LIKELY_REP = 3,
        CERTAIN_REP = 4,
        CONFIRMED_REP = 5
    }

    public enum CountingSpeed
    {
        NORMAL = 8,
        ABSURDLY_SLOW = 2,
        VERY_SLOW = 5,
        SLOW = 6,
        FAST = 10,
        VERY_FAST = 12,
        ABSURDLY_FAST = 20
    }

    public class State : MonoBehaviour
    {
        #region Electoral
        [SerializeField] string _name;
        public string Name
        {
            get { return _name; }
            private set { _name = value; }
        }
        public int _electoralVotes { get; private set; }
        [SerializeField] float _demRegistrationPercent;
        [SerializeField] float _repRegistrationPercent;
        float _unregisteredPercent;
        [SerializeField] PreviousElectionLeaning[] _previousElectionLeanings; // This should be 5 long, will check

        public float _demVotes { get; private set; } = 0;
        public float _repVotes {get; private set; } = 0;

        float _currentDemVotes = 0;
        float _currentRepVotes = 0;
        float _currentDemPercentage = 0.5f;
        float _currentRepPercentage = 0.5f;
        #endregion

        [Space(20)]

        #region Demographics
        [SerializeField] int _population;
        [SerializeField] float _registeredPopulationPercent;
        float _registeredVoters;

        [Space(10)]

        // None of the main data points here need calculating as I have the data:
        //"Estimates of the Total Resident Population and Resident Population Age 18 Years and Older for the United States, 
        //Regions, States, District of Columbia, and Puerto Rico: July 1, 2023 (SCPRC-EST2023-18+POP)"			
        [SerializeField] float _youthPercent;
        float _youthRegisteredVoters;
        [SerializeField] float _lowerMiddlePercent;
        float _lowerMiddleRegisteredVoters;
        [SerializeField] float _upperMiddlePercent;
        float _upperMiddleRegisteredVoters;
        [SerializeField] float _elderlyPercent;
        float _elderlyRegisteredVoters;

        [Space(10)]

        // Only have data for urban, rural is easy to work out from there
        [SerializeField] float _urbanPercent;
        float _urbanRegisteredVoters;
        float _ruralPercent;
        float _ruralRegisteredVoters;

        [Space(10)]

        // No data for Working class, can work out from other data points.
        float _workingClassPercent;
        float _workingClassRegisteredVoters;
        [SerializeField] float _middleClassPercent;
        float _middleClassRegisteredVoters;
        [SerializeField] float _richPercent;
        float _richRegisteredVoters;

        [Space(10)]

        // No data for PoC, just 100 - non-PoC
        [SerializeField] float _nonPoCPercent;
        float _nonPoCRegisteredVoters;
        float _poCPercent;
        float _poCRegisteredVoters;

        #endregion

        [Space(20)]

        #region InSimulationStats
        // https://www.270towin.com/poll-closing-times
        [SerializeField] float _pollClosingTime; // In seconds where 0 is 00:00 on the day of the election and 86,400 is 00:00 the day after
        bool _countingStarted = false;
        bool _countingFinished = false;

        bool _isStateCalled = false;
        int _callDirection = 0;

        [Tooltip("2,500 per electoral vote is used as a default if left at 0")]
        [SerializeField] float _numSecondsEstimate;

        float _rampingUp; // Used within simulation to allow for the speed of votes coming in to gradually "ramp-up" over the first little while to "terminal velocity"
        float _randomizingMultiplierDems, _randomizingMultiplierReps;

        const float RATE_OF_INCREASE_SLOWER = 160000000; // Used to divide the vote incoming speed or else all will be done within a second
        [SerializeField] CountingSpeed _countingSpeed;
        bool _swing; // Used to make counting slower if a close election


        System.Random _random; // Used to get around pesky unityengine not liking parallelization of randomness
        #endregion

        [Space(20)]

        // UI Stuff
        #region UI
        LeanState _leanState = LeanState.NEUTRAL; // Neutral is only used at the start
        [SerializeField] Image _image;
        #endregion

        [Space(30)]

        // This region is temporary, used to force states to align to how they are irl, better formulae should make this unnecessary in the future.
        #region Temporary
        [SerializeField] float _demModifier;
        [SerializeField] float _repModifier;
        #endregion

        void Awake()
        {
            
        }

        void Start()
        {
            
        }

        public void SetupFromStateData(StateDataStruct stateData)
        {
            _electoralVotes = stateData._electoralVotes;

            _demRegistrationPercent = stateData._demReg;
            _repRegistrationPercent = stateData._repReg;

            _population = stateData._pop;
            _registeredPopulationPercent = stateData._regPopPercent;

            _youthPercent = stateData._youth;
            _lowerMiddlePercent = stateData._lowerM;
            _upperMiddlePercent = stateData._upperM;
            _elderlyPercent = stateData._eld;

            _urbanPercent = stateData._urb;
            _middleClassPercent = stateData._mid;
            _richPercent = stateData._rich;

            _nonPoCPercent = stateData._nonPoC;

            _demModifier = stateData._demMod;
            _repModifier = stateData._repMod;

            DoStatisticalSetup();
        }

        void DoStatisticalSetup()
        {
            if (_previousElectionLeanings.Length != 5)
                Debug.LogError($"{_name}'s has been set with an incorrect amount of previous election leanings, please check again");

            CalculateUnalignedPercent();
            CalculateRegisteredVoters();

            CalculateAgeRegisteredVoters();

            CalculateRuralPercent();
            CalculateLocationRegisteredVoters();

            CalculateWorkingClassPercent();
            CalculateClassRegisteredVoters();

            CalculatePoCPercent();
            CalculateDemographicRegisteredVoters();

            // Setup duration simulation should roughly take
            //Debug.Log($"_numSecondsEstimate before check: {_numSecondsEstimate}");
            if (_numSecondsEstimate == 0)
            {
                _numSecondsEstimate = 2500 * _electoralVotes;
                //Debug.Log($"_numSecondsEstimate updated to: {_numSecondsEstimate}");
            }


            // Simulation vote counting speed acceleration variable
            _rampingUp = 0;
            // Semi-random multiplier, used to simulate randomness in votes coming in
            _randomizingMultiplierDems = 1;
            _randomizingMultiplierReps = 1;

            // Setup randomness
            _random = new System.Random();


            _image = GetComponent<Image>();
            _image.GetComponent<Image>().alphaHitTestMinimumThreshold = 1.2f;

            float countingSpeedMultiplier = (float)((int)_countingSpeed) / 8;

            _numSecondsEstimate /= countingSpeedMultiplier;
            _numSecondsEstimate *= (_swing ? 1.5f : 1f);
        }

        #region ElectoralMethods
        void CalculateUnalignedPercent()
        {
            _unregisteredPercent = 1 - _demRegistrationPercent - _repRegistrationPercent;
        }
        #endregion

        #region DemographicsMethods
        void CalculateRegisteredVoters()
        {
            _registeredVoters = _population * _registeredPopulationPercent;
        }

        void CalculateAgeRegisteredVoters()
        {
            _youthRegisteredVoters = _registeredVoters * _youthPercent;
            _lowerMiddleRegisteredVoters = _registeredVoters * _lowerMiddlePercent;
            _upperMiddleRegisteredVoters = _registeredVoters * _upperMiddlePercent;
            _elderlyRegisteredVoters = _registeredVoters * _elderlyPercent;
        }

        void CalculateRuralPercent()
        {
            _ruralPercent = 1 - _urbanPercent;
        }

        void CalculateLocationRegisteredVoters()
        {
            _urbanRegisteredVoters = _registeredVoters * _urbanPercent;
            _ruralRegisteredVoters = _registeredVoters * _ruralPercent;
        }

        void CalculateWorkingClassPercent()
        {
            _workingClassPercent = 1 - _middleClassPercent - _richPercent;
        }

        void CalculateClassRegisteredVoters()
        {
            _workingClassRegisteredVoters = _registeredVoters * _workingClassPercent;
            _middleClassRegisteredVoters = _registeredVoters * _middleClassPercent;
            _richRegisteredVoters = _registeredVoters * _richPercent;
        }

        void CalculatePoCPercent()
        {
            _poCPercent = 1 - _nonPoCPercent;
        }

        void CalculateDemographicRegisteredVoters()
        {
            _nonPoCRegisteredVoters = _registeredVoters * _nonPoCPercent;
            _poCRegisteredVoters = _registeredVoters * _poCPercent;
        }
        #endregion

        #region VoteCalculationMethods
        public void CalculateVotes(Candidate democrat, Candidate republican, float varianceModifier)
        {
            _demVotes = 0;
            _repVotes = 0;

            _currentDemVotes = 0;
            _currentRepVotes = 0;
            _currentDemPercentage = 0.5f;
            _currentRepPercentage = 0.5f;


            if (varianceModifier >= 200)
                Debug.LogError("variance modifier too high, check set value and adjust");

            double[] demCombinationAndTurnoutParts = CalculateCombinationAndTurnoutVoteCountParts(democrat, republican);
            double[] repCombinationAndTurnoutParts = CalculateCombinationAndTurnoutVoteCountParts(republican, democrat);

            double combinationPartsTotal = demCombinationAndTurnoutParts[0] + repCombinationAndTurnoutParts[0];


            double demVotes = demCombinationAndTurnoutParts[0] / combinationPartsTotal * demCombinationAndTurnoutParts[1] * _registeredVoters * democrat.GetAverageIncludingRadicalismPercent() * 1.75f;
            double repVotes = repCombinationAndTurnoutParts[0] / combinationPartsTotal * repCombinationAndTurnoutParts[1] * _registeredVoters * republican.GetAverageIncludingRadicalismPercent() * 1.75f;

            if (_name == "NAME") // Specific State Check
            {
                return;
            }

            demVotes = demVotes * (1 + (_random.Next((int)-(varianceModifier / 2f), (int)(varianceModifier / 2f)) / 100f)); // Add variance
            repVotes = repVotes * (1 + (_random.Next((int)-(varianceModifier / 2f), (int)(varianceModifier / 2f)) / 100f)); // Add variance

            float totalVotes = (float)demVotes + (float)repVotes;
            double random =_random.NextDouble() * (varianceModifier / 2d);
            int dir = _random.Next(0, 2);

            if (dir == 0)
            {
                repVotes -= (float)random;
                demVotes += (float)random;

                if (repVotes <= 0)
                {
                    demVotes += repVotes * -1;
                    repVotes = 1;
                }
            }
            else if(dir == 1)
            {
                demVotes -= (float)random;
                repVotes += (float)random;

                if (demVotes <= 0)
                {
                    repVotes += demVotes * -1;
                    demVotes = 1;
                }
            }

            if (_name == "NAME") // Specific State Check
            {
                return;
            }

            _demVotes = (float)demVotes;
            _repVotes = (float)repVotes;

            if (Mathf.Abs(_demVotes - _repVotes) / (_demVotes + _repVotes) <= 0.05f) _swing = true;
        }

        public void CalculateVotesPolling(Candidate democrat, Candidate republican)
        {
            CalculateVotes(democrat, republican, 0);

            _demVotes = MathF.Pow(_demVotes, 0.96f);
            _repVotes = MathF.Pow(_repVotes, 0.96f);

            if (Mathf.Abs(_demVotes - _repVotes) / (_demVotes + _repVotes) <= 0.04f) _swing = true;

            float multiplier = (10 / (_demVotes + _repVotes)) * _electoralVotes * (_swing ? 5f : 1);
            
            _demVotes *= (0.925f + ((float)_random.NextDouble() / 12.5f));
            _repVotes *= (0.925f + ((float)_random.NextDouble() / 12.5f));

            _demVotes *= multiplier;
            _repVotes *= multiplier;

            _currentDemVotes = _demVotes;
            _currentRepVotes = _repVotes;

            // Handle setting state
            SetLean(ViewMode.MARGINS);

            // Handle Percentages
            float currenttotalVotes = _currentDemVotes + _currentRepVotes;
            _currentDemPercentage = _currentDemVotes / currenttotalVotes;
            _currentRepPercentage = _currentRepVotes / currenttotalVotes;
        }

        float[] CalculateCombinationAndTurnoutVoteCountPartsOLD(Candidate candidate, Candidate other)
        {
            float[] combinationAndturnout = new float[2]; // Return value

            float partisanPart;
            if (candidate.GetParty() == Party.DEMOCRAT)
            {
                partisanPart = _demRegistrationPercent * _registeredVoters / Mathf.Pow(1 - Mathf.Pow(candidate.GetRadicalismPercent() / 2.4f, 4.2f), 0.5f)/8f;
            }
            else
            {
                partisanPart = _repRegistrationPercent * _registeredVoters / Mathf.Pow(1 - Mathf.Pow(candidate.GetRadicalismPercent() / 2.4f, 4.2f), 0.5f)/8f;
            }

            float neutralPart = _unregisteredPercent * _registeredVoters * candidate.GetNeutralPullPercent() / (Mathf.Pow((1 + candidate.GetRadicalismPercent()), 2.4f)) * 1.5f;
            float youthPart = _youthRegisteredVoters * candidate.GetYouthPercent() / 1.5f; // The youth are less likely to vote
            float youngerMiddlePart = _lowerMiddleRegisteredVoters * ((candidate.GetYouthPercent() * 0.9f) + (candidate.GetMiddleAgePercent() * 0.1f));
            float olderMiddlePart = _upperMiddleRegisteredVoters * ((candidate.GetMiddleAgePercent() * 0.5f) + (candidate.GetElderlyPercent() * 0.5f));
            float elderlyPart = _elderlyRegisteredVoters * candidate.GetElderlyPercent() * 1.25f; // The elderly are more likely to vote
            float urbanPart = _urbanRegisteredVoters * candidate.GetUrbanPercent() / 5f;
            float ruralPart = _ruralRegisteredVoters * candidate.GetRuralPercent() / 5f;
            float workingClassPart = _workingClassRegisteredVoters * candidate.GetWorkingClassPercent() / 1.25f;
            float middleClassPart = _middleClassRegisteredVoters * candidate.GetMiddleClassPercent() * 1.25f;
            float richPart = _richRegisteredVoters * candidate.GetRichPercent() * 2.6f;

            float nonPoCPart = _nonPoCRegisteredVoters * candidate.GetNonPoCPercent() * 1.05f;
            float poCPart = _poCRegisteredVoters * candidate.GetPoCPercent() / 1.05f; // division to show trends in voter suppression

            // Old calculation
            /*if (candidate.GetParty() == Party.DEMOCRAT)
            {
                nonPoCPart = _nonPoCRegisteredVoters * _demRegistrationPercent * candidate.GetNonPoCPercent();
                poCPart = _poCRegisteredVoters * _demRegistrationPercent * candidate.GetPoCPercent();
            }
            else
            {
                nonPoCPart = _nonPoCRegisteredVoters * _repRegistrationPercent * candidate.GetNonPoCPercent();
                poCPart = _poCRegisteredVoters * _repRegistrationPercent * candidate.GetPoCPercent();
            }*/

            // Part of old calculation
            /*
            int[] lastFiveAsInts = Array.ConvertAll(_previousElectionLeanings, delegate (PreviousElectionLeaning e) { return (int)e; });

            // conform to excel template's -5, -3, -1, 1, 3, 5.
            float lastFivePart = lastFiveAsInts.Sum() / ((candidate.GetParty() == Party.DEMOCRAT) ? 2 : -2);
            */

            // Long calculation, broken into steps.
            float combinationPart = partisanPart + neutralPart + youthPart + youngerMiddlePart + olderMiddlePart + elderlyPart + urbanPart + ruralPart + workingClassPart + middleClassPart + richPart + nonPoCPart + poCPart;
            combinationPart *= (candidate.GetParty() == Party.DEMOCRAT) ? (1 + _demModifier) : (1 + _repModifier); // TEMP

            combinationPart *= Mathf.Pow(candidate.GetRadicalismPercent(), 0.005f);
            combinationPart *= Mathf.Pow(candidate.GetNameRecognitionPercent(), 0.025f);
            combinationPart *= Mathf.Pow(candidate.GetCoveragePercent(), 0.1f);
            combinationPart *= Mathf.Pow(candidate.GetPassionPercent(), 0.075f);
            combinationPart *= Mathf.Pow(candidate.GetSpeechGivingPercent(), 0.075f);
            combinationPart = Mathf.Pow(combinationPart, 4f);

            // Old calculation
            //combinationPart *= (1 + (lastFivePart / 32));

            // Long calculation, broken into steps.
            float turnout;
            turnout = Mathf.Pow(candidate.GetAveragePercent(), 0.9f);

            turnout *= Mathf.Pow(candidate.GetNameRecognitionPercent(), 0.04f);
            turnout *= Mathf.Pow(candidate.GetCoveragePercent(), 0.02f);
            turnout *= Mathf.Pow(candidate.GetPassionPercent(), 0.06f);
            turnout *= Mathf.Pow(candidate.GetSpeechGivingPercent(), 0.02f);

            float partisanPartTurnout = (candidate.GetParty() == Party.DEMOCRAT) ? _demRegistrationPercent / 16f + _unregisteredPercent / 8f : _repRegistrationPercent / 16f + _unregisteredPercent / 8f;
            partisanPartTurnout = MathF.Pow(partisanPartTurnout, 0.25f);

            turnout *= partisanPartTurnout;
            turnout = Mathf.Pow(turnout, 0.1f);
            turnout *= MathF.Pow(other.GetRadicalismPercent(), 0.08f);
            turnout *= 0.9f; // 0.975f
            turnout /= 1.3f;


            combinationAndturnout[0] = combinationPart;
            combinationAndturnout[1] = turnout;

            if (_name == "NAME" && candidate.GetAge() == 78) // Debugging for Donald Trump
            {
                return combinationAndturnout;
            }
            if (_name == "NAME" && candidate.GetAge() == 60) // Debugging for Kamala Harris
            {
                return combinationAndturnout;
            }

            return combinationAndturnout;
        }

        double[] CalculateCombinationAndTurnoutVoteCountParts(Candidate candidate, Candidate other)
        {
            double[] combinationAndturnout = new double[2]; // Return value

            float partisanPart;
            if (candidate.GetParty() == Party.DEMOCRAT)
            {
                partisanPart = _demRegistrationPercent * _registeredVoters / Mathf.Pow(1 - Mathf.Pow(candidate.GetRadicalismPercent() / 2.4f, 4.2f), 0.5f) / 8f;
            }
            else
            {
                partisanPart = _repRegistrationPercent * _registeredVoters / Mathf.Pow(1 - Mathf.Pow(candidate.GetRadicalismPercent() / 2.4f, 4.2f), 0.5f) / 8f;
            }

            // It was too weak
            double demographicModifier = 1.2d;

            double neutralPart = _unregisteredPercent * _registeredVoters * candidate.GetNeutralPullPercent() / (Mathf.Pow((1 + candidate.GetRadicalismPercent()), 2.4f)) * 1.5f;
            double youthPart = _youthRegisteredVoters * candidate.GetYouthPercent() / 1.5f * demographicModifier; // The youth are less likely to vote
            double youngerMiddlePart = _lowerMiddleRegisteredVoters * ((candidate.GetYouthPercent() * 0.9f) + (candidate.GetMiddleAgePercent() * 0.1f));
            double olderMiddlePart = _upperMiddleRegisteredVoters * ((candidate.GetMiddleAgePercent() * 0.5f) + (candidate.GetElderlyPercent() * 0.5f));
            double elderlyPart = _elderlyRegisteredVoters * candidate.GetElderlyPercent() * 1.25f; // The elderly are more likely to vote
            double urbanPart = _urbanRegisteredVoters * candidate.GetUrbanPercent() / 5f;
            double ruralPart = _ruralRegisteredVoters * candidate.GetRuralPercent() / 5f;
            double workingClassPart = _workingClassRegisteredVoters * candidate.GetWorkingClassPercent() / 1.25f;
            double middleClassPart = _middleClassRegisteredVoters * candidate.GetMiddleClassPercent() * 1.25f;
            double richPart = _richRegisteredVoters * candidate.GetRichPercent() * 2.6f;

            double nonPoCPart = _nonPoCRegisteredVoters * candidate.GetNonPoCPercent() * 1.05f;
            double poCPart = _poCRegisteredVoters * candidate.GetPoCPercent() / 1.05f; // division to show trends in voter suppression

            // Long calculation, broken into steps.
            double combinationPart = partisanPart + Math.Pow(neutralPart, demographicModifier) + Math.Pow(youthPart, demographicModifier) + Math.Pow(youngerMiddlePart, demographicModifier) 
                + Math.Pow(olderMiddlePart, demographicModifier) + Math.Pow(elderlyPart, demographicModifier) + Math.Pow(urbanPart, demographicModifier) + Math.Pow(ruralPart, demographicModifier)
                + Math.Pow(workingClassPart, demographicModifier) + Math.Pow(middleClassPart, demographicModifier) + Math.Pow(richPart, demographicModifier) + Math.Pow(nonPoCPart, demographicModifier) + Math.Pow(poCPart, demographicModifier);
            combinationPart *= (candidate.GetParty() == Party.DEMOCRAT) ? (1 + _demModifier) : (1 + _repModifier);

            combinationPart *= Mathf.Pow(candidate.GetRadicalismPercent(), 0.005f);
            combinationPart *= Mathf.Pow(candidate.GetNameRecognitionPercent(), 0.025f);
            combinationPart *= Mathf.Pow(candidate.GetCoveragePercent(), 0.1f);
            combinationPart *= Mathf.Pow(candidate.GetPassionPercent(), 0.075f);
            combinationPart *= Mathf.Pow(candidate.GetSpeechGivingPercent(), 0.075f);
            combinationPart = Math.Pow(combinationPart, 4f);

            // Long calculation, broken into steps.
            double turnout;
            turnout = Mathf.Pow(candidate.GetAveragePercent(), 0.9f);

            turnout *= Mathf.Pow(candidate.GetNameRecognitionPercent(), 0.04f);
            turnout *= Mathf.Pow(candidate.GetCoveragePercent(), 0.02f);
            turnout *= Mathf.Pow(candidate.GetPassionPercent(), 0.06f);
            turnout *= Mathf.Pow(candidate.GetSpeechGivingPercent(), 0.02f);

            double partisanPartTurnout = (candidate.GetParty() == Party.DEMOCRAT) ? _demRegistrationPercent / 16f + _unregisteredPercent / 8f : _repRegistrationPercent / 16f + _unregisteredPercent / 8f;
            partisanPartTurnout = Math.Pow(partisanPartTurnout, 0.25f);
            partisanPartTurnout = Math.Pow(partisanPartTurnout, 1.28f); // It was too strong

            turnout *= partisanPartTurnout;
            turnout = Math.Pow(turnout, 0.1f);
            turnout *= Math.Pow(other.GetRadicalismPercent(), 0.06f);
            turnout *= 0.9f; // 0.975f
            turnout /= 1.28f;


            combinationAndturnout[0] = combinationPart;
            combinationAndturnout[1] = turnout;

            if (_name == "NAME" && candidate.GetAge() == 78) // Debugging for Donald Trump
            {
                return combinationAndturnout;
            }
            if (_name == "NAME" && candidate.GetAge() == 60) // Debugging for Kamala Harris
            {
                return combinationAndturnout;
            }

            return combinationAndturnout;
        }

        #endregion

        #region InSimulationMethods
        public void FrameUpdate(float deltaTime, float simulationErraticness, float simulationSpeed, ViewMode viewMode)
        {
            if ((_countingFinished || !_countingStarted)) return;

            // Handle counting acceleration
            for (int i = 0; i <= simulationSpeed; i++)
            {
                if (_rampingUp < 10000000)
                    _rampingUp += _random.Next(200, 500);
                else
                    _rampingUp *= 1 - (_random.Next(8500, 11000) / 1000000000);
            }

            // Handle randomness multiplier
            if (simulationErraticness != 0)
            {
                RandomizingMultiplierUpdate(ref _randomizingMultiplierDems, deltaTime, simulationErraticness, simulationSpeed);
                RandomizingMultiplierUpdate(ref _randomizingMultiplierReps, deltaTime, simulationErraticness, simulationSpeed);
                
                _randomizingMultiplierDems *= 0.9999999999f;
                _randomizingMultiplierReps *= 1.0000000001f;
            }

            // Handle Currents
            // These both return a bool if the vote is in, in full for the given party
            // Therefore you can set counting finished to true
            bool demFinish = UpdateCurrentVoteCount(ref _currentDemVotes, _demVotes, _randomizingMultiplierDems, deltaTime, simulationErraticness, simulationSpeed);
            bool repFinish = UpdateCurrentVoteCount(ref _currentRepVotes, _repVotes, _randomizingMultiplierReps, deltaTime, simulationErraticness, simulationSpeed);

            if (demFinish && repFinish)
            {
                _countingFinished = true;
            }

            // Handle setting state
            SetLean(viewMode);

            // Handle Percentages
            float currenttotalVotes = _currentDemVotes + _currentRepVotes;
            _currentDemPercentage = _currentDemVotes / currenttotalVotes;
            _currentRepPercentage = _currentRepVotes / currenttotalVotes;
        }
        void RandomizingMultiplierUpdate(ref float randomizingMultiplier, float deltaTime, float simulationErraticness, float simulationSpeed)
        {
            float oldValue = randomizingMultiplier;
            int numberOfRecursions = Mathf.FloorToInt(simulationSpeed);

            for (int i = 0; i < numberOfRecursions; i++)
            {
                randomizingMultiplier += ((float)_random.Next(-10000, 10001)) / 1000000 * Mathf.Pow(simulationErraticness, 1.5f) * deltaTime;

                if (randomizingMultiplier > (1 + simulationErraticness / 5f))
                {
                    randomizingMultiplier -= 0.0002f;
                    randomizingMultiplier /= 1.000096f;
                }

                else if (randomizingMultiplier < (0.9f - (simulationErraticness / 115f)))
                {
                    randomizingMultiplier += 0.0256f;
                    if (randomizingMultiplier > 0)
                    {
                        randomizingMultiplier *= 1.02048f;
                    }
                }

                else if (randomizingMultiplier <= 0)
                {
                    randomizingMultiplier = 0.66f - simulationErraticness / 250f;
                }
            }

            float newValue = randomizingMultiplier - oldValue;

            float extra = newValue * simulationSpeed / numberOfRecursions;
            randomizingMultiplier += extra;

            if (randomizingMultiplier < 0) randomizingMultiplier = 0.66f - simulationErraticness / 250f;
        }

        bool UpdateCurrentVoteCount(ref float currentVoteCount, float endVoteCount, float randomizingMultiplier, float deltaTime,
            float simulationErraticness, float simulationSpeed)
        {
            if (_currentDemVotes < 1 || _currentRepVotes < 1)
            {
                _currentDemVotes = 1;
                _currentRepVotes = 1;
            }

            float votePercentage = currentVoteCount / endVoteCount;
            if (votePercentage >= 1)
                return true;

            float pow = (votePercentage <= 0.995f) ? Mathf.Sqrt(votePercentage) : 0.99f;

            if (simulationErraticness > 0)
            {
                currentVoteCount += (endVoteCount - Mathf.Pow(currentVoteCount, pow)) * _random.Next((int)(275 - simulationErraticness * 2.5f), (int)(375
                    + simulationErraticness * 2.5f)) * _rampingUp * randomizingMultiplier / Mathf.Pow(_numSecondsEstimate, 1.32f) * simulationSpeed * deltaTime
                    / RATE_OF_INCREASE_SLOWER;
            }
            else
            {
                currentVoteCount += (endVoteCount - Mathf.Pow(currentVoteCount, pow)) * 335f * _rampingUp * randomizingMultiplier / Mathf.Pow(_numSecondsEstimate, 1.4f - Mathf.Abs((_repVotes - _demVotes) / (_repVotes + _demVotes)))
                    * simulationSpeed * deltaTime / RATE_OF_INCREASE_SLOWER;
            }

            if (currentVoteCount > endVoteCount)
            {
                currentVoteCount = endVoteCount;
                return true;
            }

            return false;

        }

        public void Skip()
        {
            _currentDemVotes = _demVotes;
            _currentRepVotes = _repVotes;

            // Handle Percentages
            float currenttotalVotes = _currentDemVotes + _currentRepVotes;
            _currentDemPercentage = _currentDemVotes / currenttotalVotes;
            _currentRepPercentage = _currentRepVotes / currenttotalVotes;

            SetLean(ViewMode.LIKELIHOOD);
            _countingFinished = true;
        }

        public void SetStart(float time)
        {
            if (time >= _pollClosingTime && !_countingFinished)
                _countingStarted = true;
        }

        public void SetLean(ViewMode viewMode)
        {
            float demLean = _currentDemPercentage - _currentRepPercentage;
            float votesLeft = _demVotes + _repVotes - _currentDemVotes - _currentRepVotes;
            float percentLeft = 1 - (_currentDemVotes + _currentRepVotes) / (_demVotes + _repVotes);

            if (viewMode == ViewMode.LIKELIHOOD)
            {
                _leanState = GetLeanLikelihood(demLean, votesLeft, percentLeft);
            }
            else
            {
                SetLeanMargins(demLean);
            }

            PerformCalledCheck(demLean, votesLeft, percentLeft);
        }

        public LeanState GetLeanLikelihood(float demLean, float votesLeft, float percentLeft)
        {
            if (_currentDemVotes == 0 || _currentRepVotes == 0)
            {
                return LeanState.NEUTRAL;
            }


            if (_demVotes - _repVotes > votesLeft)
            {
                return LeanState.CONFIRMED_DEM;
            }
            else if (_repVotes - _demVotes > votesLeft)
            {
                return LeanState.CONFIRMED_REP;
            }

            if (demLean > 0.50f) // Extreme Margins
            {
                if (percentLeft < 0.925f) return LeanState.CONFIRMED_DEM;
                else if (percentLeft < 0.975f) return LeanState.CERTAIN_DEM;
                else if (percentLeft < 0.995f) return LeanState.LIKELY_DEM;
                else return LeanState.LEAN_DEM;
            }
            else if (demLean < -0.50f) // Extreme Margins
            {
                if (percentLeft < 0.925f) return LeanState.CONFIRMED_REP;
                else if (percentLeft < 0.975f) return LeanState.CERTAIN_REP;
                else if (percentLeft < 0.995f) return LeanState.LIKELY_REP;
                else return LeanState.LEAN_REP;
            }

            if (demLean > 0.30f) // Finished Margins
            {
                if (percentLeft < 0.8f) return LeanState.CONFIRMED_DEM;
                else if (percentLeft < 0.95f) return LeanState.CERTAIN_DEM;
                else if (percentLeft < 0.9925f) return LeanState.LIKELY_DEM;
                else return LeanState.LEAN_DEM;
            }
            else if (demLean < -0.30f) // Finished Margins
            {
                if (percentLeft < 0.8f) return LeanState.CONFIRMED_REP;
                else if (percentLeft < 0.95f) return LeanState.CERTAIN_REP;
                else if (percentLeft < 0.9925f) return LeanState.LIKELY_REP;
                else return LeanState.LEAN_REP;
            }

            if (demLean > 0.25f) // Overkill Margins
            {
                if (percentLeft < 0.75f) return LeanState.CONFIRMED_DEM;
                else if (percentLeft < 0.9f) return LeanState.CERTAIN_DEM;
                else if (percentLeft < 0.988f) return LeanState.LIKELY_DEM;
                else return LeanState.LEAN_DEM;
            }
            else if (demLean < -0.25f) // Overkill Margins
            {
                if (percentLeft < 0.75f) return LeanState.CONFIRMED_REP;
                else if (percentLeft < 0.9f) return LeanState.CERTAIN_REP;
                else if (percentLeft < 0.988f) return LeanState.LIKELY_REP;
                else return LeanState.LEAN_REP;
            }

            if (demLean > 0.20f) // Over Margins
            {
                if (percentLeft < 0.7f) return LeanState.CONFIRMED_DEM;
                else if (percentLeft < 0.85f) return LeanState.CERTAIN_DEM;
                else if (percentLeft < 0.985f) return LeanState.LIKELY_DEM;
                else return LeanState.LEAN_DEM;
            }
            else if (demLean < -0.20f) // Over Margins
            {
                if (percentLeft < 0.7f) return LeanState.CONFIRMED_REP;
                else if (percentLeft < 0.85f) return LeanState.CERTAIN_REP;
                else if (percentLeft < 0.985f) return LeanState.LIKELY_REP;
                else return LeanState.LEAN_REP;
            }

            if (demLean > 0.15f) // Safe Margins
            {
                if (percentLeft < 0.6f) return LeanState.CONFIRMED_DEM;
                else if (percentLeft < 0.8f) return LeanState.CERTAIN_DEM;
                else if (percentLeft < 0.98f) return LeanState.LIKELY_DEM;
                else return LeanState.LEAN_DEM;
            }
            else if (demLean < -0.15f) // Safe Margins
            {
                if (percentLeft < 0.6f) return LeanState.CONFIRMED_REP;
                else if (percentLeft < 0.8f) return LeanState.CERTAIN_REP;
                else if (percentLeft < 0.98f) return LeanState.LIKELY_REP;
                else return LeanState.LEAN_REP;
            }

            if (demLean > 0.05f) // Likely Margins
            {
                if (percentLeft < 0.08f) return LeanState.CERTAIN_DEM;
                else if (percentLeft < 0.9f) return LeanState.LIKELY_DEM;
                else if (percentLeft < 0.995f) return LeanState.LEAN_DEM;
                else return LeanState.TILT_DEM;
            }
            else if (demLean < -0.05f) // Likely Margins
            {
                if (percentLeft < 0.08f) return LeanState.CERTAIN_REP;
                else if (percentLeft < 0.9f) return LeanState.LIKELY_REP;
                else if (percentLeft < 0.995f) return LeanState.LEAN_REP;
                else return LeanState.TILT_REP;
            }

            if (demLean > 0.01f) // Lean margins
            {
                if (percentLeft < 0.002f) return LeanState.CERTAIN_DEM;
                else if (percentLeft < 0.08f) return LeanState.LIKELY_DEM;
                else if (percentLeft < 0.8f) return LeanState.LEAN_DEM;
                else return LeanState.TILT_DEM;
            }
            else if (demLean < -0.01f) // Lean margins
            {
                if (percentLeft < 0.002f) return LeanState.CERTAIN_REP;
                else if (percentLeft < 0.08f) return LeanState.LIKELY_REP;
                else if (percentLeft < 0.8f) return LeanState.LEAN_REP;
                else return LeanState.TILT_REP;
            }
            else if (_currentRepVotes != _currentDemVotes) // Tilt margins
            {
                if (_currentDemVotes > _currentRepVotes) return LeanState.TILT_DEM;
                else return LeanState.TILT_REP;
            }


            return LeanState.NEUTRAL;
        }

        void SetLeanMargins(float demLean)
        {
            if (_currentDemVotes == 0 || _currentRepVotes == 0)
            {
                _leanState = LeanState.NEUTRAL;
                return;
            }


            switch (demLean)
            {
                case >= 0.15f:
                    _leanState = LeanState.CONFIRMED_DEM;
                    return;
                case >= 0.05f:
                    _leanState = LeanState.LIKELY_DEM;
                    return;
                case >= 0.01f:
                    _leanState = LeanState.LEAN_DEM;
                    return;
                case >= 0f:
                    _leanState = LeanState.TILT_DEM;
                    return;
                case >= -0.01f:
                    _leanState = LeanState.TILT_REP;
                    return;
                case >= -0.05f:
                    _leanState = LeanState.LEAN_REP;
                    return;
                case >= -0.15f:
                    _leanState = LeanState.LIKELY_REP;
                    return;
                case < -0.15f:
                    _leanState = LeanState.CONFIRMED_REP;
                    return;
            }


            _leanState = LeanState.NEUTRAL;
            return;
        }






        bool PerformCalledCheck(float demLean, float votesLeft, float percentLeft)
        {
            if (_demVotes - _repVotes > votesLeft)
            {
                return true;
            }
            else if (_repVotes - _demVotes > votesLeft)
            {
                return true;
            }

            if (demLean > 0.50f) // Extreme Margins
            {
                if (percentLeft < 0.925f) return true; ;
            }
            else if (demLean < -0.50f) // Extreme Margins
            {
                if (percentLeft < 0.925f) return true; ;
            }

            if (demLean > 0.30f) // Finished Margins
            {
                if (percentLeft < 0.8f) return true; ;
            }
            else if (demLean < -0.30f) // Finished Margins
            {
                if (percentLeft < 0.8f) return true; ;
            }

            if (demLean > 0.25f) // Overkill Margins
            {
                if (percentLeft < 0.75f) return true; ;
            }
            else if (demLean < -0.25f) // Overkill Margins
            {
                if (percentLeft < 0.75f) return true; ;
            }

            if (demLean > 0.20f) // Over Margins
            {
                if (percentLeft < 0.7f) return true;
            }
            else if (demLean < -0.20f) // Over Margins
            {
                if (percentLeft < 0.7f) return true; ;
            }

            if (demLean > 0.15f) // Safe Margins
            {
                if (percentLeft < 0.6f) return true; ;
            }
            else if (demLean < -0.15f) // Safe Margins
            {
                if (percentLeft < 0.6f) return true; ;
            }

            return false;
        }


        public int CheckStateCalled() // returns 1 on the frame the dems win, returns -1 on the frame reps win
        {
            float demLean = _currentDemPercentage - _currentRepPercentage;
            float votesLeft = _demVotes + _repVotes - _currentDemVotes - _currentRepVotes;
            float percentLeft = 1 - (_currentDemVotes + _currentRepVotes) / (_demVotes + _repVotes);

            if (!PerformCalledCheck(demLean, votesLeft, percentLeft) && !_isStateCalled)
            {
                return 0;
            }

            LeanState callLean = GetLeanLikelihood(demLean, votesLeft, percentLeft);
            _callDirection = (int)callLean;

            return _callDirection;
        }

        public void SetCalled(bool called)
        {
            _isStateCalled = called;

            if (_isStateCalled == false)
            {
                _callDirection = 0;
            }
        }
        #endregion

        #region UI
        public void SetColour()
        {
            switch (_leanState)
            {
                case LeanState.CONFIRMED_DEM:
                    _image.color = StateSwingColour._confirmedDemColour;
                    break;
                case LeanState.CERTAIN_DEM:
                    _image.color = StateSwingColour._certainDemColour;
                    break;
                case LeanState.LIKELY_DEM:
                    _image.color = StateSwingColour._likelyDemColour;
                    break;
                case LeanState.LEAN_DEM:
                    _image.color = StateSwingColour._leanDemColour;
                    break;
                case LeanState.TILT_DEM:
                    _image.color = StateSwingColour._tiltDemColour;
                    break;

                case LeanState.TILT_REP:
                    _image.color = StateSwingColour._tiltRepColour;
                    break;
                case LeanState.LEAN_REP:
                    _image.color = StateSwingColour._leanRepColour;
                    break;
                case LeanState.LIKELY_REP:
                    _image.color = StateSwingColour._likelyRepColour;
                    break;
                case LeanState.CERTAIN_REP:
                    _image.color = StateSwingColour._certainRepColour;
                    break;
                case LeanState.CONFIRMED_REP:
                    _image.color = StateSwingColour._confirmedRepColour;
                    break;

                default:
                    _image.color = StateSwingColour._neutralColour;
                    break;
            }
        }
        #endregion


        #region Accessors
        public string GetName()
        {
            return _name;
        }

        public int GetElectoralVotes()
        {
            return _electoralVotes;
        }

        public bool GetCountingStarted()
        {
            return _countingStarted;
        }

        public bool GetCountringFinished()
        {
            return _countingFinished;
        }

        public bool GetIsCalled()
        {
            return _isStateCalled;
        }

        public int GetCallDirection()
        {
            return _callDirection;
        }

        public float GetDemLean()
        {
            return _currentDemPercentage - _currentRepPercentage;
        }

        public Image GetImage()
        {
            return _image;
        }

        public float[] GetCurrentVotes()
        {
            float[] votes = new float[2];

            votes[0] = _currentDemVotes;
            votes[1] = _currentRepVotes;

            return votes;
        }

        public float[] GetCurrentVotingData()
        {
            float demLean = _currentDemPercentage - _currentRepPercentage;
            float votesLeft = _demVotes + _repVotes - _currentDemVotes - _currentRepVotes;
            float percentLeft = 1 - (_currentDemVotes + _currentRepVotes) / (_demVotes + _repVotes);

            float[] votingData = new float[3] { demLean, votesLeft, percentLeft };

            return votingData;  
        }

        public float GetTotalVotes()
        {
            return _demVotes + _repVotes;
        }

        public LeanState GetLeanState()
        {
            return _leanState;
        }

    public float GetCountStartTime()
        {
            return _pollClosingTime;
        }
        #endregion
    }

    public static class StateSwingColour
    {
        public static Color _tiltDemColour = new Color(148f / 255f, 155f / 255f, 179f / 255f, 1f);
        public static Color _leanDemColour = new Color(138f / 255f, 175f / 255f, 1f, 1f);
        public static Color _likelyDemColour = new Color(87f / 255f, 124f / 255f, 204f / 255f, 1f);
        public static Color _certainDemColour = new Color(28f / 255f, 64f / 255f, 140f / 255f, 1f);
        public static Color _confirmedDemColour = new Color(0f / 255f, 0f / 255f, 1f, 1f);


        public static Color _tiltRepColour = new Color(207f / 255f, 137f / 255f, 128f / 255f, 1f);
        public static Color _leanRepColour = new Color(1f, 139f / 255f, 152f / 255f, 1f);
        public static Color _likelyRepColour = new Color(1f, 88f / 255f, 101f / 255f, 1f);
        public static Color _certainRepColour = new Color(191f / 255f, 29f / 255f, 41f / 255f, 1f);
        public static Color _confirmedRepColour = new Color(1f, 0f / 255f, 0f / 255f, 1f);

        public static Color _neutralColour = new Color(0.7f, 0.7f, 0.7f, 1f);
    }
}