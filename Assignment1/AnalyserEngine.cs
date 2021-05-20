using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using static Assignment1.StopWordDetectionType;
using System.Threading;
using Utils;
using System.Collections;

namespace Assignment1
{
    /// <summary>
    /// Used to differentiate different stop word types. This was going to be an option provided
    /// to user, but ran out of time
    /// </summary>
    public enum StopWordDetectionType
    {
        MEAN, LOG_MIDPOINT, INTER_QUARTILE, TOP_N
    }

    /// <summary>
    /// Settings class for varying options the engine could have used. Currently, it is only
    /// used with default settings. StopWordDetection type, whether to use phrases,  and
    /// the length of NGrams to use in phrases
    /// </summary>
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
        /// <summary>
        /// This is the main class responsible for the Corpus and all logic for processing instructions.
        /// This uses a BagOfWords object for tracking all words, frequency, document frequency etc.
        /// This also contained the raw pipelines at all stages.
        /// The list of stop words is tracked once that has been generated.
        /// </summary>
        public AnalyserEngineSettings settings;
        private IGUIAdapter.Adapter gui;
        private Dictionary<int, ProcessingPipeline> _indexIDDict;
        public Dictionary<int, ProcessingPipeline> IndexIDDict { get { return _indexIDDict; } }
        public Dictionary<int, MovieIndex> UnProcessedIndexes { get; } = new Dictionary<int, MovieIndex>();

        private BagOfWords _corpusBOW = new BagOfWords();
        public BagOfWords CorpusBOW { get { return _corpusBOW; } }
        private StopWordGenerator _stopWordGenerator;
        public StopWordGenerator StopWordGenerator { get { return _stopWordGenerator; } }
        public List<string> StopWords { get; set; } = new List<string>();

        public AnalyserEngine(Dictionary<int, MovieIndex> unProcessedIndexes, 
            IGUIAdapter.Adapter gui,
            AnalyserEngineSettings settings)
        {
            this.gui = gui;
            this.UnProcessedIndexes = unProcessedIndexes;
            this.settings = settings;
        }

        /// <summary>
        /// Used in Step 2 - Tokenization
        /// Main method for generating processed pipes from the unprocessed MovieIndexes
        /// generates a pipe with builder to process, adds pipe and id to IndexIDDict
        /// </summary>
        public void GenerateTokenizedPipes()
        {
            List<Thread> threads = new List<Thread>();
            _indexIDDict = new Dictionary<int, ProcessingPipeline>(UnProcessedIndexes.Count);
            object dictLock = new object();
            foreach (var entry in UnProcessedIndexes)
            {
                ThreadHelper.AddThread(() =>
                {
                    var pipeline = new ProcessingPipeline.Builder(entry.Value.GetFullText()).
                        SplitBulletPoints().
                        SplitSentences().
                        RemovePunctuation().
                        Normalize().
                        Tokenize().
                        Build();
                    lock (dictLock)
                    {
                        IndexIDDict[entry.Key] = pipeline;
                    }
                });
            }
            ThreadHelper.WaitThreads();
            foreach(var pip in IndexIDDict)
                CorpusBOW.AddTerms(pip.Value.Tokens);
            _stopWordGenerator = new StopWordGenerator(gui, CorpusBOW);
        }

        /// <summary>
        /// used in step 3 - stopword removal
        /// RemoveStopWords gets a generated stop word list and calls each pipe to remove the terms, and
        /// calls the CorpusBOW to remove the terms
        /// </summary>
        public void RemoveStopWords()
        {
            GenerateStopWords();
            foreach (var pipe in IndexIDDict)
            {
                ThreadHelper.AddThread(() => pipe.Value.RemoveTokens(this.StopWordGenerator.StopWords));
            }
            ThreadHelper.WaitThreads();
            CorpusBOW.RemoveTerms(this.StopWordGenerator.StopWords);
            StopWords = this.StopWordGenerator.StopWords;
        }

