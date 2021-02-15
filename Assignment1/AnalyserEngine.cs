using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;

namespace Assignment1
{
    class AnalyserEngine
    {
        private IGUIAdapter.Adapter gui;
        public Dictionary<int, ProcessingPipeline> Indexes { get; } = new Dictionary<int, ProcessingPipeline>();
        public Dictionary<int, MovieIndex> UnProcessedIndexes { get; } = new Dictionary<int, MovieIndex>();
        public Dictionary<string, int> CorpusWordCount { get; } = new Dictionary<string, int>();
        public AnalyserEngine(Dictionary<int, MovieIndex> unProcessedIndexes, IGUIAdapter.Adapter gui)
        {
            this.gui = gui;
            this.UnProcessedIndexes = unProcessedIndexes;
            GeneratePipes();
        }

        private void GeneratePipes()
        {
            foreach (var entry in UnProcessedIndexes)
            {   
                Indexes[entry.Key] = new ProcessingPipeline.Builder(entry.Value.GetFullText()).
                    SplitSentences().
                    SplitBulletPoints().
                    RemovePunctuation().
                    Normalize().
                    Tokenize().
                    Build();
                GenerateWordCounts(Indexes[entry.Key]);
            }
            Indexes.Clear();
        }

        private void GenerateWordCounts(ProcessingPipeline pipe)
        {
            foreach (var wordCountPair in pipe.TokenWordCount)
            {
                if (CorpusWordCount.ContainsKey(wordCountPair.Key))
                    CorpusWordCount[wordCountPair.Key] += wordCountPair.Value;
                else
                    CorpusWordCount[wordCountPair.Key] = wordCountPair.Value;
            }
        }

        public Tuple<float[], float[], string> GetZipfsNormStats()
        {
            var CorpusWCList = GetZipfsBaseList();
            float[] xInt = new float[CorpusWCList.Count];
            float[] yInt = new float[CorpusWCList.Count];
            for (int i = 0; i < CorpusWCList.Count; i++)
            {
                xInt[i] = CorpusWCList[i].Value + 1;
                yInt[i] = i + 1;
            }
            return new Tuple<float[], float[], string>(xInt, yInt, "Zipfs Normal");
        }

        public Tuple<float[], float[], string> GetZipfsLogStats()
        {
            var CorpusWCList = GetZipfsBaseList();
            float[] x = new float[CorpusWCList.Count];
            float[] y = new float[CorpusWCList.Count];
            for (int i = 0; i < CorpusWCList.Count; i++)
            {
                var val = Math.Log(CorpusWCList[i].Value + 1);
                y[i] = (float)Math.Log(CorpusWCList[i].Value);
                x[i] = (float)Math.Log(i + 1);
            }
            return new Tuple<float[], float[], string>(x, y, "Xipfs Natural Log");
        }

        private void PrintStatsToGui(string message, Tuple<float[], float[], string> inputs, int startIndex, int endIndex, int match)
        {
            var CorpusWCList = GetZipfsBaseList();
            message += "\n";
            for (int i = startIndex; i <= endIndex; ++i)
            {
                message += $"Rank: {i} Frequency: {CorpusWCList[i].Value} String: {CorpusWCList[i].Key}\n";
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
            var CorpusWCList = GetZipfsBaseList();
            var logStats = GetZipfsLogStats();
            var maxRank = logStats.Item1[logStats.Item1.Length - 1];
            int rank = (int)Math.Pow(maxRank / 2, Math.E);
            int startIndexPrint = Math.Max(rank - 10, 0);
            var result = new Tuple<float[], float[], string>(
                new float[] { 0, rank, logStats.Item1.Length },
                new float[] { CorpusWCList[0].Value, CorpusWCList[rank].Value, CorpusWCList[logStats.Item1.Length - 1].Value },
                "Selection Log Mid Point"
                );
            PrintStatsToGui("Zipf log mid point selection", result, startIndexPrint, rank + 10, rank);
            return result;
        }

        public Tuple<float[], float[], string> SelectStopWordsStandardDev()
        {
            var CorpusWCList = GetZipfsBaseList();
            List<int> values = new List<int>();
            for (int i = 0; i < CorpusWCList.Count; i++)
            {
                for (int j = 0; j < CorpusWCList[i].Value; ++j)
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
                new float[] { CorpusWCList[0].Value, CorpusWCList[Math.Max(meanIndex - 1, 1) - 1].Value, CorpusWCList[plus1deviation - 1].Value, CorpusWCList[CorpusWCList.Count - 1].Value },
                "Selection Standard Deviation"
                );
            var startIndex = Math.Max(meanIndex - 10, 0);
            PrintStatsToGui("Selection Standard Deviation", result, startIndex, meanIndex + 10, meanIndex);
            return result;
        }

        public Tuple<float[], float[], string> SelectStopWordsMedianIQRange()
        {
            var CorpusWCList = GetZipfsBaseList();
            List<int> values = new List<int>();
            for (int i = 0; i < CorpusWCList.Count; i++)
            {
                for (int j = 0; j < CorpusWCList[i].Value; ++j)
                    values.Add(i + 1);
            }
            float median = values[values.Count / 2];
            float q1 = values[values.Count / 4];
            float q3 = values[3 * values.Count / 4];
            var result =  new Tuple<float[], float[], string>(
                new float[] { 1, q1, median, q3, CorpusWCList.Count },
                new float[] { CorpusWCList[0].Value, CorpusWCList[(int)Math.Max(q1, 1) - 1].Value, CorpusWCList[(int)median - 1].Value, CorpusWCList[(int)q3].Value, CorpusWCList[CorpusWCList.Count - 1].Value },
                "Selection IQ Range"
                );
            var startIndex = Math.Max((int)q1 - 10, 0);
            PrintStatsToGui("Selection IQ Range Q1", result, startIndex, (int)q1 + 10, (int)q1);
            startIndex = Math.Max((int)median - 10, 0);
            PrintStatsToGui("Selection IQ Range Median", result, startIndex, (int)median + 10, (int)median);
            startIndex = Math.Max((int)q3 - 10, 0);
            PrintStatsToGui("Selection IQ Range Q3", result, startIndex, (int)q3 + 10, (int)q3);
            return result;
        }

        private List<KeyValuePair<string, int>> GetZipfsBaseList()
        {
            var CorpusWCList = CorpusWordCount.ToList();
            // sorting is unneccesary, but for some reason this eats up ram without sorting for me
            CorpusWCList.Sort((first, second) => second.Value.CompareTo(first.Value));
            return CorpusWCList;
        }
    }
}
