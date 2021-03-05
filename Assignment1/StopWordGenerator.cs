using System;
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

        /// <summary>
        /// Creates a dataset in Tuple form which is printable by the SimpleWPFChart project
        /// for normal zipf frequency/ranked values
        /// </summary>
        /// <returns>Tuple containing y, x axis values and title of chart</returns>
        public Tuple<float[], float[], string> GetZipfsNormStats()
        {
            var CorpusWCList = GetZipfsRankedList();
            float[] xInt = new float[CorpusWCList.Count];
            float[] yInt = new float[CorpusWCList.Count];
            for (int i = 0; i < CorpusWCList.Count; i++)
            {
                xInt[i] = CorpusWCList[i].Value.TermFreq + 1;
                yInt[i] = i + 1;
            }
            return new Tuple<float[], float[], string>(xInt, yInt, "Zipfs Normal");
        }

        /// <summary>
        /// iterates through a collextion of stop words and outputs them to the gui
        /// </summary>
        /// <param name="index">Where in the total collection of zipf ranked words selection has been made</param>
        /// <param name="rankedList">The total zipf ranked collection of all words</param>
        private void OutputStopwordsSelected(int index, List<KeyValuePair<string, WordStats>> rankedList)
        {
            _stopWords = (from KeyValuePair<string, WordStats> pair in rankedList.Take(index) select pair.Key).ToList();
            string stopwords = "";
            for (int i = 0; i < _stopWords.Count; i++)
            {
                stopwords += $"Stopword {i+=1} = {_stopWords[i]}\n";
            }
            gui.AddConsoleMessage("Stopwords Generated\n" + stopwords);
        }

        /// <summary>
        /// Outputs a smallword preview of a methods selected stop words
        /// </summary>
        /// <param name="message">The type of method used for selection</param>
        /// <param name="startIndex">The first word to appear from the selection</param>
        /// <param name="endIndex">The last word to appear in the selection</param>
        /// <param name="match">The index of the selected word</param>
        private void PrintStatsToGui(string message, int startIndex, int endIndex, int match)
        {
            var CorpusWCList = GetZipfsRankedList();
            message = "StopWord selection stats:-\n" + message;
            message += "\n";
            for (int i = startIndex; i <= endIndex; ++i)
            {
                message += $"Rank: {i} Frequency: {CorpusWCList[i].Value.TermFreq} String: {CorpusWCList[i].Key}\n";
                if (i == match)
                {
                    message = message.Substring(0, message.Length - 1);
                    message += " - ***Selected Match***\n";
                }
            };
            message = message.Substring(0, message.Length - 1);
            gui.AddConsoleMessage(message);
        }

        /// <summary>
        /// Main logic for generating the zipf log mid point, and returns data that can be output
        /// as a graph in SimpleWPFChart project
        /// </summary>
        /// <returns>Data to be output to graph with x/y axis values and title, and used for selection</returns>
        public Tuple<float[], float[], string> SelectStopWordsLogMidPoint()
        {
            var CorpusWCList = GetZipfsRankedList();
            var logStats = GetZipfsLogStats();
            var maxRank = logStats.Item1[logStats.Item1.Length - 1];
            int rank = (int)Math.Pow(maxRank / 2, Math.E);
            int startIndexPrint = Math.Max(rank - 10, 0);
            var result = new Tuple<float[], float[], string>(
                new float[] { 0, rank, logStats.Item1.Length },
                new float[] { CorpusWCList[0].Value.TermFreq,
                    CorpusWCList[rank].Value.TermFreq,
                    CorpusWCList[logStats.Item1.Length - 1].Value.TermFreq },
                "Selection Log Mid Point"
                );
            PrintStatsToGui("Zipf log mid point selection", startIndexPrint, rank + 10, rank);
            OutputStopwordsSelected(rank, CorpusWCList);
            return result;
        }

        /// <summary>
        /// Main logic for generating the zipf log stats graph, and returns data that can be output
        /// as a graph in SimpleWPFChart project
        /// </summary>
        /// <returns>Data to be output to graph with x/y axis values and title, and used for selection</returns>
        public Tuple<float[], float[], string> GetZipfsLogStats()
        {
            var CorpusWCList = GetZipfsRankedList();
            float[] x = new float[CorpusWCList.Count];
            float[] y = new float[CorpusWCList.Count];
            for (int i = 0; i < CorpusWCList.Count; i++)
            {
                y[i] = (float)Math.Log(CorpusWCList[i].Value.TermFreq);
                x[i] = (float)Math.Log(i + 1);
            }
            return new Tuple<float[], float[], string>(x, y, "Xipfs Natural Log");
        }

        /// <summary>
        /// Main logic for generating the mean and standard deviation, and returns data that can be output
        /// as a graph in SimpleWPFChart project
        /// </summary>
        /// <returns>Data to be output to graph with x/y axis values and title, and used for selection</returns>
        public Tuple<float[], float[], string> SelectStopWordsStandardDev()
        {
            var CorpusWCList = GetZipfsRankedList();
            List<int> values = new List<int>();
            for (int i = 0; i < CorpusWCList.Count; i++)
            {
                for (int j = 0; j < CorpusWCList[i].Value.TermFreq; ++j)
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
                new float[] { CorpusWCList[0].Value.TermFreq,
                    CorpusWCList[Math.Max(meanIndex - 1, 1) - 1].Value.TermFreq,
                    CorpusWCList[plus1deviation - 1].Value.TermFreq,
                    CorpusWCList[CorpusWCList.Count - 1].Value.TermFreq },
                "Selection Standard Deviation"
                );
            var startIndex = Math.Max(meanIndex - 10, 0);
            PrintStatsToGui("Selection Standard Deviation", startIndex, meanIndex + 10, meanIndex);
            OutputStopwordsSelected(meanIndex, CorpusWCList);
            return result;
        }

        /// <summary>
        /// Main logic for generating the IQ range and median selection, and returns data that can be output
        /// as a graph in SimpleWPFChart project
        /// </summary>
        /// <returns>Data to be output to graph with x/y axis values and title, and used for selection</returns>
        public Tuple<float[], float[], string> SelectStopWordsMedianIQRange()
        {
            var CorpusWCList = GetZipfsRankedList();
            List<int> values = new List<int>();
            for (int i = 0; i < CorpusWCList.Count; i++)
            {
                for (int j = 0; j < CorpusWCList[i].Value.TermFreq; ++j)
                    values.Add(i + 1);
            }
            float median = values[values.Count / 2];
            float q1 = values[values.Count / 4];
            float q3 = values[3 * values.Count / 4];
            var result = new Tuple<float[], float[], string>(
                new float[] { 1, q1, median, q3, CorpusWCList.Count },
                new float[] { CorpusWCList[0].Value.TermFreq,
                    CorpusWCList[(int)Math.Max(q1, 1) - 1].Value.TermFreq,
                    CorpusWCList[(int)median - 1].Value.TermFreq,
                    CorpusWCList[(int)q3].Value.TermFreq,
                    CorpusWCList[CorpusWCList.Count - 1].Value.TermFreq },
                "Selection IQ Range"
                );
            var startIndex = Math.Max((int)q1 - 10, 0);
            PrintStatsToGui("Selection IQ Range Q1", startIndex, (int)q1 + 10, (int)q1);
            startIndex = Math.Max((int)median - 10, 0);
            PrintStatsToGui("Selection IQ Range Median", startIndex, (int)median + 10, (int)median);
            startIndex = Math.Max((int)q3 - 10, 0);
            PrintStatsToGui("Selection IQ Range Q3", startIndex, (int)q3 + 10, (int)q3);
            OutputStopwordsSelected((int)median, CorpusWCList);
            return result;
        }

        /// <summary>
        /// Generates the frequency ranked zipf term list from the Corpus'es BagOfWords
        /// </summary>
        /// <returns>Zipf ranked word list with element 0 being highest frequency word</returns>
        private List<KeyValuePair<string, WordStats>> GetZipfsRankedList()
        {
            var CorpusWCList = CorpusBOW.Terms.ToList();
            // sorts by total frequency, i.e a ranked sorted list with 0 being most frequent
            CorpusWCList.Sort((first, second) => second.Value.TermFreq.CompareTo(first.Value.TermFreq));
            return CorpusWCList;
        }
    }
}
