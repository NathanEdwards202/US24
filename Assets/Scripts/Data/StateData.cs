using States;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;


namespace Data
{
    public static class StateData
    {
        static Random r = new();
        static bool _setup = false;
        public static ConcurrentDictionary<string, StateDataStruct> _oldData = new();
        public static ConcurrentDictionary<string, StateDataStruct> _currentData = new();
        public static ConcurrentDictionary<string, StateDataStruct> _changesInData = new(); 

        static int totalVotes = 538;
        static int minVotes = 3;

        static float registrationPull = 0.75f;
        static float youthPull = 0.2f;
        static float lowerMiddlePull = 0.25f;
        static float upperMiddlePull = 0.3f;
        static float urbanPull = 0.8f;
        static float middlePull = 0.34f;
        static float richPull = 0.06f;
        static float nonPocPull = 0.5f;


        public static void InitialSetup()
        {
            if (_setup) return;
            _currentData = new(BaseData._initialData);
            _setup = true;
        }

        public static void UpdateSelf(List<State> states, bool updateEVs)
        {
            _oldData = new(_currentData);
            ConcurrentDictionary<string, StateDataStruct> newData = new();

            if (updateEVs)
            {
                UpdateElectoralVotes();
            }

            // Stuff based on states
            Parallel.ForEach(states, s =>
            {
                StateDataStruct newStruct = new StateDataStruct();

                string stateName = s.Name;
                int newEVs = _currentData[stateName]._electoralVotes;
                float demLean = (float)s.GetDemLean();
                float newDemMod;
                float newRepMod;
                float newDemReg = _currentData[s.Name]._demReg;
                float newRepReg = _currentData[s.Name]._repReg;

                int newPop = _currentData[s.Name]._pop;

                float newRegistation = _currentData[s.Name]._regPopPercent;
                float newYouth = _currentData[s.Name]._youth;
                float newLower = _currentData[s.Name]._lowerM;
                float newUpper = _currentData[s.Name]._upperM;
                float newUrban = _currentData[s.Name]._urb;
                float newMiddle = _currentData[s.Name]._mid;
                float newRich = _currentData[s.Name]._rich;
                float newNonPoC = _currentData[s.Name]._nonPoC;

                newDemReg = (0.32f + (float)s.GetDemLean() + newDemReg * 11) / 13;
                newRepReg = (0.32f + (float)s.GetDemLean() * -1 + newRepReg * 11) / 13;

                for (int i = 0; i < 4; i++)
                {
                    newDemReg += GenerateSkewedRandomFloat(-0.02f, 0.02f, 1.32f);
                    newRepReg += GenerateSkewedRandomFloat(-0.02f, 0.02f, 1.32f);
                }
                

                while (newRepReg + newDemReg >= 0.95f)
                {
                    newRepReg /= 1.01f;
                    newDemReg /= 1.01f;
                }


                for (int i = 0; i < 4; i++)
                {
                    newPop = (int)MathF.Round(newPop * GenerateRandomFloat(0.9925f - (newEVs - 3) / 8000f, 1.0325f - (newEVs - 3) / 8000f));
                }

                float addition = -60;
                float additionTwo = 16;

                newRegistation *= 68;
                newRegistation += registrationPull;
                newRegistation += GenerateRandomFloat(0, 1) * 2;
                newRegistation /= 71;

                newYouth *= 88 + addition;
                newYouth += youthPull;
                newYouth += GenerateRandomFloat(0, 1) * 4;
                newYouth /= 93 + addition;

                newLower *= 88 + addition;
                newLower += lowerMiddlePull;
                newLower += GenerateRandomFloat(0, 1) * 3;
                newLower /= 92 + addition;

                newUpper *= 88 + addition;
                newUpper += upperMiddlePull;
                newUpper += GenerateRandomFloat(0, 1) * 2;
                newUpper /= 91 + addition;

                while (newYouth + newLower + newUpper > 0.95f)
                {
                    newYouth /= 1.01f;
                    newLower /= 1.01f;
                    newUpper /= 1.01f;
                }

                newUrban *= 88 + addition;
                newUrban += urbanPull;
                newUrban += GenerateRandomFloat(0, 1) * 3;
                newUrban /= 92 + addition;

                newMiddle *= 88 + addition;
                newMiddle += middlePull;
                newMiddle += GenerateRandomFloat(0, 1) * 9;
                newMiddle /= 98 + addition;

                newRich *= 88 + addition;
                newRich += richPull;
                newRich += GenerateRandomFloat(0, 1) * 5;
                newRich /= 94 + addition;

                while (newMiddle + newRich > 0.95f)
                {
                    newMiddle /= 1.01f;
                    newRich /= 1.01f;
                }

                newNonPoC *= 88 + addition + additionTwo;
                newNonPoC += nonPocPull;
                newNonPoC += GenerateRandomFloat(0, 1) * 5;
                newNonPoC /= 94 + addition + additionTwo;

                while (newNonPoC > 0.95f)
                {
                    newNonPoC /= 1.01f;
                }

                newDemMod = _currentData[s.Name]._demMod;
                newRepMod = _currentData[s.Name]._repMod;

                float change = GenerateSkewedRandomFloat(-0.032f, 0.048f, 2f);
                if (newDemMod > 0)
                {
                    newDemMod += change;
                    if (newDemMod < 0)
                    {
                        newRepMod = -newDemMod;
                        newDemMod = 0;
                    }
                }
                else
                {
                    newRepMod += change;
                    if (newRepMod < 0)
                    {
                        newDemMod = -newRepMod;
                        newRepMod = 0;
                    }
                }

                newDemMod /= 1.1f;
                newRepMod /= 1.1f;

                newStruct._electoralVotes = newEVs;
                newStruct._pop = newPop;
                newStruct._regPopPercent = Math.Clamp(newRegistation, 0.02f, 0.98f);
                newStruct._demReg = Math.Clamp(newDemReg, 0.02f, 0.78f);
                newStruct._repReg = Math.Clamp(newRepReg, 0.02f, 0.78f);
                newStruct._youth = Math.Clamp(newYouth, 0.02f, 0.98f);
                newStruct._lowerM = Math.Clamp(newLower, 0.02f, 0.98f);
                newStruct._upperM = Math.Clamp(newUpper, 0.02f, 0.98f);
                newStruct._eld = Math.Clamp(1 - newYouth - newLower - newUpper, 0.02f, 0.98f);
                newStruct._urb = Math.Clamp(newUrban, 0.02f, 0.98f);
                newStruct._mid = Math.Clamp(newMiddle, 0.02f, 0.98f);
                newStruct._rich = Math.Clamp(newRich, 0.02f, 0.98f);
                newStruct._nonPoC = Math.Clamp(newNonPoC, 0.02f, 0.98f);
                newStruct._demMod = Math.Clamp(newDemMod, 0f, 0.98f);
                newStruct._repMod = Math.Clamp(newRepMod, 0f, 0.98f);

                newData[s.Name] = newStruct;
            });

            _currentData = newData;

            UpdateChangesInData();
        }

