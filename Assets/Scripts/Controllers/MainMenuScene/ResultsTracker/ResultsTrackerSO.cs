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
            OnNewResult(1988, "Michael S. Dukakis", "George Bush", 111, 426);
            OnNewResult(1992, "William J. Clinton", "George Bush", 370, 168);
            OnNewResult(1996, "William J. Clinton", "Robert Dole", 379, 159);
            OnNewResult(2000, "Albert Gore, Jr.", "George W. Bush", 266, 271);
            OnNewResult(2004, "John F. Kerry", "George W. Bush", 251, 286);
            OnNewResult(2008, "Barack H. Obama", "John S. McCain", 365, 173);
            OnNewResult(2012, "Barack H. Obama", "W. Mitt Romney", 332, 206);
            OnNewResult(2016, "Hillary R. Clinton", "Donald J. Trump", 227, 304);
            OnNewResult(2020, "Joseph R. Biden", "Donald J. Trump", 306, 232);
            OnNewResult(2024, "Kamala D. Harris", "Donald J. Trump", 226, 312);
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