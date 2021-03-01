using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using static Assignment1.StopWordDetectionType;
using Utils;
using System.Collections;

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
        public Dictionary<int, MovieIndex> UnProcessedIndexes { get; } = new Dictionary<int, MovieIndex>();

        private BagOfWords _corpusBOW = new BagOfWords();
        public BagOfWords CorpusBOW { get { return _corpusBOW; } }
        private StopWordGenerator _stopWordGenerator;
        public StopWordGenerator StopWordGenerator { get { return _stopWordGenerator; } }

        public AnalyserEngine(Dictionary<int, MovieIndex> unProcessedIndexes, 
            IGUIAdapter.Adapter gui,
            AnalyserEngineSettings settings)
        {
            this.gui = gui;
            this.UnProcessedIndexes = unProcessedIndexes;
            this.settings = settings;
        }

        public void GenerateTokenizatedPipes()
        {
            foreach (var entry in UnProcessedIndexes)
            {
                var pipeline = new ProcessingPipeline.Builder(entry.Value.GetFullText()).
                    SplitBulletPoints().
                    SplitSentences().
                    RemovePunctuation().
                    Normalize().
                    Tokenize().
                    Build();
                IndexIDDict[entry.Key] = pipeline;
                CorpusBOW.AddTerms(pipeline.Tokens);
            }
            CorpusBOW.AddNormalizedTermFreq();
            CorpusBOW.IndexWords();
        }

        /// <summary>
        /// RemoveStopWords gets a generated stop word list and calls each pipe to remove the term and
        /// calls the CorpusBOW to remove the term
        /// </summary>
        private void RemoveStopWords()
        {
            var stopWords = GenerateStopWords();
            foreach (var pipe in IndexIDDict)
            {
                pipe.Value.RemoveTokens(stopWords);
            }
            CorpusBOW.RemoveTerms(stopWords);
            CorpusBOW.AddNormalizedTermFreq();
            CorpusBOW.IndexWords();
        }

        /// <summary>
        /// GenerateStopWords calls the StopWordGenerator method associated with the settings detection type
        /// to generate a list of stop words
        /// </summary>
        /// <returns>List of the stop words generated</returns>
        private List<string> GenerateStopWords()
        {
            _stopWordGenerator = new StopWordGenerator(gui, CorpusBOW);
            List<string> stopWords;
            switch (settings.StopType)
            {
                case MEAN:
                    this.StopWordGenerator.SelectStopWordsStandardDev();
                    break;
                case LOG_MIDPOINT:
                    this.StopWordGenerator.SelectStopWordsLogMidPoint();
                    break;
                case INTER_QUARTILE:
                    this.StopWordGenerator.SelectStopWordsMedianIQRange();
                    break;
                case TOP_N:
                    this.StopWordGenerator.SelectStopWordsN(Math.Max(settings.StopWordN, 0));
                    break;
            }
            stopWords = this.StopWordGenerator.StopWords;
            return stopWords;
        }

        public void GenerateStems()
        {
            _corpusBOW = new BagOfWords();
            foreach (KeyValuePair<int, ProcessingPipeline> idPipePair in IndexIDDict)
            {
                idPipePair.Value.Stem();
                CorpusBOW.AddTerms(idPipePair.Value.Tokens);
            }
            CorpusBOW.AddNormalizedTermFreq();
            CorpusBOW.IndexWords();
        }

        public void GeneratePhrases()
        {
            _corpusBOW = new BagOfWords();
            foreach (KeyValuePair<int, ProcessingPipeline> idPipePair in IndexIDDict)
            {
                idPipePair.Value.NGramNum = settings.NGramNum;
                idPipePair.Value.MakeNGrams();
                CorpusBOW.AddTerms(idPipePair.Value.Tokens);
            }
            CorpusBOW.AddNormalizedTermFreq();
            CorpusBOW.IndexWords();
        }

        /// <summary>
        /// 
        /// </summary>
        /// Calculations for IDF have been taken from CE306 combined with knowledge from:
        /// https://janav.wordpress.com/2013/10/27/tf-idf-and-cosine-similarity/ 
        /// for 1 +, such that the IDF will never take fractional values below 1
        public void CalculateIDFs()
        {
            foreach (var term in CorpusBOW.Terms)
            {
                double IDF = 1 + Math.Log(IndexIDDict.Count / (float)CorpusBOW.Terms[term.Key].DocFreq);
                CorpusBOW.Terms[term.Key].IDF = IDF;
            }
            CorpusBOW.IDFed = true;
        }

        public double CosineSimilarity(BagOfWords doc1, BagOfWords doc2)
        {
            // nominator calcs
            double[] doc1TFIDFVector = CorpusBOW.GetDocNormTFTimesIDFVector(doc1);
            double[] doc2TFIDFVector = CorpusBOW.GetDocNormTFTimesIDFVector(doc2);
            double innerProduct = VectorOps.Multiplication(doc1TFIDFVector, doc2TFIDFVector).Sum();
            //denominator calcs
            double doc1Sqrt = Math.Sqrt(VectorOps.Multiplication(doc1TFIDFVector, doc1TFIDFVector).Sum());
            double doc2Sqrt = Math.Sqrt(VectorOps.Multiplication(doc2TFIDFVector, doc2TFIDFVector).Sum());
            double denominator = doc1Sqrt * doc2Sqrt;
            return innerProduct / denominator;
        }
    }
}