        static void UpdateChangesInData()
        {
            foreach (string s in _currentData.Keys)
            {
                StateDataStruct oldData = _oldData[s];
                StateDataStruct newData = _currentData[s];

                StateDataStruct dataChange = new(
                        newData._electoralVotes - oldData._electoralVotes,
                        newData._demReg - oldData._demReg,
                        newData._repReg - oldData._repReg,
                        newData._pop - oldData._pop,
                        newData._regPopPercent - oldData._regPopPercent,
                        newData._youth - oldData._youth,
                        newData._lowerM - oldData._lowerM,
                        newData._upperM - oldData._upperM,
                        newData._eld - oldData._eld,
                        newData._urb - oldData._urb,
                        newData._mid - oldData._mid,
                        newData._rich - oldData._rich,
                        newData._nonPoC - oldData._nonPoC,
                        newData._demMod - oldData._demMod,
                        newData._repMod - oldData._repMod
                    );

                _changesInData[s] = dataChange;
            }
        }

        public static void UpdateElectoralVotes()
        {
            // Calculate the total population of all states
            int totalPopulation = _currentData.Values.Sum(state => state._pop);

            // Step 1: Recalculate the electoral votes proportionally
            double totalVotesAssigned = 0;
            foreach (var state in _currentData)
            {
                var stateData = state.Value;
                // Calculate the proportion of electoral votes for the state
                double electoralVotesProportion = (double)stateData._pop / (double)totalPopulation;
                // Assign electoral votes based on the proportion, with a minimum of 3 votes
                int newElectoralVotes = (int)Math.Round(electoralVotesProportion * totalVotes);
                newElectoralVotes += 3;
                stateData._electoralVotes = Math.Max(newElectoralVotes, minVotes);
                totalVotesAssigned += stateData._electoralVotes;

                _currentData[state.Key] = stateData;
            }

            // Step 2: Adjust the total electoral votes to make sure it sums to 538
            // Calculate the difference in the total allocated votes
            int difference = totalVotes - (int)totalVotesAssigned;
            UnityEngine.Debug.Log(difference);

            // Distribute the difference to the states in some way (e.g., proportionally or evenly)
            // We will distribute the difference to states with more population (higher weight).
            if (difference != 0)
            {
                // Create a sorted list of the data based on population
                var orderedList = _currentData
                    .OrderByDescending(data => data.Value._pop)
                    .ToList();

                while (difference != 0)
                {
                    orderedList = _currentData
                    .OrderByDescending(data => data.Value._pop)
                    .ToList();

                    // Distribute the difference across the states
                    foreach (var item in orderedList)
                    {
                        var stateData = item.Value;

                        if (difference == 0) break;

                        if (difference > 0 && stateData._electoralVotes > 3)
                        {
                            stateData._electoralVotes++;
                            difference--;
                        }
                        else if (difference < 0 && stateData._electoralVotes > minVotes)
                        {
                            stateData._electoralVotes--;
                            difference++;
                        }

                        // Reassign updated data
                        _currentData[item.Key] = stateData;
                    }
                }                
            }
        }

        public static float GenerateRandomFloat(float minimum, float maximum)
        {
            return (float)(r.NextDouble() * (maximum - minimum) + minimum);
        }

