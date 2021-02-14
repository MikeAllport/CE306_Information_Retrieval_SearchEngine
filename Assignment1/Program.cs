using System;
using System.IO;
using Elasticsearch;
using Nest;

namespace Assignment1
{
    public class Program
    {
        public static readonly string SOLUTION_DIR = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\..\\"));
        private static readonly string DATA_ABS_PATH = SOLUTION_DIR +"documents.csv";
        private static readonly int _numDocuments = 1000;
        private MovieIndexService _miService;
        private ElasticService<MovieIndexService> _esService;
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
        /// This function parses the csv contents a given URI, given the uri points to a correctly
        /// formatted movie index csv, through usage of MovieIndexService. It then establishes a 
        /// connection to ElasticSearch, and uses the ESService to document all parsed MovieIdndexes.
        /// Upon indexing, response strings of ES's current indexes following indexing and a query is made
        /// to ES for an index using MatchAll with the result deserialized as json to be output
        /// to the gui console.
        /// </summary>
        /// <param name="uri">The full absolute path to a file, must be a MovieIndex csv as per assignment</param>
        public void PerformIndexing(string uri)
        {
            try
            {
                _miService = new MovieIndexService();
                _miService.NumDocuments = _numDocuments;
                AddDocuments(_miService, uri);
                _esService = new ElasticService<MovieIndexService>(_miService);
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

        private static void AddDocuments(MovieIndexService miservice, string uri)
        {
            DataMunger munger = new DataMunger(uri);
            var movieIndexList = munger.GetMovieIndexes();
            movieIndexList.Sort();
            miservice.AddDocuments(movieIndexList);
        }
    }
}
