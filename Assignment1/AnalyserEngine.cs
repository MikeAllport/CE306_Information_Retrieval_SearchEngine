using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using static Assignment1.StopWordDetectionType;
using Utils;

namespace Assignment1
{
    public enum StopWordDetectionType
    {
        MEAN, LOG_MIDPOINT, INTER_QUARTILE, TOP_N
    }
    public class AnalyserEngineSettings
    {
        public StopWordDetectionType StopType;
        public int StopWordN;
        public bool UseNgramPhraseDetection;
        public int NGramNum;
        public AnalyserEngineSettings(
            StopWordDetectionType type = LOG_MIDPOINT,
            int StopWordN = -1,
            bool UseNgram = true,
            int NGramNum = 3
            )
        {
            this.StopType = type;  this.StopWordN = StopWordN;  this.UseNgramPhraseDetection = UseNgram;
            this.NGramNum = NGramNum;
        }
    }

    public class AnalyserEngine
    {
        AnalyserEngineSettings settings;
        private IGUIAdapter.Adapter gui;
        public Dictionary<int, ProcessingPipeline> IndexIDDict { get; } = new Dictionary<int, ProcessingPipeline>();
        private TwoDimensionalArray<int> _binaryTextIndex;
        public Dictionary<int, MovieIndex> UnProcessedIndexes { get; } = new Dictionary<int, MovieIndex>();
        // CorpusWordCount's tuple is First = wordcount, Second = index used in binaryIndex sorted alphabetically
        private Dictionary<string, Pair<int, int>> _corpusWordCount = new Dictionary<string, Pair<int, int>>();
        public Dictionary<string, Pair<int, int>> CorpusWordCount { get { return _corpusWordCount; } }
        private List<Pair<string, int>> _tdIDFList = new List<Pair<string, int>>();
        public AnalyserEngine(Dictionary<int, MovieIndex> unProcessedIndexes, 
            IGUIAdapter.Adapter gui,
            AnalyserEngineSettings settings)
        {
            this.gui = gui;
            this.UnProcessedIndexes = unProcessedIndexes;
            this.settings = settings;
            GeneratePipes();
            RemoveStopWords();
        }

        private void GeneratePipes()
        {
            foreach (var entry in UnProcessedIndexes)
            {
                IndexIDDict[entry.Key] = new ProcessingPipeline.Builder(entry.Value.GetFullText()).
                    SplitSentences().
                    SplitBulletPoints().
                    RemovePunctuation().
                    Normalize().
                    Tokenize().
                    Build();
                GenerateWordCounts(IndexIDDict[entry.Key]);
            }
        }

        private void GenerateWordCounts(ProcessingPipeline pipe)
        {
            foreach (var wordCountPair in pipe.TokenWordCount)
            {
                if (CorpusWordCount.ContainsKey(wordCountPair.Key))
                    CorpusWordCount[wordCountPair.Key].First += wordCountPair.Value;
                else
                {
                    CorpusWordCount[wordCountPair.Key] = new Pair<int, int>(0, 0);
                    CorpusWordCount[wordCountPair.Key].First = wordCountPair.Value;
                }
            }
        }

        /// <summary>
        /// Removes stop words, and calls CreateSortedPipeIndex to instantiate a list of sorted
        /// 
        /// 
        /// </summary>
        private void RemoveStopWords()
        {
            int index = 0;
            switch (settings.StopType)
            {
                case MEAN:
                    index = (int)SelectStopWordsStandardDev().Item1[1];
                    break;
                case LOG_MIDPOINT:
                    index = (int)SelectStopWordsLogMidPoint().Item1[1];
                    break;
                case INTER_QUARTILE:
                    index = (int)SelectStopWordsMedianIQRange().Item1[2];
                    break;
                case TOP_N:
                    index = Math.Max(settings.NGramNum, 0);
                    break;
            }
            CreateSortedPipeIndex(index);
        }