        // Skew factor of 0 means all results will be the minimum / maximum
        // Skew factor of 1 means regular distribution
        // Skew factor of 2 means normal distribution
        // Skew factor of 4 means 87% chance of being within 20% of the midpoint.
        public static float GenerateSkewedRandomFloat(float minimum, float maximum, float skewFactor)
        {
            float midpoint = (minimum + maximum) / 2;
            float range = (maximum - minimum) / 2;

            double v = (r.NextDouble() * 2 - 1);
            double skewed = Math.Sign(v) * Math.Pow(Math.Abs(v), skewFactor);

            return (float)(midpoint + skewed * range);
        }
    }

    static class BaseData
    {
        public readonly static Dictionary<string, StateDataStruct> _initialData = new()
        {
            { "Alabama", new(
                9,
                0.35f,
                0.52f,
                3977628,
                0.68f,
                0.088f,
                0.117f,
                0.366f,
                0.181f,
                0.577f,
                0.3552f,
                0.0487f,
                0.675f,
                0f,
                0.15f
                ) },
            { "Alaska", new(
                3,
                0.13f,
                0.24f,
                557899,
                0.742f,
                0.088f,
                0.134f,
                0.387f,
                0.139f,
                0.649f,
                0.4557f,
                0.0818f,
                0.6336f,
                0f,
                0.0975f
                ) },
            { "Arizona", new(
                11,
                0.31f,
                0.34f,
                5848310,
                0.764f,
                0.095f,
                0.123f,
                0.366f,
                0.19f,
                0.893f,
                0.3919f,
                0.0603f,
                0.7377f,
                0f,
                0.0315f
                ) },
            { "Arkansas", new(
                6,
                0.05f,
                0.07f,
                2362124,
                0.62f,
                0.092f,
                0.111f,
                0.36f,
                0.177f,
                0.555f,
                0.346f,
                0.0433f,
                0.7537f,
                0f,
                0.185f
                ) },
            { "California", new(
                54,
                0.47f,
                0.24f,
                30159524,
                0.694f,
                0.089f,
                0.134f,
                0.4f,
                0.159f,
                0.942f,
                0.3854f,
                0.0851f,
                0.5605f,
                0.0684f,
                0f
                ) },
            { "Colorado", new(
                10,
                0.27f,
                0.25f,
                4662926,
                0.713f,
                0.09f,
                0.142f,
                0.412f,
                0.158f,
                0.86f,
                0.4178f,
                0.0748f,
                0.8152f,
                0.073f,
                0f
                ) },
            { "Connecticut", new(
                7,
                0.37f,
                0.2f,
                2894190,
                0.733f,
                0.083f,
                0.115f,
                0.373f,
                0.184f,
                0.863f,
                0.3882f,
                0.0944f,
                0.7422f,
                0.08875f,
                0f
                ) },
            { "Delaware", new(
                3,
                0.48f,
                0.28f,
                819952,
                0.751f,
                0.074f,
                0.118f,
                0.355f,
                0.211f,
                0.826f,
                0.4082f,
                0.0698f,
                0.6744f,
                0.07f,
                0f
                ) },
            { "Washington DC", new(
                3,
                0.77f,
                0.05f,
                552380,
                0.869f,
                0.089f,
                0.198f,
                0.482f,
                0.134f,
                1f,
                0.3384f,
                0.0912f,
                0.4107f,
                0.7525f,
                0f
                ) },
            { "Florida", new(
                30,
                0.34f,
                0.36f,
                18229883,
                0.671f,
                0.08f,
                0.113f,
                0.364f,
                0.216f,
                0.915f,
                0.3752f,
                0.0587f,
                0.7164f,
                0f,
                0.0743f
                ) },
            { "Georgia", new(
                16,
                0.41f,
                0.41f,
                8490546,
                0.707f,
                0.092f,
                0.121f,
                0.386f,
                0.151f,
                0.741f,
                0.3738f,
                0.0607f,
                0.5725f,
                0f,
                0.033f
                ) },
            { "Hawaii", new(
                4,
                0.51f,
                0.28f,
                1141525,
                0.687f,
                0.072f,
                0.111f,
                0.369f,
                0.212f,
                0.861f,
                0.4341f,
                0.092f,
                0.2415f,
                0.0025f,
                0f
                ) },
            { "Idaho", new(
                4,
                0.13f,
                0.58f,
                1497384,
                0.693f,
                0.09f,
                0.116f,
                0.365f,
                0.171f,
                0.692f,
                0.4161f,
                0.0514f,
                0.8841f,
                0f,
                0.154f
                ) },
            { "Illinois", new(
                19,
                0.48f,
                0.33f,
                9844167,
                0.744f,
                0.088f,
                0.123f,
                0.383f,
                0.171f,
                0.869f,
                0.398f,
                0.0713f,
                0.6979f,
                0.058f,
                0f
                ) },
            { "Indiana", new(
                11,
                0.37f,
                0.42f,
                5274945,
                0.92f,
                0.092f,
                0.119f,
                0.368f,
                0.168f,
                0.712f,
                0.4014f,
                0.0534f,
                0.8228f,
                0f,
                0.0625f
                ) },
            { "Iowa", new(
                6,
                0.32f,
                0.34f,
                2476882,
                0.76f,
                0.089f,
                0.116f,
                0.36f,
                0.182f,
                0.632f,
                0.4093f,
                0.0571f,
                0.8909f,
                0f,
                0.0158f
                ) },
            { "Kansas", new(
                6,
                0.26f,
                0.44f,
                2246209,
                0.708f,
                0.094f,
                0.114f,
                0.358f,
                0.178f,
                0.723f,
                0.403f,
                0.0581f,
                0.8296f,
                0f,
                0.04789f
                ) },
            { "Kentucky", new(
                8,
                0.45f,
                0.45f,
                3509259,
                0.759f,
                0.088f,
                0.117f,
                0.368f,
                0.175f,
                0.587f,
                0.3665f,
                0.0469f,
                0.8625f,
                0f,
                0.10f
                ) },
            { "Louisiana", new(
                8,
                0.4f,
                0.33f,
                3506600,
                0.693f,
                0.087f,
                0.117f,
                0.367f,
                0.17f,
                0.715f,
                0.3337f,
                0.0481f,
                0.6125f,
                0f,
                0.1375f
                ) },
            { "Maine", new(
                4,
                0.36f,
                0.28f,
                1146670,
                0.774f,
                0.069f,
                0.113f,
                0.364f,
                0.225f,
                0.386f,
                0.3763f,
                0.056f,
                0.9368f,
                0.126f,
                0f
                ) },
            { "Maryland", new(
                10,
                0.54f,
                0.24f,
                4818337,
                0.786f,
                0.08f,
                0.118f,
                0.382f,
                0.169f,
                0.856f,
                0.4072f,
                0.0972f,
                0.5424f,
                0.125f,
                0f
                ) },
            { "Massachusetts", new(
                11,
                0.3f,
                0.09f,
                5659598,
                0.724f,
                0.086f,
                0.128f,
                0.387f,
                0.181f,
                0.913f,
                0.3887f,
                0.0597f,
                0.7656f,
                0.125f,
                0f
                ) },
            { "Michigan", new(
                15,
                0.47f,
                0.34f,
                7925350,
                0.738f,
                0.091f,
                0.118f,
                0.363f,
                0.187f,
                0.735f,
                0.3887f,
                0.0597f,
                0.7756f,
                0.02f,
                0f
                ) },
            { "Minnesota", new(
                10,
                0.46f,
                0.39f,
                4436981,
                0.829f,
                0.086f,
                0.121f,
                0.375f,
                0.171f,
                0.719f,
                0.431f,
                0.0743f,
                0.8164f,
                0.0655f,
                0f
                ) },
            { "Mississippi", new(
                6,
                0.42f,
                0.44f,
                2259864,
                0.804f,
                0.091f,
                0.108f,
                0.36f,
                0.176f,
                0.463f,
                0.3166f,
                0.0418f,
                0.58f,
                0f,
                0.12f
                ) },
            { "Missouri", new(
                10,
                0.42f,
                0.41f,
                4821686,
                0.757f,
                0.088f,
                0.119f,
                0.367f,
                0.18f,
                0.695f,
                0.3803f,
                0.0539f,
                0.8129f,
                0f,
                0.055f
                ) },
            { "Montana", new(
                4,
                0.3f,
                0.49f,
                897161,
                0.775f,
                0.087f,
                0.116f,
                0.358f,
                0.201f,
                0.534f,
                0.3754f,
                0.0528f,
                0.878f,
                0f,
                0.03325f
                ) },
            { "Nebraska", new(
                5,
                0.28f,
                0.49f,
                1497381,
                0.709f,
                0.09f,
                0.115f,
                0.364f,
                0.169f,
                0.73f,
                0.417f,
                0.0586f,
                0.8531f,
                0f,
                0.0625f
                ) },
            { "Nevada", new(
                6,
                0.33f,
                0.29f,
                2508220,
                0.662f,
                0.083f,
                0.128f,
                0.394f,
                0.171f,
                0.941f,
                0.3968f,
                0.0561f,
                0.6208f,
                0f,
                0.0575f
                ) },
            { "New Hampshire", new(
                4,
                0.31f,
                0.30f,
                1150004,
                0.783f,
                0.076f,
                0.122f,
                0.373f,
                0.201f,
                0.583f,
                0.441f,
                0.0847f,
                0.9198f,
                0.0825f,
                0f
                ) },
            { "New Jersey", new(
                14,
                0.39f,
                0.24f,
                7280551,
                0.846f,
                0.081f,
                0.116f,
                0.381f,
                0.173f,
                0.938f,
                0.3873f,
                0.0976f,
                0.655f,
                0.01875f,
                0f
                ) },
            { "New Mexico", new(
                5,
                0.44f,
                0.31f,
                1663024,
                0.686f,
                0.092f,
                0.114f,
                0.36f,
                0.194f,
                0.745f,
                0.3488f,
                0.0497f,
                0.7f,
                0.0368f,
                0f
                ) },
            { "New York", new(
                28,
                0.5f,
                0.22f,
                15611308,
                0.705f,
                0.085f,
                0.128f,
                0.384f,
                0.181f,
                0.874f,
                0.3719f,
                0.0752f,
                0.6231f,
                0.0459f,
                0f
                ) },
            { "North Carolina", new(
                16,
                0.34f,
                0.3f,
                8498868,
                0.698f,
                0.086f,
                0.119f,
                0.377f,
                0.176f,
                0.667f,
                0.3712f,
                0.0545f,
                0.6758f,
                0f,
                0.0175f
                ) },
            { "North Dakota", new(
                3,
                0.33f,
                0.5f,
                599192,
                0.773f,
                0.099f,
                0.131f,
                0.372f,
                0.167f,
                0.61f,
                0.4005f,
                0.0616f,
                0.8568f,
                0f,
                0.144f
                ) },
            { "Ohio", new(
                17,
                0.4f,
                0.42f,
                9207681,
                0.77f,
                0.085f,
                0.119f,
                0.367f,
                0.183f,
                0.763f,
                0.3876f,
                0.0554f,
                0.8047f,
                0f,
                0.02345f
                ) },
            { "Oklahoma", new(
                7,
                0.31f,
                0.51f,
                3087217,
                0.673f,
                0.09f,
                0.121f,
                0.371f,
                0.166f,
                0.646f,
                0.3638f,
                0.0049f,
                0.7115f,
                0f,
                0.191333f
                ) },
            { "Oregon", new(
                8,
                0.34f,
                0.25f,
                3401528,
                0.799f,
                0.083f,
                0.128f,
                0.395f,
                0.173f,
                0.805f,
                0.4022f,
                0.0643f,
                0.8259f,
                0.10172f,
                0f
                ) },
            { "Pennsylvania", new(
                19,
                0.46f,
                0.4f,
                10322678,
                0.763f,
                0.08f,
                0.119f,
                0.368f,
                0.196f,
                0.765f,
                0.3892f,
                0.0644f,
                0.7937f,
                0.02705f,
                0f
                ) },
            { "Rhode Island", new(
                4,
                0.41f,
                0.14f,
                892124,
                0.741f,
                0.085f,
                0.129f,
                0.38f,
                0.19f,
                0.911f,
                0.4235f,
                0.0669f,
                0.79f,
                0.0725f,
                0f
                ) },
            { "South Carolina", new(
                9,
                0.39f,
                0.43f,
                4229354,
                0.7f,
                0.084f,
                0.112f,
                0.362f,
                0.193f,
                0.679f,
                0.3653f,
                0.054f,
                0.6651f,
                0f,
                0.089f
                ) },
            { "South Dakota", new(
                3,
                0.26f,
                0.5f,
                697420,
                0.674f,
                0.08f,
                0.115f,
                0.351f,
                0.181f,
                0.572f,
                0.4069f,
                0.0533f,
                0.8361f,
                0f,
                0.1064f
                ) },
            { "Tennessee", new(
                11,
                0.36f,
                0.48f,
                5555761,
                0.743f,
                0.088f,
                0.123f,
                0.378f,
                0.173f,
                0.662f,
                0.3692f,
                0.0521f,
                0.7673f,
                0f,
                0.1275f
                ) },
            { "Texas", new(
                40,
                0.4f,
                0.39f,
                22942176,
                0.718f,
                0.095f,
                0.129f,
                0.394f,
                0.135f,
                0.837f,
                0.3844f,
                0.0632f,
                0.6916f,
                0f,
                0.07125f
                ) },
            { "Utah", new(
                6,
                0.14f,
                0.5f,
                2484582,
                0.674f,
                0.114f,
                0.131f,
                0.381f,
                0.12f,
                0.898f,
                0.4662f,
                0.0705f,
                0.8514f,
                0f,
                0.0944f
                ) },
            { "Vermont", new(
                3,
                0.57f,
                0.29f,
                532828,
                0.73f,
                0.077f,
                0.115f,
                0.364f,
                0.217f,
                0.351f,
                0.4203f,
                0.0629f,
                0.936f,
                0.294f,
                0f
                ) },
            { "Virginia", new(
                13,
                0.39f,
                0.43f,
                6834154,
                0.76f,
                0.086f,
                0.118f,
                0.379f,
                0.172f,
                0.756f,
                0.039f,
                0.0831f,
                0.6632f,
                0.08888f,
                0f
                ) },
            { "Washington", new(
                12,
                0.44f,
                0.33f,
                6164810,
                0.748f,
                0.084f,
                0.136f,
                0.403f,
                0.169f,
                0.834f,
                0.4091f,
                0.0785f,
                0.7353f,
                0.11425f,
                0f
                ) },
            { "West Virginia", new(
                4,
                0.33f,
                0.39f,
                1417859,
                0.673f,
                0.083f,
                0.104f,
                0.353f,
                0.214f,
                0.446f,
                0.3398f,
                0.0421f,
                0.9252f,
                0f,
                0.155f
                ) },
            { "Wisconsin", new(
                10,
                0.42f,
                0.42f,
                4661826,
                0.767f,
                0.087f,
                0.115f,
                0.362f,
                0.187f,
                0.671f,
                0.4206f,
                0.0583f,
                0.843f,
                0.044f,
                0f
                ) },
            { "Wyoming", new(
                3,
                0.1f,
                0.8f,
                454508,
                0.693f,
                0.076f,
                0.114f,
                0.368f,
                0.187f,
                0.62f,
                0.4181f,
                0.0545f,
                0.9035f,
                0f,
                0.1863f
                ) },
        };
    }

