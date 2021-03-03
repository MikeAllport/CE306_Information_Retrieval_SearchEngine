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
        public static readonly string SOLUTION_DIR = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\..\\"));
        public static readonly string DEFAULT_DATA_FILE = SOLUTION_DIR + "documents.csv";
        private const int _numDocuments = 1000;
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
        /// This function parses the csv contents of a given URI through usage of MovieIndexService,
        /// poviding uri points to a correctly formatted movie index csv. It then establishes a 
        /// connection to ElasticSearch, and uses the ESService to document all parsed MovieIdndexes.
        /// Upon indexing, response strings of ES's current indexes are printed to gui and a query is made
        /// to ES for an index using MatchAll with the result output to gui using json serialization
        /// </summary>
        /// <param name="uri">The full absolute path to a file, must be a MovieIndex csv as per assignment</param>
        public bool PerformFullIndexing(string uri, int numDocs)
        {
            try
            {
                // creates data services
                var _miService = new MovieIndexService<MovieIndex>();
                var _esService = new ElasticService<MovieIndexService<MovieIndex>, MovieIndex>(_miService);
                _esService.InitDB();
                _miService.NumDocuments = numDocs;

                // parses csv files
                DataMunger munger = new DataMunger(uri);
                var movieIndexList = munger.GetMovieIndexes();
                movieIndexList.Sort();

                // uploads documents to database
                _miService.AddDocuments(movieIndexList);
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
                gui?.AddConsoleMessage(e.Message, IGUIAdapter.GUIColor.ERROR_COLOR);
                return false;
            }
        }

        /// <summary>
        /// Assignment Step 2 - Tokenization
        /// This function instantiates data services, calls the analyzer engine to generate the needed ProcessingPipelines
        /// for each document in the corpus, uploads the processed data to database via services, and outputs the first
        /// documents with ID 1-6 to gui
        /// </summary>
        public bool PerformTokenization()
        {
            try
            {
                // creates database services
                var tokenizedService = new MovieIndexServiceProcessed<MovieIndexTokenized>();
                var elasticService = new ElasticService<MovieIndexService<MovieIndexTokenized>, MovieIndexTokenized>(tokenizedService);
                elasticService.InitDB();

                //processes documents and uploads to ElasticSearch
                engine.GenerateTokenizatedPipes();
                var processedDocs = SortedIndexDictToList(CreateMovieIndexChildClasses(tokenizedService));
                tokenizedService.UploadData(elasticService);

                // outputs first 6 documents to gui
                var top6docs = processedDocs.Take(6).ToList();
                elasticService.AwaitASync();
                string result = $"Created Tokenized Processed Index: {tokenizedService.GetIndexTitle()}\n> 6 results: \n";
                foreach (var document in top6docs)
                {
                    result += document.Serialize() + "\n";
                }
                gui?.AddConsoleMessage(result);
                return true;
            }
            catch (Exception e)
            {
                gui?.AddConsoleMessage(e.Message, IGUIAdapter.GUIColor.ERROR_COLOR);
                return false;
            }
        }

        /// <summary>
        /// Assignment Step 3 - Keyword Selection
        /// This function instantiates the data services, calls analyser engine methods for document processing used in this
        /// step (stop word removal with zipf analysis, phrase generation, removal of very infrequent words, IDF weighting, 
        /// and keyword selection), uploads processed documents to database, and outputs first documents ID'd 1-6 to gui
        /// </summary>
        public bool PerformKeywordSelection()
        {
            try
            {
                // creates database services
                var keywordedService = new MovieIndexServiceProcessed<MovieIndexKeyWords>();
                var elasticService = new ElasticService<MovieIndexService<MovieIndexKeyWords>, MovieIndexKeyWords>(keywordedService);
                elasticService.InitDB();

                // processes documents for this steps
                engine.RemoveStopWords();
                engine.GeneratePhrases();
                engine.RemoveVeryInfrequentWords();
                engine.CalculateIDFs();
                engine.AddKeywordsToPipes();

                // indexes documents and uploads to ElasticSearch
                var indexedDocs = SortedIndexDictToList(CreateMovieIndexChildClasses(keywordedService));
                keywordedService.UploadData(elasticService);

                // outputs first 6 documents to gui
                var top6docs = indexedDocs.Take(6).ToList();
                elasticService.AwaitASync();
                string result = $"Created Keyworded Processed Index: {keywordedService.GetIndexTitle()}\n> 6 results: \n";
                foreach (var document in top6docs)
                {
                    result += document.Serialize() + "\n";
                }
                gui?.AddConsoleMessage(result);
                return true;
            }
            catch (Exception e)
            {
                gui?.AddConsoleMessage(e.Message, IGUIAdapter.GUIColor.ERROR_COLOR);
                return false;
            }

        }

        /// <summary>
        /// Assignment Step 4 - 
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
                SortedDictionary<int, MovieIndexKeyWordStemmed> results = CreateMovieIndexChildClasses(tokenizedService);

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
                gui?.AddConsoleMessage(e.Message, IGUIAdapter.GUIColor.ERROR_COLOR);
                return false;
            }
}

        public MovieIndexMatches PerformSearch(string searchString, FieldName fieldType = FieldName.NONE)
        {
            try
            {
                // create pipes of query, 2 needed, one keyworded and stemmed one none keyworded
                var keywordedQueryPipe = new ProcessingPipeline.Builder(searchString).
                        SplitBulletPoints().
                        SplitSentences().
                        RemovePunctuation().
                        Normalize().
                        Tokenize().
                        Build();
                keywordedQueryPipe.NGramNum = 3;
                keywordedQueryPipe.MakeNGrams();
                keywordedQueryPipe.AddTokensToKeywords();
                keywordedQueryPipe.StemKeywords();

                var rawProcessedQueryPipe = new ProcessingPipeline.Builder(searchString).
                        SplitBulletPoints().
                        SplitSentences().
                        RemovePunctuation().
                        Normalize().
                        Tokenize().
                        Build();

                // instantiates data services
                var tokenizedService = new MovieIndexServiceProcessed<MovieIndexKeyWords>();
                var elasticService = new ElasticService<MovieIndexService<MovieIndexKeyWords>, MovieIndexKeyWords>(tokenizedService);

                //makes query
                List<MovieIndexKeyWords> results = elasticService.KeywordQuery<MovieIndexKeyWords>(keywordedQueryPipe.Keywords);
                MovieIndexMatches matchesObj = new MovieIndexMatches(results.Count, searchString);

                // processes queries
                foreach (MovieIndexKeyWords match in results)
                {
                    // finds cosing similarity between query and match document
                    var matchPipe = new ProcessingPipeline.Builder(match.GetFullText())
                        .SplitBulletPoints()
                        .SplitSentences()
                        .RemovePunctuation()
                        .Normalize()
                        .Tokenize()
                        .Build();
                    double queryDocSimilarity = engine.CosineSimilarity(rawProcessedQueryPipe.TermsAndStats, matchPipe.TermsAndStats);

                    // processes match, checking if fields match if user selected a field in gui
                    MovieIndexQueryMatch processedMatch;
                    if (fieldType != FieldName.NONE)
                    {
                        bool fieldMatches = FieldMatches(fieldType, match, rawProcessedQueryPipe.Tokens);
                        processedMatch = new MovieIndexQueryMatch(queryDocSimilarity, match, true, fieldMatches);
                    }
                    else
                    {
                        processedMatch = new MovieIndexQueryMatch(queryDocSimilarity, match);
                    }
                    matchesObj.Matches.Add(processedMatch);
                }

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
                gui?.AddConsoleMessage(e.Message, IGUIAdapter.GUIColor.ERROR_COLOR);
                return null;
            }
        }

        /// <summary>
        /// Detects if any of the terms in the users query matches a given match's field of a given field
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
            if (fieldPipe.Tokens.Intersect(queryRaw).Count() > 0)
                return true;
            return false;
        }

        /// <summary>
        /// Generates the analysis bar charts for Zipf stopword removal in gui
        /// </summary>
        public void RunZipfsSelectionAnalysis()
        {
            gui?.SetChart(engine.StopWordGenerator.GetZipfsNormStats());
            gui?.SetChart(engine.StopWordGenerator.GetZipfsLogStats());
            gui?.SetChart(engine.StopWordGenerator.SelectStopWordsStandardDev());
            gui?.SetChart(engine.StopWordGenerator.SelectStopWordsMedianIQRange());
            gui?.SetChart(engine.StopWordGenerator.SelectStopWordsLogMidPoint());
        }

        /// <summary>
        /// Creates an ID keyed dictionary of MovieIndex child objects of type T
        /// </summary>
        /// <typeparam name="T">The type of MovieIndex child class to create</typeparam>
        /// <param name="miservice">The movie index service containing all documents of type T</param>
        /// <returns>A dictionary with ID keys and type T movie index derrived classes (the documents)</returns>
        public SortedDictionary<int, T> CreateMovieIndexChildClasses<T>(MovieIndexService<T> miservice) where T :
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
