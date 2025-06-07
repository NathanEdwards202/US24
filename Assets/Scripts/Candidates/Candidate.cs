using UnityEngine;



namespace Candidates
{
    public enum Party
    {
        DEMOCRAT,
        REPUBLICAN
    }

    public class Candidate : MonoBehaviour
    {
        [SerializeField] CandidateSO _template;

        [SerializeField] Party _party;
        [SerializeField] string _name;
        [SerializeField] int _age;

        [Space(20)]

        [SerializeField] float _radicalismPercent;
        [SerializeField] float _nameRecognitionPercent;
        [SerializeField] float _coveragePercent;
        [SerializeField] float _passionPercent;
        [SerializeField] float _speechGivingPercent;
        [SerializeField] float _neutralPullPercent;
        [SerializeField] float _youthPercent;
        [SerializeField] float _middleAgePercent;
        [SerializeField] float _elderlyPercent;
        [SerializeField] float _urbanPercent;
        [SerializeField] float _ruralPercent;
        [SerializeField] float _workingClassPercent;
        [SerializeField] float _middleClassPercent;
        [SerializeField] float _richPercent;
        [SerializeField] float _nonPoCPercent;
        [SerializeField] float _poCPercent;
        float _averageExcludingRadicalism;

        float _popularVote = 0;
        public int _confirmedElectoralVotes { get; private set; }
        int _leaningElectoralVotes = 0;

        private void Awake()
        {
            CreateFromTemplate();
        }

        void CreateFromTemplate()
        {
            _name = _template._name;

            _radicalismPercent = 10/20f + (_template._radicalismPercent * 5f / 100f / 20f * 10f);
            _nameRecognitionPercent = 14/20f + (_template._nameRecognitionPercent * 5f / 100f / 20f * 6f);
            _coveragePercent = 14/20f + (_template._coveragePercent * 5f / 100f / 20f * 6f);
            _passionPercent = 14 / 20f + (_template._passionPercent * 5f / 100f / 20f * 6f);
            _speechGivingPercent = 14 / 20f + (_template._speechGivingPercent * 5f / 100 / 20f * 6f);
            _neutralPullPercent = 10 / 20f + (_template._neutralPullPercent * 5f / 100f / 20f * 10f);
            _youthPercent = 8 / 20f + (_template._youthPercent * 5f / 100f / 20f * 12f);
            _middleAgePercent = 8 / 20f + (_template._middleAgePercent * 5f / 100f / 20f * 12f);
            _elderlyPercent = 8 / 20f + (_template._elderlyPercent * 5f / 100f / 20f * 12f);
            _urbanPercent = 8 / 20f + (_template._urbanPercent * 5f / 100f / 20f * 12f);
            _ruralPercent = 8 / 20f + (_template._ruralPercent * 5f / 100f / 20f * 12f);
            _workingClassPercent = 8 / 20f + (_template._workingClassPercent * 5f / 100f / 20f * 12f);
            _middleClassPercent = 8 / 20f + (_template._middleClassPercent * 5f / 100f / 20f * 12f);
            _richPercent = 8 / 20f + (_template._richPercent * 5f / 100f / 20f * 12f);
            _nonPoCPercent = 8 / 20f + (_template._nonPoCPercent * 5f / 100f / 20f * 12f);
            _poCPercent = 8 / 20f + (_template._poCPercent * 5f / 100f / 20f * 12f);

            CalculateAverage();
        }

        void CalculateAverage()
        {
            _averageExcludingRadicalism = _nameRecognitionPercent + _coveragePercent + _passionPercent + _speechGivingPercent + _neutralPullPercent + _youthPercent + _middleAgePercent + _elderlyPercent + _urbanPercent + _ruralPercent + _workingClassPercent + _middleClassPercent + _richPercent + _nonPoCPercent + _poCPercent;
            _averageExcludingRadicalism /= 15f;
        }

        float CalculateAverageIncludingRadicalism()
        {
            float averageIncludingRadicalism = _averageExcludingRadicalism * 15f;

            averageIncludingRadicalism += _radicalismPercent;

            averageIncludingRadicalism /= 16f;

            return averageIncludingRadicalism;
        }

        #region Accessors
        public Party GetParty()
        {
            return _party;
        }

        public string GetName()
        {
            return _name;
        }

        public int GetAge()
        {
            return _age;
        }


        public float GetRadicalismPercent()
        {
            return _radicalismPercent;
        }

        public float GetNameRecognitionPercent()
        {
            return _nameRecognitionPercent;
        }

        public float GetCoveragePercent()
        {
            return _coveragePercent;
        }

        public float GetPassionPercent()
        {
            return _passionPercent;
        }

        public float GetSpeechGivingPercent()
        {
            return _speechGivingPercent;
        }

        public float GetNeutralPullPercent()
        {
            return _neutralPullPercent;
        }

        public float GetYouthPercent()
        {
            return _youthPercent;
        }

        public float GetMiddleAgePercent()
        {
            return _middleAgePercent;
        }

        public float GetElderlyPercent()
        {
            return _elderlyPercent;
        }

        public float GetRuralPercent()
        {
            return _ruralPercent;
        }

        public float GetUrbanPercent()
        {
            return _urbanPercent;
        }

        public float GetWorkingClassPercent()
        {
            return _workingClassPercent;
        }

        public float GetMiddleClassPercent()
        {
            return _middleClassPercent;
        }

        public float GetRichPercent()
        {
            return _richPercent;
        }

        public float GetNonPoCPercent()
        {
            return _nonPoCPercent;
        }

        public float GetPoCPercent()
        {
            return _poCPercent;
        }

        public float GetAveragePercent()
        {
            return _averageExcludingRadicalism;
        }

        public float GetAverageIncludingRadicalismPercent()
        {
            return CalculateAverageIncludingRadicalism();
        }

        public float GetPopularVote()
        {
            return _popularVote;
        }

        public int GetConfirmedElectoralVotes()
        {
            return _confirmedElectoralVotes;
        }

        public int GetLeaningElectoralVotes()
        {
            return _leaningElectoralVotes;
        }
        #endregion


        #region Mutators
        public void SetPopularVote(float value)
        {
            _popularVote = value;
        }

        public void SetConfimedElectoralVotes(int value)
        {
            _confirmedElectoralVotes = value;
        }

        public void SetLeaningElectoralVotes(int value)
        {
            _leaningElectoralVotes = value;
        }
        #endregion
    }
}