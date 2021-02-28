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
        private static readonly string DATA_ABS_PATH = SOLUTION_DIR +"documents.csv";
        private static readonly int _numDocuments = 1000;
        private MovieIndexService<MovieIndex> _miService;
        private ElasticService<MovieIndexService<MovieIndex>, MovieIndex> _esService;
        private AnalyserEngine engine;
        private IGUIAdapter.Adapter gui;
        public static bool _running = true;

        static void Main(string[] args)
        {
        }

        public Program(IGUIAdapter.Adapter gui)
        {
            this.gui = gui;
        }
        
        /// <summary>
        /// Assignment Step 1 - Full Indexing
        /// This function parses the csv contents of a given URI, given the uri points to a correctly
        /// formatted movie index csv, through usage of MovieIndexService. It then establishes a 
        /// connection to ElasticSearch, and uses the ESService to document all parsed MovieIdndexes.
        /// Upon indexing, response strings of ES's current indexes following indexing and a query is made
        /// to ES for an index using MatchAll with the result deserialized as json to be output
        /// to the gui console.
        /// </summary>
        /// <param name="uri">The full absolute path to a file, must be a MovieIndex csv as per assignment</param>
        public void PerformFullIndexing(string uri)
        {
            try
            {
                _miService = new MovieIndexService<MovieIndex>();
                _miService.NumDocuments = _numDocuments;
                AddDocuments(_miService, uri);
                _esService = new ElasticService<MovieIndexService<MovieIndex>, MovieIndex>(_miService);
                _miService.UploadData(_esService);
                _esService.AwaitASync();
                string result = _esService.DescribeIndices();
                result = "Created Index: " + _miService.GetIndexTitle() + "\n> ElasticSearch list:\n" + result;
                result += "\n> First result: \n";
                foreach (var document in _esService.GetFullMatches(1))
                {
                    result += document.Serialize() + "\n";
                }
                gui.AddConsoleMessage(result);
            }
            catch (Exception e)
            {
                gui.AddConsoleMessage(e.Message, IGUIAdapter.GUIColor.ERROR_COLOR);
            }
        }

        public void PerformTokenization()
        {
            engine = new AnalyserEngine(_miService.MovieIndexMap, gui, new AnalyserEngineSettings());
            var tokenizedService = new MovieIndexServiceProcessed<MovieIndexTokenized>();
            var elasticService = new ElasticService<MovieIndexService<MovieIndexTokenized>, MovieIndexTokenized>(tokenizedService);
            var top6docs = AddDocuments(tokenizedService);
            tokenizedService.UploadData(elasticService);
            elasticService.AwaitASync();
            string result = $"Created Tokenized Processed Index: {tokenizedService.GetIndexTitle()}\n> 6 results: \n";
            foreach (var document in top6docs)
            {
                result += document.Serialize() + "\n";
            }
            gui.AddConsoleMessage(result);
        }

        public void PerformStemming()
        {
            var tokenizedService = new MovieIndexServiceProcessed<MovieIndexStemmed>();
            var elasticService = new ElasticService<MovieIndexService<MovieIndexStemmed>, MovieIndexStemmed>(tokenizedService);
            var top6docs = AddDocuments(tokenizedService);
            tokenizedService.UploadData(elasticService);
            elasticService.AwaitASync();
            string result = $"Created Stemmed Processed Index: {tokenizedService.GetIndexTitle()}\n> 6 results: \n";
            foreach (var document in top6docs)
            {
                result += document.Serialize() + "\n";
            }
            gui.AddConsoleMessage(result);
        }

        public void RunZipfsSelectionAnalysis()
        {
            gui.SetChart(engine.StopWordGenerator.GetZipfsNormStats());
            gui.SetChart(engine.StopWordGenerator.GetZipfsLogStats());
            gui.SetChart(engine.StopWordGenerator.SelectStopWordsStandardDev());
            gui.SetChart(engine.StopWordGenerator.SelectStopWordsMedianIQRange());
            gui.SetChart(engine.StopWordGenerator.SelectStopWordsLogMidPoint());
        }

        private void AddDocuments(MovieIndexService<MovieIndex> miservice, string uri)
        {
            DataMunger munger = new DataMunger(uri);
            var movieIndexList = munger.GetMovieIndexes();
            movieIndexList.Sort();
            miservice.AddDocuments(movieIndexList);
        }

        private List<MovieIndexTokenized> AddDocuments(MovieIndexService<MovieIndexTokenized> miservice)
        {
            List<MovieIndexTokenized> results = new List<MovieIndexTokenized>();
            engine.GenerateTokenizatedPipes();
            foreach (var processedIndexID in engine.IndexIDDict)
            {
                var document = new MovieIndexTokenized(
                    engine.UnProcessedIndexes[processedIndexID.Key], 
                    processedIndexID.Value
                    );
                miservice.MovieIndexMap[processedIndexID.Key] = document;
                results.Add(document);
            }
            return results.Take(6).ToList();
        }
        private List<MovieIndexStemmed> AddDocuments(MovieIndexService<MovieIndexStemmed> miservice)
        {
            SortedDictionary<int, MovieIndexStemmed> results = new SortedDictionary<int, MovieIndexStemmed>();
            foreach (var processedIndexID in engine.IndexIDDict)
            {
                var document = new MovieIndexStemmed(
                    engine.UnProcessedIndexes[processedIndexID.Key],
                    processedIndexID.Value
                    );
                miservice.MovieIndexMap[processedIndexID.Key] = document;
                results[processedIndexID.Key] = document;
            }
            engine.GenerateStems();
            foreach (var processedIndexID in engine.IndexIDDict)
            {
                results[processedIndexID.Key].AddStemmedTokensFromPipe(processedIndexID.Value);
            }
            return (from KeyValuePair<int, MovieIndexStemmed> idIndexPair in results select idIndexPair.Value).Take(6).ToList();
        }
    }
}
