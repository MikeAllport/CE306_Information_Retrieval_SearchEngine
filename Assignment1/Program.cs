using System;
using System.IO;
using Elasticsearch;
using Nest;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Assignment1
{
    public class Program
    {
        /// <summary>
        /// Program contains the main logic for the assignment's step sequence. Most processing operations
        /// are done through the AnalyzerEngine class and ProcessingPipeline. All database operations are done through the ElasticService class,
        /// but this is generalized and interfaces with MovieIndexService class and sub class (at the bottom of the movieindexservice class).
        /// 
        /// This has been a really fun assignment. Although I have no idea if this is how you want it orchestrated.
        /// ElasticSearch is not used in depth. It is mainly a data store. Zips is mainly used for stop word removal, 
        /// a custom BagOfWords representation created, IDF feature vectors, and CosineSimilarity for ranking matches. 
        /// No external libraries have been used for processing in the final version, all regex expressions. 
        /// 
        /// Have fun marking! If you break it, please let me know where >.<
        /// </summary>
        public static readonly string SOLUTION_DIR = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\..\\"));
        public static readonly string DEFAULT_DATA_FILE = SOLUTION_DIR + "documents.csv";
        public const int _numDocuments = 10000;
        private AnalyserEngine engine;
        public AnalyserEngine AnalyserEngine { get { return engine; } }
        private IGUIAdapter.Adapter gui;
        public static bool _running = true;

        public Program(IGUIAdapter.Adapter gui)
        {
            this.gui = gui;
        }

        /// <summary>
        /// helper function to call full indexing with default parameters
        /// this must be done instead of using default assignment else GUI cannot call with default assignment
        /// </summary>
        /// <param name="uri"></param>
        public bool PerformFullIndexing(string uri)
        {
            return PerformFullIndexing(uri, _numDocuments);
        }

        /// <summary>
        /// Assignment Step 1 - Full Indexing
        /// This function initializes all documents. CSV file are mainly parsed in DataMunger through use of CSVParser
        /// which instantiates MovieIndex classes  having realised the ICSVEntity interface. All movie indexes 
        /// are created with their full fields having been parsed.
        /// 
        /// This and all proceeding steps follow a general sequence of operations:
        /// 
        ///     Processing performed on documents
        ///     MovieIndexService created, has methods needed for indexing
        ///     ElasticService created, creates and commincates with the database interfacing with MIService
        ///     Processed documents attached to MovieIndexService, which uploads data via ElasticService
        ///     Top6 documents obtained following processing are Output to GUI
        ///     
        /// The only difference being with searching, which does not perform any processing on the Corpus,
        /// only the query and matches.
        /// </summary>
        /// <param name="uri">The full absolute path to csv file, must be a MovieIndex csv as per assignment</param>
        public bool PerformFullIndexing(string uri, int numDocs)
        {
            try
            {
                // parses csv files
                DataMunger munger = new DataMunger(uri);
                List<MovieIndex> movieIndexList = munger.GetMovieIndexes();
                movieIndexList.Sort();

                // creates MI service and attaches documents
                var _miService = new MovieIndexService<MovieIndex>();
                _miService.NumDocuments = numDocs;
                _miService.AddDocuments(movieIndexList);

                // creates DB and uploads documents
                var _esService = new ElasticService<MovieIndexService<MovieIndex>, MovieIndex>(_miService);
                _esService.InitDB();
                _miService.UploadData(_esService);
                _esService.AwaitASync();

                // outputs database statistics and 1 result to gui
                string result = _esService.DescribeIndices();
                result = "Created Index: " + _miService.GetIndexTitle() + "\n> ElasticSearch list:\n" + result;
                result += "\n> First result: \n";
                foreach (var document in _esService.GetFullMatches(1))
                {
                    result += document.Serialize() + "\n";
                }
                gui?.AddConsoleMessage(result);

                // instantiates the analyzer engine used in proceeding steps
                engine = new AnalyserEngine(_miService.MovieIndexMap, gui, new AnalyserEngineSettings());
                return true;
            }
            catch (Exception e)
            {
                OutputMessageGUI(e);
                return false;
            }
        }

        private void OutputMessageGUI(Exception e)
        {
            gui?.AddConsoleMessage(e.Message, IGUIAdapter.GUIColor.ERROR_COLOR);
            gui?.AddConsoleMessage(e.StackTrace, IGUIAdapter.GUIColor.ERROR_COLOR);
        }

        /// <summary>
        /// Assignment Step 2 - Tokenization
        /// This function calls engines GenerateTokenizedPipes to process and tokenize the documents.
        /// See engines method for implementation, however the ProcessingPipeline is the main class
        /// responsible for the processing steps.
        /// 
        /// As with steps 1-4, data services are created, and data is uploaded via associated 
        /// MovieIndexService and ElastiService's. Documents id'd 1-6 are output to gui
        /// 
        /// Steps 2-4 make use of helper function CreateMovieIndexChildClasses which transforms
        /// existing MovieIndex objects in the AnalyzerEngine into MovieIndex sub classes such as 
        /// MovieIndexTokenized or MovieIndexKeyWorded etc
        /// </summary>
        public bool PerformTokenization()
        {
            try
            {
                // processes documents
                engine.GenerateTokenizedPipes();

                // creates MI service and attaches documents via CreateMIInstancesAndAttachToMIService
                var miService = new MovieIndexServiceProcessed<MovieIndexTokenized>();
                var processedDocs = SortedIndexDictToList(CreateMIInstancesAndAttachToMIService(miService));

                // creates ES connection and uploads data
                var elasticService = new ElasticService<MovieIndexService<MovieIndexTokenized>, MovieIndexTokenized>(miService);
                elasticService.InitDB();
                miService.UploadData(elasticService);

                // outputs first 6 documents to gui
                var top6docs = processedDocs.Take(6).ToList();
                elasticService.AwaitASync();
                string result = $"Created Tokenized Processed Index: {miService.GetIndexTitle()}\n> 6 results: \n";
                foreach (var document in top6docs)
                {
                    result += document.Serialize() + "\n";
                }
                gui?.AddConsoleMessage(result);
                return true;
            }
            catch (Exception e)
            {
                OutputMessageGUI(e);
                return false;
            }
        }

        /// <summary>
        /// Assignment Step 3 - Keyword Selection
        /// This function performs selection by calling analyser engine for stop word removal using zipf analysis, 
        /// phrase generation, removal of very infrequent words, IDF weighting, 
        /// and keyword selection. 
        /// 
        /// Processed documents are then uploaded via data services and id's 1-6 are output
        /// to the gui. See engine methods, the first 5 operations, for main implementations
        /// </summary>
        public bool PerformKeywordSelection()
        {
            try
            {
                // processes documents for this steps
                engine.RemoveStopWords();
                engine.GeneratePhrases();
                engine.RemoveVeryInfrequentWords();
                engine.CalculateIDFs();
                engine.AddKeywordsToPipes();

                // create MI service and attaches documents
                var miService = new MovieIndexServiceProcessed<MovieIndexKeyWords>();
                var indexedDocs = SortedIndexDictToList(CreateMIInstancesAndAttachToMIService(miService));
                
                // creates database and uploads documents
                var elasticService = new ElasticService<MovieIndexService<MovieIndexKeyWords>, MovieIndexKeyWords>(miService);
                elasticService.InitDB();
                miService.UploadData(elasticService);

                // outputs first 6 documents to gui
                var top6docs = indexedDocs.Take(6).ToList();
                elasticService.AwaitASync();
                string result = $"Created Keyworded Processed Index: {miService.GetIndexTitle()}\n> 6 results: \n";
                foreach (var document in top6docs)
                {
                    result += document.Serialize() + "\n";
                }
                gui?.AddConsoleMessage(result);
                return true;
            }
            catch (Exception e)
            {
                OutputMessageGUI(e);
                return false;
            }

        }

        /// <summary>
        /// Assignment Step 4 - Stemnming
        /// Firstly a dict of MovieIndexKeyWordStemmed is created with helper method, these store
        /// unstemmed keywords. Then the documents are processed in the engine, and stemmed keywords are
        /// added to all documents in the dict. Stemmed documents are uploaded to elasticsearch and then
        /// top 6 output to gui
        /// </summary>
        public bool PerformStemming()
        {
            try
            {
                // instantiates data services
                var tokenizedService = new MovieIndexServiceProcessed<MovieIndexKeyWordStemmed>();
                var elasticService = new ElasticService<MovieIndexService<MovieIndexKeyWordStemmed>, MovieIndexKeyWordStemmed>(tokenizedService);
                elasticService.InitDB();

                // creates indexes with original non stemmed keywords
                SortedDictionary<int, MovieIndexKeyWordStemmed> results = CreateMIInstancesAndAttachToMIService(tokenizedService);

                // processed documents with stems
                engine.GenerateKeywordStems();
                // adds new stemmed keywords to index
                foreach (var indexIDPipePair in engine.IndexIDDict)
                {
                    results[indexIDPipePair.Key].AddKeywordStemmedFromPipe(indexIDPipePair.Value);
                }
                var top6docs = (from KeyValuePair<int, MovieIndexKeyWordStemmed> idIndexPair in results select idIndexPair.Value).Take(6).ToList();

                // uploads documents to database
                tokenizedService.UploadData(elasticService);
                elasticService.AwaitASync();

                // outputs first 6 documents to gui
                string result = $"Created Stemmed Processed Index: {tokenizedService.GetIndexTitle()}\n> 6 results: \n";
                foreach (var document in top6docs)
                {
                    result += document.Serialize() + "\n";
                }
                gui?.AddConsoleMessage(result);
                return true;
            }
            catch (Exception e)
            {
                OutputMessageGUI(e);
                return false;
            }
        }

        /// <summary>
        /// Assignment Step 5 - Searching
        /// Firstly, two the query is processed twice using ProcessingPipelines on the query.
        /// 1 is needed pre keywording for cosine similarity comparison and 1 needed post
        /// to obtain results from.
        /// 
        /// Data services are then instantiated and the query made.
        /// 
        /// For each match, the cosine similarity is obtained between the query and the match using
        /// AnalyzerEngine methods. If FieldType matching is required, helper functions are used to see
        /// if the match contains any results in the given field. If not, they are just added as normal
        /// 
        /// The matches are then sorted and id'd in descending order of relevance.
        /// 
        /// Matches are then returned for the gui to output to console
        /// 
        /// Even if field match query given, this still returns results where the query has matched anywhere
        /// else in the body, just output to the bottom of the matches and has field 'FieldMatched=false' set
        /// The field match is an OR search, so any split strings in the query can be matched to the field. However,
        /// matches with more words in the field are given greater relevance thanks to cosine similarity :)
        /// </summary>
        /// <param name="searchString">The query string</param>
        /// <param name="fieldType">What field to match, if NONE then matching any field</param>
        /// <returns></returns>
        public MovieIndexMatches PerformSearch(string searchString, FieldName fieldType = FieldName.NONE)
        {
            try
            {
                // create pipes of query
                var queryPipe = new ProcessingPipeline.Builder(searchString).
                        SplitBulletPoints().
                        SplitSentences().
                        RemovePunctuation().
                        Normalize().
                        Tokenize().
                        Build();
                queryPipe.NGramNum = 3;
                queryPipe.MakeNGrams();
                queryPipe.AddTokensToKeywords();
                queryPipe.GetStemmedKeywords();

                // instantiates data services
                var tokenizedService = new MovieIndexServiceProcessed<MovieIndexKeyWords>();
                var elasticService = new ElasticService<MovieIndexService<MovieIndexKeyWords>, MovieIndexKeyWords>(tokenizedService);

                //makes query
                List<MovieIndexKeyWords> results = elasticService.KeywordQuery<MovieIndexKeyWords>(queryPipe.Keywords);
                MovieIndexMatches matchesObj = new MovieIndexMatches(results.Count, searchString);
                object matchesLock = new object();

                // processes queries
                foreach (MovieIndexKeyWords match in results)
                {
                    ThreadHelper.AddThread(() =>
                    {
                        // finds cosing similarity between query and match document
                        var matchPipe = new ProcessingPipeline.Builder(match.GetFullText())
                            .SplitBulletPoints()
                            .SplitSentences()
                            .RemovePunctuation()
                            .Normalize()
                            .Tokenize()
                            .Build();
                        matchPipe.NGramNum = 3;
                        matchPipe.MakeNGrams();
                        matchPipe.AddTokensToKeywords();
                        matchPipe.GetStemmedKeywords();

                        double queryDocSimilarity = engine.CosineSimilarity(queryPipe.TermsAndStats, matchPipe.TermsAndStats);
                        // processes match, checking if fields match if user selected a field in gui
                        MovieIndexQueryMatch processedMatch;
                        if (fieldType != FieldName.NONE)
                        {
                            bool fieldMatches = FieldMatches(fieldType, match, queryPipe.Tokens);
                            processedMatch = new MovieIndexQueryMatch(queryDocSimilarity, match, true, fieldMatches);
                        }
                        else
                        {
                            processedMatch = new MovieIndexQueryMatch(queryDocSimilarity, match);
                        }
                        lock (matchesLock)
                        {
                            matchesObj.Matches.Add(processedMatch);
                        }
                    });
                }
                ThreadHelper.WaitThreads();

                // assign match indexes and set count
                matchesObj.ResultsFound = matchesObj.Matches.Count;
                for (int i = 0; i < matchesObj.Matches.Count;)
                {
                    matchesObj.Matches.ElementAt(i).ID = ++i;
                }

                // return matches, leaving responsibility for gui to output results message such that it can clear screen
                return matchesObj;
            }
            catch (Exception e)
            {
                OutputMessageGUI(e);
                return null;
            }
        }

        /// <summary>
        /// Detects if any of the terms in the users query is in a given match's field of a given field
        /// name
        /// </summary>
        /// <param name="name">The field to check if query matches</param>
        /// <param name="match">The movie index to checl</param>
        /// <param name="queryRaw">The list of users queries, pre-processed</param>
        /// <returns>true if field contains one of users terms, false otherwise</returns>
        private bool FieldMatches(FieldName name, MovieIndex match, List<string> queryRaw)
        {
            string field;
            switch(name)
            {
                case FieldName.CAST:
                    field = match.Cast;
                    break;
                case FieldName.DIRECTOR:
                    field = match.Director;
                    break;
                case FieldName.GENRE:
                    field = match.Genre;
                    break;
                case FieldName.ORIGIN:
                    field = match.Origin;
                    break;
                case FieldName.PLOT:
                    field = match.Plot;
                    break;
                case FieldName.RELEASEYEAR:
                    field = match.ReleaseYear.ToString();
                    break;
                case FieldName.TITLE:
                    field = match.Title;
                    break;
                default: //(WIKI)
                    field = match.Wiki;
                    break;
            }
            ProcessingPipeline fieldPipe = new ProcessingPipeline.Builder(field)
                    .SplitBulletPoints()
                    .SplitSentences()
                    .RemovePunctuation()
                    .Normalize()
                    .Tokenize()
                    .Build();
            fieldPipe.AddTokensToKeywords();
            fieldPipe.GetStemmedKeywords();
            if (fieldPipe.Keywords.Intersect(queryRaw).Count() > 0)
                return true;
            return false;
        }

        /// <summary>
        /// Generates the zipf analysis bar charts for Zipf stopword removal in gui
        /// </summary>
        public void RunZipfsSelectionAnalysis()
        {
            gui?.SetChart(engine.StopWordGenerator?.GetZipfsNormStats());
            gui?.SetChart(engine.StopWordGenerator?.GetZipfsLogStats());
            gui?.SetChart(engine.StopWordGenerator?.SelectStopWordsStandardDev());
            gui?.SetChart(engine.StopWordGenerator?.SelectStopWordsMedianIQRange());
            gui?.SetChart(engine.StopWordGenerator?.SelectStopWordsLogMidPoint());
        }

        /// <summary>
        /// Creates an ID keyed dictionary of MovieIndex child objects of type T by getting
        /// all MovieIndex parents from the AnalyzerEngine, creating child object with the associated pipe
        /// for the child object to extract data from the pipes, see any child constructor for this implementation
        /// under the folder MovieIndexes
        /// </summary>
        /// <typeparam name="T">The type of MovieIndex child class to create</typeparam>
        /// <param name="miservice">The movie index service containing all documents of type T</param>
        /// <returns>A dictionary with ID keys and type T movie index derrived classes (the documents)</returns>
        public SortedDictionary<int, T> CreateMIInstancesAndAttachToMIService<T>(MovieIndexService<T> miservice) where T :
            MovieIndex
        {
            SortedDictionary<int, T> results = new SortedDictionary<int, T>();
            foreach (var indexIDPipePair in engine.IndexIDDict)
            {
                var document = (T)Activator.CreateInstance(typeof(T), engine.UnProcessedIndexes[indexIDPipePair.Key],
                    indexIDPipePair.Value);
                miservice.MovieIndexMap[indexIDPipePair.Key] = document;
                results[indexIDPipePair.Key] = document;
            }
            return results;
        }

        /// <summary>
        /// A helper function for converting movie indexes with ID keyed dictionaries to a list
        /// </summary>
        /// <typeparam name="T">The type of MovieIndex child classes</typeparam>
        /// <param name="dict">The initial dictionary containind ID/Index pairs</param>
        /// <returns></returns>
        public List<T> SortedIndexDictToList<T>(SortedDictionary<int, T> dict)
        {
            return (from KeyValuePair<int, T> idIndexPair in dict select idIndexPair.Value).ToList();
        }
    }
}
