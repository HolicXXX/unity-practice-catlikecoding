﻿using UnityEngine;
using UnityEngine.UI;

namespace NucleonPerformance
{
    [RequireComponent(typeof(FPSCounter))]
    public class FPSDisplay : MonoBehaviour {

        [System.Serializable]
        private struct FPSColor
        {
            public Color color;
            public int minimumFPS;
        }

        [SerializeField]
        private Text highestFPSLabel, averageFPSLabel, lowestFPSLabel;

        private FPSCounter counter;

        [SerializeField]
        private FPSColor[] coloring;

        static string[] stringsFrom00To99 = {
            "00", "01", "02", "03", "04", "05", "06", "07", "08", "09",
            "10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
            "20", "21", "22", "23", "24", "25", "26", "27", "28", "29",
            "30", "31", "32", "33", "34", "35", "36", "37", "38", "39",
            "40", "41", "42", "43", "44", "45", "46", "47", "48", "49",
            "50", "51", "52", "53", "54", "55", "56", "57", "58", "59",
            "60", "61", "62", "63", "64", "65", "66", "67", "68", "69",
            "70", "71", "72", "73", "74", "75", "76", "77", "78", "79",
            "80", "81", "82", "83", "84", "85", "86", "87", "88", "89",
            "90", "91", "92", "93", "94", "95", "96", "97", "98", "99"
        };

        private void Awake()
        {
            counter = GetComponent<FPSCounter>();
        }

	    // Update is called once per frame
	    void Update () {
            DisplayText(highestFPSLabel, counter.HighestFPS);
            DisplayText(averageFPSLabel, counter.AverageFPS);
            DisplayText(lowestFPSLabel, counter.LowestFPS);
        }

        void DisplayText(Text text, int fps)
        {
            text.text = stringsFrom00To99[Mathf.Clamp(fps, 0, 99)];
            foreach(var c in coloring)
            {
                if(fps >= c.minimumFPS)
                {
                    text.color = c.color;
                    break;
                }
            }
        }
    }
}
