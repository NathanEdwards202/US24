using System;
using UnityEngine;



namespace Candidates
{
    [Serializable]
    [CreateAssetMenu(fileName = "Candidate", menuName = "ScriptableObjects/Candidate")]
    public class CandidateSO : ScriptableObject
    {
        public string _name;

        [Space(20)]

        public float _radicalismPercent;
        public float _nameRecognitionPercent;
        public float _coveragePercent;
        public float _passionPercent;
        public float _speechGivingPercent;
        public float _neutralPullPercent;
        public float _youthPercent;
        public float _middleAgePercent;
        public float _elderlyPercent;
        public float _urbanPercent;
        public float _ruralPercent;
        public float _workingClassPercent;
        public float _middleClassPercent;
        public float _richPercent;
        public float _nonPoCPercent;
        public float _poCPercent;
    }
}