        /// <summary>
        /// Used in step 3 - keyword selection
        /// GenerateStopWords calls the StopWordGenerator method associated with the settings detection type
        /// to generate a list of stop words. 
        /// The main class to generate the stopwords is StopWordGenerator
        /// </summary>
        /// <returns>List of the stop words generated</returns>
        private void GenerateStopWords()
        {
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
            }
        }


        /// <summary>
        /// step 3 - keyword selection
        /// Removes very infrequently occuring words from corpus and pipes, ignoring infrequent words
        /// that occure in important fields
        /// </summary>
        public void RemoveVeryInfrequentWords()
        {
            var infrequentTerms = GetInfrequentWorlist();
            CorpusBOW.RemoveTerms(infrequentTerms);
            foreach (var pipeID in IndexIDDict)
            {
                ThreadHelper.AddThread(() => pipeID.Value.RemoveTokensBOW(_corpusBOW));
            }
            ThreadHelper.WaitThreads();
        }

        /// <summary>
        /// used in step 3
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
            HashSet<string> terms = new HashSet<string>();
            Dictionary<int, ProcessingPipeline> tempImportantPipes = 
                new Dictionary<int, ProcessingPipeline>(UnProcessedIndexes.Count);
            object dictLock = new object();
            // removes any words from infrequent terms that appear in important fields such as cast/title
            // in any pipes
            foreach (var pipeIdPair in IndexIDDict)
            {
                ThreadHelper.AddThread(() =>
                {
                    string importantTerms = "";
                    importantTerms += UnProcessedIndexes[pipeIdPair.Key].Director + " ";
                    importantTerms += UnProcessedIndexes[pipeIdPair.Key].Genre + " ";
                    importantTerms += UnProcessedIndexes[pipeIdPair.Key].Origin + " ";
                    importantTerms += UnProcessedIndexes[pipeIdPair.Key].Title + " ";
                    importantTerms += UnProcessedIndexes[pipeIdPair.Key].Cast + " ";
                    var newpipe = new ProcessingPipeline.Builder(importantTerms).
                        RemovePunctuation().
                        Normalize().
                        Tokenize().
                        Build();
                    lock (dictLock)
                    {
                        tempImportantPipes[pipeIdPair.Key] = newpipe;
                    }
                });
            }
            ThreadHelper.WaitThreads();
            foreach (var pipe in tempImportantPipes)
                terms.UnionWith(pipe.Value.Tokens);
            infrequentTerms = infrequentTerms.Except(terms).ToList();
            return infrequentTerms;
        }

        /// <summary>
        /// used in step 3
        /// Adds NGrams from pipes of word length 1-settings.NGramNum for all pipes
        /// </summary>
        public void GeneratePhrases()
        {
            _corpusBOW = new BagOfWords();
            foreach (KeyValuePair<int, ProcessingPipeline> idPipePair in IndexIDDict)
            {
                ThreadHelper.AddThread(() =>
                {
                    idPipePair.Value.NGramNum = settings.NGramNum;
                    idPipePair.Value.MakeNGrams();
                });
            }
            ThreadHelper.WaitThreads();
            foreach (KeyValuePair<int, ProcessingPipeline> idPipePair in IndexIDDict)
            {
                CorpusBOW.AddTerms(idPipePair.Value.Tokens);
            }
        }

        /// <summary>
        /// used in step 3
        /// Assigns each word in corpus's bow their associated IDF value
        /// </summary>
        public void CalculateIDFs()
        {
            foreach (var term in CorpusBOW.Terms)
            {
                double IDF = Math.Log(IndexIDDict.Count / (float)CorpusBOW.Terms[term.Key].DocFreq);
                CorpusBOW.Terms[term.Key].IDF = IDF;
            }
            CorpusBOW.IDFed = true;
        }

        /// <summary>
        /// used in step 3
        /// Calls each pipe to place tokens into their _keywords list
        /// </summary>
        public void AddKeywordsToPipes()
        {
            foreach(KeyValuePair<int, ProcessingPipeline> idPipePair in IndexIDDict)
            {
                ThreadHelper.AddThread(() => idPipePair.Value.AddTokensToKeywords());
            }
            ThreadHelper.WaitThreads();
        }

        /// <summary>
        /// used in step 4 - Keyword generation
        /// </summary>
        public void GenerateKeywordStems()
        {
            _corpusBOW = new BagOfWords();
            foreach (KeyValuePair<int, ProcessingPipeline> idPipePair in IndexIDDict)
            {
                ThreadHelper.AddThread(() => idPipePair.Value.GetStemmedKeywords());
            }
            ThreadHelper.WaitThreads();
            foreach (KeyValuePair<int, ProcessingPipeline> idPipePair in IndexIDDict)
            {
                _corpusBOW.AddTerms(idPipePair.Value.Tokens);
            }
            CalculateIDFs();
        }

        /// <summary>
        /// used in step 5
        /// Calculates the cosine similarity of two input documents, doc1 generally being a query,
        /// based upon the current corpus
        /// </summary>
        /// <param name="doc1">first document to be compared</param>
        /// <param name="doc2">second document to be compared against first</param>
        /// <returns>double, cosine similarity, or NaN if zero matches</returns>
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
