using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


namespace Controllers.MainScene.ResultsTracker
{
    [CreateAssetMenu(fileName = "ResultsTrackerSO", menuName = "ScriptableObjects/MainMenuScene/ResultsTracker")]
    public class ResultsTrackerSO : ScriptableObject
    {
        public List<Result> _results;

        public void StartSelf()
        {
            _results = new();
            OnNewResult(2024, "Kamala Harris", "Donald Trump", 226, 312);
        }

        public void OnNewResult(int yr, string dn, string rn, int dev, int rev)
        {
            _results.Insert(0, new(yr, dn, rn, dev, rev));
            if (_results.Count >= 10)
            {
                _results = _results.GetRange(0, 10);
            }
            else
            {
                _results = _results.GetRange(0, _results.Count);
            }
        }
    }

    public struct Result
    {
        public Result(int yr, string dn, string rn, int dev, int rev)
        {
            year = yr;

            demName = dn;
            demEV = dev;

            repName = rn;
            repEV = rev;
        }

        public string GetResultDisplayTextFromThis()
        {
            return $"{year}:\n"
                 + $"{demName} {demEV} - {repEV} {repName}";
        }

        int year;

        string repName;
        string demName;

        int demEV;
        int repEV;
    }
}