    public struct StateDataStruct
    {
        public int _electoralVotes, _pop;
        public float _demReg, _repReg, _regPopPercent;
        public float _youth, _lowerM, _upperM, _eld;
        public float _urb, _mid, _rich;
        public float _nonPoC;

        public float _demMod;
        public float _repMod;

        public StateDataStruct(int ev, float demReg, float repReg, int pop, float regPopPercent, float youth, float lowerM, float upperM, float eld,
        float urb, float mid, float rich, float nonPoC, float demMod, float repMod)
        {
            _electoralVotes = ev;
            _demReg = demReg;
            _repReg = repReg;
            _pop = pop;
            _regPopPercent = regPopPercent;
            _youth = youth;
            _lowerM = lowerM;
            _upperM = upperM;
            _eld = eld;
            _urb = urb;
            _mid = mid;
            _rich = rich;
            _nonPoC = nonPoC;
            _demMod = demMod;
            _repMod = repMod;
        }
    }
}





/*
OLD SHIT

09/03/2025 --- 0.4.2a

static class BaseData
    {
        public readonly static Dictionary<string, StateDataStruct> _initialData = new()
        {
            { "Alabama", new(
                9,
                0.35f,
                0.52f,
                3977628,
                0.68f,
                0.088f,
                0.117f,
                0.366f,
                0.181f,
                0.577f,
                0.3552f,
                0.0487f,
                0.675f,
                0f,
                0.1325f
                ) },
            { "Alaska", new(
                3,
                0.13f,
                0.24f,
                557899,
                0.742f,
                0.088f,
                0.134f,
                0.387f,
                0.139f,
                0.649f,
                0.4557f,
                0.0818f,
                0.6336f,
                0f,
                0.085f
                ) },
            { "Arizona", new(
                11,
                0.31f,
                0.34f,
                5848310,
                0.764f,
                0.095f,
                0.123f,
                0.366f,
                0.19f,
                0.893f,
                0.3919f,
                0.0603f,
                0.7377f,
                0f,
                0.025f
                ) },
            { "Arkansas", new(
                6,
                0.05f,
                0.07f,
                2362124,
                0.62f,
                0.092f,
                0.111f,
                0.36f,
                0.177f,
                0.555f,
                0.346f,
                0.0433f,
                0.7537f,
                0f,
                0.1825f
                ) },
            { "California", new(
                54,
                0.47f,
                0.24f,
                30159524,
                0.694f,
                0.089f,
                0.134f,
                0.4f,
                0.159f,
                0.942f,
                0.3854f,
                0.0851f,
                0.5605f,
                0.07f,
                0f
                ) },
            { "Colorado", new(
                10,
                0.27f,
                0.25f,
                4662926,
                0.713f,
                0.09f,
                0.142f,
                0.412f,
                0.158f,
                0.86f,
                0.4178f,
                0.0748f,
                0.8152f,
                0.0675f,
                0f
                ) },
            { "Connecticut", new(
                7,
                0.37f,
                0.2f,
                2894190,
                0.733f,
                0.083f,
                0.115f,
                0.373f,
                0.184f,
                0.863f,
                0.3882f,
                0.0944f,
                0.7422f,
                0.07f,
                0f
                ) },
            { "Delaware", new(
                3,
                0.48f,
                0.28f,
                819952,
                0.751f,
                0.074f,
                0.118f,
                0.355f,
                0.211f,
                0.826f,
                0.4082f,
                0.0698f,
                0.6744f,
                0.065f,
                0f
                ) },
            { "Washington DC", new(
                3,
                0.77f,
                0.05f,
                552380,
                0.869f,
                0.089f,
                0.198f,
                0.482f,
                0.134f,
                1f,
                0.3384f,
                0.0912f,
                0.4107f,
                0.777f,
                0f
                ) },
            { "Florida", new(
                30,
                0.34f,
                0.36f,
                18229883,
                0.671f,
                0.08f,
                0.113f,
                0.364f,
                0.216f,
                0.915f,
                0.3752f,
                0.0587f,
                0.7164f,
                0f,
                0.0725f
                ) },
            { "Georgia", new(
                16,
                0.41f,
                0.41f,
                8490546,
                0.707f,
                0.092f,
                0.121f,
                0.386f,
                0.151f,
                0.741f,
                0.3738f,
                0.0607f,
                0.5725f,
                0f,
                0.0225f
                ) },
            { "Hawaii", new(
                4,
                0.51f,
                0.28f,
                1141525,
                0.687f,
                0.072f,
                0.111f,
                0.369f,
                0.212f,
                0.861f,
                0.4341f,
                0.092f,
                0.2415f,
                0.03f,
                0f
                ) },
            { "Idaho", new(
                4,
                0.13f,
                0.58f,
                1497384,
                0.693f,
                0.09f,
                0.116f,
                0.365f,
                0.171f,
                0.692f,
                0.4161f,
                0.0514f,
                0.8841f,
                0f,
                0.13f
                ) },
            { "Illinois", new(
                19,
                0.48f,
                0.33f,
                9844167,
                0.744f,
                0.088f,
                0.123f,
                0.383f,
                0.171f,
                0.869f,
                0.398f,
                0.0713f,
                0.6979f,
                0.055f,
                0f
                ) },
            { "Indiana", new(
                11,
                0.37f,
                0.42f,
                5274945,
                0.92f,
                0.092f,
                0.119f,
                0.368f,
                0.168f,
                0.712f,
                0.4014f,
                0.0534f,
                0.8228f,
                0f,
                0.065f
                ) },
            { "Iowa", new(
                6,
                0.32f,
                0.34f,
                2476882,
                0.76f,
                0.089f,
                0.116f,
                0.36f,
                0.182f,
                0.632f,
                0.4093f,
                0.0571f,
                0.8909f,
                0f,
                0.02f
                ) },
            { "Kansas", new(
                6,
                0.26f,
                0.44f,
                2246209,
                0.708f,
                0.094f,
                0.114f,
                0.358f,
                0.178f,
                0.723f,
                0.403f,
                0.0581f,
                0.8296f,
                0f,
                0.045f
                ) },
            { "Kentucky", new(
                8,
                0.45f,
                0.45f,
                3509259,
                0.759f,
                0.088f,
                0.117f,
                0.368f,
                0.175f,
                0.587f,
                0.3665f,
                0.0469f,
                0.8625f,
                0f,
                0.11f
                ) },
            { "Louisiana", new(
                8,
                0.4f,
                0.33f,
                3506600,
                0.693f,
                0.087f,
                0.117f,
                0.367f,
                0.17f,
                0.715f,
                0.3337f,
                0.0481f,
                0.6125f,
                0f,
                0.13f
                ) },
            { "Maine", new(
                4,
                0.36f,
                0.28f,
                1146670,
                0.774f,
                0.069f,
                0.113f,
                0.364f,
                0.225f,
                0.386f,
                0.3763f,
                0.056f,
                0.9368f,
                0.11f,
                0f
                ) },
            { "Maryland", new(
                10,
                0.54f,
                0.24f,
                4818337,
                0.786f,
                0.08f,
                0.118f,
                0.382f,
                0.169f,
                0.856f,
                0.4072f,
                0.0972f,
                0.5424f,
                0.115f,
                0f
                ) },
            { "Massachusetts", new(
                11,
                0.3f,
                0.09f,
                5659598,
                0.724f,
                0.086f,
                0.128f,
                0.387f,
                0.181f,
                0.913f,
                0.3887f,
                0.0597f,
                0.7656f,
                0.1125f,
                0f
                ) },
            { "Michigan", new(
                15,
                0.47f,
                0.34f,
                7925350,
                0.738f,
                0.091f,
                0.118f,
                0.363f,
                0.187f,
                0.735f,
                0.3887f,
                0.0597f,
                0.7756f,
                0.01125f,
                0f
                ) },
            { "Minnesota", new(
                10,
                0.46f,
                0.39f,
                4436981,
                0.829f,
                0.086f,
                0.121f,
                0.375f,
                0.171f,
                0.719f,
                0.431f,
                0.0743f,
                0.8164f,
                0.055f,
                0f
                ) },
            { "Mississippi", new(
                6,
                0.42f,
                0.44f,
                2259864,
                0.804f,
                0.091f,
                0.108f,
                0.36f,
                0.176f,
                0.463f,
                0.3166f,
                0.0418f,
                0.58f,
                0f,
                0.105f
                ) },
            { "Missouri", new(
                10,
                0.42f,
                0.41f,
                4821686,
                0.757f,
                0.088f,
                0.119f,
                0.367f,
                0.18f,
                0.695f,
                0.3803f,
                0.0539f,
                0.8129f,
                0f,
                0.06f
                ) },
            { "Montana", new(
                4,
                0.3f,
                0.49f,
                897161,
                0.775f,
                0.087f,
                0.116f,
                0.358f,
                0.201f,
                0.534f,
                0.3754f,
                0.0528f,
                0.878f,
                0f,
                0.035f
                ) },
            { "Nebraska", new(
                5,
                0.28f,
                0.49f,
                1497381,
                0.709f,
                0.09f,
                0.115f,
                0.364f,
                0.169f,
                0.73f,
                0.417f,
                0.0586f,
                0.8531f,
                0f,
                0.06f
                ) },
            { "Nevada", new(
                6,
                0.33f,
                0.29f,
                2508220,
                0.662f,
                0.083f,
                0.128f,
                0.394f,
                0.171f,
                0.941f,
                0.3968f,
                0.0561f,
                0.6208f,
                0f,
                0.045f
                ) },
            { "New Hampshire", new(
                4,
                0.31f,
                0.30f,
                1150004,
                0.783f,
                0.076f,
                0.122f,
                0.373f,
                0.201f,
                0.583f,
                0.441f,
                0.0847f,
                0.9198f,
                0.075f,
                0f
                ) },
            { "New Jersey", new(
                14,
                0.39f,
                0.24f,
                7280551,
                0.846f,
                0.081f,
                0.116f,
                0.381f,
                0.173f,
                0.938f,
                0.3873f,
                0.0976f,
                0.655f,
                0.01f,
                0f
                ) },
            { "New Mexico", new(
                5,
                0.44f,
                0.31f,
                1663024,
                0.686f,
                0.092f,
                0.114f,
                0.36f,
                0.194f,
                0.745f,
                0.3488f,
                0.0497f,
                0.7f,
                0.027f,
                0f
                ) },
            { "New York", new(
                28,
                0.5f,
                0.22f,
                15611308,
                0.705f,
                0.085f,
                0.128f,
                0.384f,
                0.181f,
                0.874f,
                0.3719f,
                0.0752f,
                0.6231f,
                0.04f,
                0f
                ) },
            { "North Carolina", new(
                16,
                0.34f,
                0.3f,
                8498868,
                0.698f,
                0.086f,
                0.119f,
                0.377f,
                0.176f,
                0.667f,
                0.3712f,
                0.0545f,
                0.6758f,
                0f,
                0.01f
                ) },
            { "North Dakota", new(
                3,
                0.33f,
                0.5f,
                599192,
                0.773f,
                0.099f,
                0.131f,
                0.372f,
                0.167f,
                0.61f,
                0.4005f,
                0.0616f,
                0.8568f,
                0f,
                0.145f
                ) },
            { "Ohio", new(
                17,
                0.4f,
                0.42f,
                9207681,
                0.77f,
                0.085f,
                0.119f,
                0.367f,
                0.183f,
                0.763f,
                0.3876f,
                0.0554f,
                0.8047f,
                0f,
                0.03f
                ) },
            { "Oklahoma", new(
                7,
                0.31f,
                0.51f,
                3087217,
                0.673f,
                0.09f,
                0.121f,
                0.371f,
                0.166f,
                0.646f,
                0.3638f,
                0.0049f,
                0.7115f,
                0f,
                0.175f
                ) },
            { "Oregon", new(
                8,
                0.34f,
                0.25f,
                3401528,
                0.799f,
                0.083f,
                0.128f,
                0.395f,
                0.173f,
                0.805f,
                0.4022f,
                0.0643f,
                0.8259f,
                0.09f,
                0f
                ) },
            { "Pennsylvania", new(
                19,
                0.46f,
                0.4f,
                10322678,
                0.763f,
                0.08f,
                0.119f,
                0.368f,
                0.196f,
                0.765f,
                0.3892f,
                0.0644f,
                0.7937f,
                0.02f,
                0f
                ) },
            { "Rhode Island", new(
                4,
                0.41f,
                0.14f,
                892124,
                0.741f,
                0.085f,
                0.129f,
                0.38f,
                0.19f,
                0.911f,
                0.4235f,
                0.0669f,
                0.79f,
                0.065f,
                0f
                ) },
            { "South Carolina", new(
                9,
                0.39f,
                0.43f,
                4229354,
                0.7f,
                0.084f,
                0.112f,
                0.362f,
                0.193f,
                0.679f,
                0.3653f,
                0.054f,
                0.6651f,
                0f,
                0.08f
                ) },
            { "South Dakota", new(
                3,
                0.26f,
                0.5f,
                697420,
                0.674f,
                0.08f,
                0.115f,
                0.351f,
                0.181f,
                0.572f,
                0.4069f,
                0.0533f,
                0.8361f,
                0f,
                0.105f
                ) },
            { "Tennessee", new(
                11,
                0.36f,
                0.48f,
                5555761,
                0.743f,
                0.088f,
                0.123f,
                0.378f,
                0.173f,
                0.662f,
                0.3692f,
                0.0521f,
                0.7673f,
                0f,
                0.12f
                ) },
            { "Texas", new(
                40,
                0.4f,
                0.39f,
                22942176,
                0.718f,
                0.095f,
                0.129f,
                0.394f,
                0.135f,
                0.837f,
                0.3844f,
                0.0632f,
                0.6916f,
                0f,
                0.07f
                ) },
            { "Utah", new(
                6,
                0.14f,
                0.5f,
                2484582,
                0.674f,
                0.114f,
                0.131f,
                0.381f,
                0.12f,
                0.898f,
                0.4662f,
                0.0705f,
                0.8514f,
                0f,
                0.08f
                ) },
            { "Vermont", new(
                3,
                0.57f,
                0.29f,
                532828,
                0.73f,
                0.077f,
                0.115f,
                0.364f,
                0.217f,
                0.351f,
                0.4203f,
                0.0629f,
                0.936f,
                0.26f,
                0f
                ) },
            { "Virginia", new(
                13,
                0.39f,
                0.43f,
                6834154,
                0.76f,
                0.086f,
                0.118f,
                0.379f,
                0.172f,
                0.756f,
                0.039f,
                0.0831f,
                0.6632f,
                0.07f,
                0f
                ) },
            { "Washington", new(
                12,
                0.44f,
                0.33f,
                6164810,
                0.748f,
                0.084f,
                0.136f,
                0.403f,
                0.169f,
                0.834f,
                0.4091f,
                0.0785f,
                0.7353f,
                0.11f,
                0f
                ) },
            { "West Virginia", new(
                4,
                0.33f,
                0.39f,
                1417859,
                0.673f,
                0.083f,
                0.104f,
                0.353f,
                0.214f,
                0.446f,
                0.3398f,
                0.0421f,
                0.9252f,
                0f,
                0.17f
                ) },
            { "Wisconsin", new(
                10,
                0.42f,
                0.42f,
                4661826,
                0.767f,
                0.087f,
                0.115f,
                0.362f,
                0.187f,
                0.671f,
                0.4206f,
                0.0583f,
                0.843f,
                0.04f,
                0f
                ) },
            { "Wyoming", new(
                3,
                0.1f,
                0.8f,
                454508,
                0.693f,
                0.076f,
                0.114f,
                0.368f,
                0.187f,
                0.62f,
                0.4181f,
                0.0545f,
                0.9035f,
                0f,
                0.16f
                ) },
        };
    }

*/