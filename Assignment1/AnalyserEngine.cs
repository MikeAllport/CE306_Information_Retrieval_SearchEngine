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
        public double topNPercentKeywords;
        public AnalyserEngineSettings(
            StopWordDetectionType type = LOG_MIDPOINT,
            int StopWordN = -1,
            bool UseNgram = true,
            int NGramNum = 3,
            double topNPercentKeywords = 0.8
            )
        {
            this.StopType = type;  this.StopWordN = StopWordN;  this.UseNgramPhraseDetection = UseNgram;
            this.NGramNum = NGramNum; this.topNPercentKeywords = topNPercentKeywords;
        }
    }

    public class AnalyserEngine
    {
        public AnalyserEngineSettings settings;
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

        /// <summary>
        /// Main method for generating processed pipes from the unprocessed MovieIndexes
        /// generates a pipe with builder to process, adds pipe and id to IndexIDDict
        /// </summary>
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
        }

        /// <summary>
        /// RemoveStopWords gets a generated stop word list and calls each pipe to remove the terms and
        /// calls the CorpusBOW to remove the terms
        /// </summary>
        public void RemoveStopWords()
        {
            var stopWords = GenerateStopWords();
            foreach (var pipe in IndexIDDict)
            {
                pipe.Value.RemoveTokens(stopWords);
            }
            CorpusBOW.RemoveTerms(stopWords);
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


        /// <summary>
        /// Removes very infrequently occuring words from corpus and pipes, ignoring infrequent words
        /// that occure in important fields
        /// </summary>
        public void RemoveVeryInfrequentWords()
        {
            var infrequentTerms = GetInfrequentWorlist();
            CorpusBOW.RemoveTerms(infrequentTerms);
            foreach (var pipeID in IndexIDDict)
            {
                pipeID.Value.RemoveTokens(infrequentTerms);
            }
        }

        /// <summary>
        /// Attains list of words that appear very infrequently, 1 occurence in this occasion.
        /// This ignores any words that appear infrequently but occur in important fields. To achieve
        /// this a second pass through is made over all documents to retrieve important fields tokens
        /// which may not be efficient
        /// </summary>
        /// <returns>List of infrequently occuring words</returns>
        private List<string> GetInfrequentWorlist()
        {
            // generates list of all words that appear only once in corpus
            var infrequentTerms = CorpusBOW.Terms.
                Select(termStats => termStats).
                Where(termStats => termStats.Value.TermFreq < 2).
                Select(termStats => termStats.Key).
                ToList();
            // removes any words from infrequent terms that appear in important fields such as cast/title
            // in any pipes
            foreach (var pipeIdPair in IndexIDDict)
            {
                string importantTerms = "";
                importantTerms += UnProcessedIndexes[pipeIdPair.Key].Director + " ";
                importantTerms += UnProcessedIndexes[pipeIdPair.Key].Genre  + " ";
                importantTerms += UnProcessedIndexes[pipeIdPair.Key].Origin + " ";
                importantTerms += UnProcessedIndexes[pipeIdPair.Key].Title  + " ";
                importantTerms += UnProcessedIndexes[pipeIdPair.Key].Cast   + " ";
                var newpipe = new ProcessingPipeline.Builder(importantTerms).
                    RemovePunctuation().
                    Normalize().
                    Tokenize().
                    Build();
                infrequentTerms = infrequentTerms.Except(newpipe.Tokens).ToList();
            }
            return infrequentTerms;
        }

        /// <summary>
        /// Adds NGrams from pipes of word length 1-settings.NGramNum for all pipes
        /// </summary>
        public void GeneratePhrases()
        {
            _corpusBOW = new BagOfWords();
            foreach (KeyValuePair<int, ProcessingPipeline> idPipePair in IndexIDDict)
            {
                idPipePair.Value.NGramNum = settings.NGramNum;
                idPipePair.Value.MakeNGrams();
                CorpusBOW.AddTerms(idPipePair.Value.Tokens);
            }
        }

        /// <summary>
        /// Calls each pipe to place tokens into their _keywords list
        /// </summary>
        public void AddKeywordsToPipes()
        {
            foreach(KeyValuePair<int, ProcessingPipeline> idPipePair in IndexIDDict)
            {
                idPipePair.Value.AddTokensToKeywords();
            }
        }

        public void GenerateKeywordStems()
        {
            foreach (KeyValuePair<int, ProcessingPipeline> idPipePair in IndexIDDict)
            {
                idPipePair.Value.StemKeywords();
            }
        }


        /// <summary>
        /// Assigns each word in corpus's bow their associated IDF value
        /// Knowledge for calculating IDF gained from lectures, but combined with:
        /// https://janav.wordpress.com/2013/10/27/tf-idf-and-cosine-similarity/
        /// This introduces normalization such that all TFIDFs will fall between 1 and 0, and as
        /// such multiplying a fraction by a fraction would lead to incorrect results so will always
        /// need to be 1+
        /// </summary>
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
            if (!CorpusBOW.IDFed)
                CalculateIDFs();
            // nominator calcs
            double[] doc1TFIDFVector = CorpusBOW.GetDocNormTFIDFVector(doc1);
            double[] doc2TFIDFVector = CorpusBOW.GetDocNormTFIDFVector(doc2);
            double innerProduct = VectorOps.Multiplication(doc1TFIDFVector, doc2TFIDFVector).Sum();
            //denominator calcs
            double doc1Sqrt = Math.Sqrt(VectorOps.Multiplication(doc1TFIDFVector, doc1TFIDFVector).Sum());
            double doc2Sqrt = Math.Sqrt(VectorOps.Multiplication(doc2TFIDFVector, doc2TFIDFVector).Sum());
            double denominator = doc1Sqrt * doc2Sqrt;
            return innerProduct / denominator;
        }
    }
}