        private void CreateSortedPipeIndex(int index)
        {
            var list = GetZipfsBaseList();
            var removedWords = (from KeyValuePair<string, Pair<int, int>> pair in list.Take(index) select pair.Key).ToList();
            RemoveTermsFromCorpus(removedWords);
            SortCorpusAssignIndices();
            foreach (var idPipe in IndexIDDict)
            {
                idPipe.Value.RemoveTokens(removedWords);
                if (settings.NGramNum > 0)
                {
                    AddNgramsFromPipe(idPipe.Value);
                }
            }
        }

        private void PopulateBinaryIndex()
        {
            _binaryTextIndex = new TwoDimensionalArray<int>(CorpusWordCount.Count, IndexIDDict.Count);
            foreach(var pair in CorpusWordCount)
            {
                foreach(var pipe in IndexIDDict)
                {
                    if (pipe.Value.Tokens.Contains(pair.Key))
                    {
                        _binaryTextIndex.GetColumn(pair.Value.Second)[pipe.Key] = 1;
                    }
                    else
                    {
                        _binaryTextIndex[pair.Value.Second, pipe.Key] = 0;
                    }
                }
            }
        }

        private void SeletKeyWordsTFIDF()
        {
            foreach(var pair in CorpusWordCount)
            {
                int corpusTermFrequency = _binaryTextIndex.GetColumn(pair.Value.Second).Sum();
                float IDF = IndexIDDict.Count / (float)corpusTermFrequency;
            }
        }

        private void RemoveTermsFromCorpus(List<string> removeWords)
        {
            foreach (var removedWord in removeWords)
            {
                CorpusWordCount.Remove(removedWord);
            }
        }

        private void SortCorpusAssignIndices()
        {
            int i = 0;
            foreach (var pair in CorpusWordCount.OrderBy((a) => a.Key).Take(CorpusWordCount.Count))
            {
                CorpusWordCount[pair.Key].Second = i++;
            }
        }

        private void AddNgramsFromPipe(ProcessingPipeline pipe)
        {
            pipe.NGramNum = settings.NGramNum;
            pipe.MakeNGrams();
            _corpusWordCount = new Dictionary<string, Pair<int, int>>();
            GenerateWordCounts(pipe);
        }

        public Tuple<float[], float[], string> GetZipfsNormStats()
        {
            var CorpusWCList = GetZipfsBaseList();
            float[] xInt = new float[CorpusWCList.Count];
            float[] yInt = new float[CorpusWCList.Count];
            for (int i = 0; i < CorpusWCList.Count; i++)
            {
                xInt[i] = CorpusWCList[i].Value.First + 1;
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
                y[i] = (float)Math.Log(CorpusWCList[i].Value.First);
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
                new float[] { CorpusWCList[0].Value.First, 
                    CorpusWCList[rank].Value.First, 
                    CorpusWCList[logStats.Item1.Length - 1].Value.First },
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
                for (int j = 0; j < CorpusWCList[i].Value.First; ++j)
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
                new float[] { CorpusWCList[0].Value.First, 
                    CorpusWCList[Math.Max(meanIndex - 1, 1) - 1].Value.First, 
                    CorpusWCList[plus1deviation - 1].Value.First, 
                    CorpusWCList[CorpusWCList.Count - 1].Value.First },
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
                for (int j = 0; j < CorpusWCList[i].Value.First; ++j)
                    values.Add(i + 1);
            }
            float median = values[values.Count / 2];
            float q1 = values[values.Count / 4];
            float q3 = values[3 * values.Count / 4];
            var result =  new Tuple<float[], float[], string>(
                new float[] { 1, q1, median, q3, CorpusWCList.Count },
                new float[] { CorpusWCList[0].Value.First, 
                    CorpusWCList[(int)Math.Max(q1, 1) - 1].Value.First, 
                    CorpusWCList[(int)median - 1].Value.First, 
                    CorpusWCList[(int)q3].Value.First, 
                    CorpusWCList[CorpusWCList.Count - 1].Value.First },
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

        private List<KeyValuePair<string, Pair<int, int>>> GetZipfsBaseList()
        {
            var CorpusWCList = CorpusWordCount.ToList();
            // sorting is unneccesary, but for some reason this eats up ram without sorting for me
            CorpusWCList.Sort((first, second) => second.Value.First.CompareTo(first.Value.First));
            return CorpusWCList;
        }
    }
}
