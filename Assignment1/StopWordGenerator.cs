﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment1
{ 
    /// <summary>
    /// StopWordGenerator mainly utilizes Zipfs law to generate a list of stop words giving 4 options
    /// of stop word removal:
    /// 
    /// Mean - Remove words based on their rank and the mean average word count
    /// Log Midpoint - takes logarithm of their zipf rank and selects words with ranks higher
    ///                than the rank at the middle of the logarithm
    /// InterQuartile - remove words based on the median rank
    /// TopN - remove words from a given input rank number
    /// </summary>
    public class StopWordGenerator
    {
        private IGUIAdapter.Adapter gui;
        private BagOfWords CorpusBOW;
        private List<string> _stopWords = new List<string>();
        public List<string> StopWords { get { return _stopWords; } }

        public StopWordGenerator(IGUIAdapter.Adapter gui, BagOfWords corpusBOW)
        {
            this.gui = gui;
            this.CorpusBOW = corpusBOW;
        }

        public Tuple<float[], float[], string> GetZipfsNormStats()
        {
            var CorpusWCList = GetZipfsRankedList();
            float[] xInt = new float[CorpusWCList.Count];
            float[] yInt = new float[CorpusWCList.Count];
            for (int i = 0; i < CorpusWCList.Count; i++)
            {
                xInt[i] = CorpusWCList[i].Value.TotalFreq + 1;
                yInt[i] = i + 1;
            }
            return new Tuple<float[], float[], string>(xInt, yInt, "Zipfs Normal");
        }

        private void MakeStopWordList(int index, List<KeyValuePair<string, WordStats>> rankedList)
        {
            _stopWords = (from KeyValuePair<string, WordStats> pair in rankedList.Take(index) select pair.Key).ToList();
        }

        private void PrintStatsToGui(string message, int startIndex, int endIndex, int match)
        {
            var CorpusWCList = GetZipfsRankedList();
            message = "StopWord selection stats:-\n" + message;
            message += "\n";
            for (int i = startIndex; i <= endIndex; ++i)
            {
                message += $"Rank: {i} Frequency: {CorpusWCList[i].Value.TotalFreq} String: {CorpusWCList[i].Key}\n";
                if (i == match)
                {
                    message = message.Substring(0, message.Length - 1);
                    message += " - ***Selected Match***\n";
                }
            };
            message = message.Substring(0, message.Length - 1);
            gui.AddConsoleMessage(message);
        }

        public Tuple<float[], float[], string> SelectStopWordsLogMidPoint()
        {
            var CorpusWCList = GetZipfsRankedList();
            var logStats = GetZipfsLogStats();
            var maxRank = logStats.Item1[logStats.Item1.Length - 1];
            int rank = (int)Math.Pow(maxRank / 2, Math.E);
            int startIndexPrint = Math.Max(rank - 10, 0);
            var result = new Tuple<float[], float[], string>(
                new float[] { 0, rank, logStats.Item1.Length },
                new float[] { CorpusWCList[0].Value.TotalFreq,
                    CorpusWCList[rank].Value.TotalFreq,
                    CorpusWCList[logStats.Item1.Length - 1].Value.TotalFreq },
                "Selection Log Mid Point"
                );
            PrintStatsToGui("Zipf log mid point selection", startIndexPrint, rank + 10, rank);
            MakeStopWordList(rank, CorpusWCList);
            return result;
        }

        public Tuple<float[], float[], string> GetZipfsLogStats()
        {
            var CorpusWCList = GetZipfsRankedList();
            float[] x = new float[CorpusWCList.Count];
            float[] y = new float[CorpusWCList.Count];
            for (int i = 0; i < CorpusWCList.Count; i++)
            {
                y[i] = (float)Math.Log(CorpusWCList[i].Value.TotalFreq);
                x[i] = (float)Math.Log(i + 1);
            }
            return new Tuple<float[], float[], string>(x, y, "Xipfs Natural Log");
        }

        public Tuple<float[], float[], string> SelectStopWordsStandardDev()
        {
            var CorpusWCList = GetZipfsRankedList();
            List<int> values = new List<int>();
            for (int i = 0; i < CorpusWCList.Count; i++)
            {
                for (int j = 0; j < CorpusWCList[i].Value.TotalFreq; ++j)
                    values.Add(i + 1);
            }
            int[] valuesInt = values.ToArray();
            float total = valuesInt.Count();
            float sum = valuesInt.Sum();
            float meanAvg = sum / total;
            var valuesSubMeanSq = from number in valuesInt select Math.Pow(number - meanAvg, 2);
            float sumSqMean = (float)valuesSubMeanSq.Sum() / valuesSubMeanSq.Count();
            float standardDeviation = (float)Math.Sqrt(sumSqMean);
            int meanIndex = (int)Math.Round(meanAvg);
            int plus1deviation = (int)Math.Round(meanAvg + standardDeviation);
            var result = new Tuple<float[], float[], string>(
                new float[] { 1, meanIndex, plus1deviation, CorpusWCList.Count },
                new float[] { CorpusWCList[0].Value.TotalFreq,
                    CorpusWCList[Math.Max(meanIndex - 1, 1) - 1].Value.TotalFreq,
                    CorpusWCList[plus1deviation - 1].Value.TotalFreq,
                    CorpusWCList[CorpusWCList.Count - 1].Value.TotalFreq },
                "Selection Standard Deviation"
                );
            var startIndex = Math.Max(meanIndex - 10, 0);
            PrintStatsToGui("Selection Standard Deviation", startIndex, meanIndex + 10, meanIndex);
            MakeStopWordList(meanIndex, CorpusWCList);
            return result;
        }

        public void SelectStopWordsN(int n)
        {
            var CorpusWCList = GetZipfsRankedList();
            PrintStatsToGui("Selection top " + n + " words", Math.Max(n - 10, 0), n + 10, n);
            MakeStopWordList(n, CorpusWCList);
        }

        public Tuple<float[], float[], string> SelectStopWordsMedianIQRange()
        {
            var CorpusWCList = GetZipfsRankedList();
            List<int> values = new List<int>();
            for (int i = 0; i < CorpusWCList.Count; i++)
            {
                for (int j = 0; j < CorpusWCList[i].Value.TotalFreq; ++j)
                    values.Add(i + 1);
            }
            float median = values[values.Count / 2];
            float q1 = values[values.Count / 4];
            float q3 = values[3 * values.Count / 4];
            var result = new Tuple<float[], float[], string>(
                new float[] { 1, q1, median, q3, CorpusWCList.Count },
                new float[] { CorpusWCList[0].Value.TotalFreq,
                    CorpusWCList[(int)Math.Max(q1, 1) - 1].Value.TotalFreq,
                    CorpusWCList[(int)median - 1].Value.TotalFreq,
                    CorpusWCList[(int)q3].Value.TotalFreq,
                    CorpusWCList[CorpusWCList.Count - 1].Value.TotalFreq },
                "Selection IQ Range"
                );
            var startIndex = Math.Max((int)q1 - 10, 0);
            PrintStatsToGui("Selection IQ Range Q1", startIndex, (int)q1 + 10, (int)q1);
            startIndex = Math.Max((int)median - 10, 0);
            PrintStatsToGui("Selection IQ Range Median", startIndex, (int)median + 10, (int)median);
            startIndex = Math.Max((int)q3 - 10, 0);
            PrintStatsToGui("Selection IQ Range Q3", startIndex, (int)q3 + 10, (int)q3);
            MakeStopWordList((int)median, CorpusWCList);
            return result;
        }

        private List<KeyValuePair<string, WordStats>> GetZipfsRankedList()
        {
            var CorpusWCList = CorpusBOW.Terms.ToList();
            // sorts by total frequency, i.e a ranked sorted list with 0 being most frequent
            CorpusWCList.Sort((first, second) => second.Value.TotalFreq.CompareTo(first.Value.TotalFreq));
            return CorpusWCList;
        }
    }
}