using System;
using System.Collections.Generic;
using System.Text;
using Elasticsearch;
using Nest;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Threading;

namespace Assignment1
{
    /// <summary>
    /// ElasticService's purpose is to handle all communication to an open database connection.
    /// This class is generic, allowing for this to interface with and obtain
    /// indexing information, to call the classes create index method, and for this class to be aware of
    /// what type of class is being sent to the database
    /// 
    /// Its main responsibility is to create connection, delegate the creation of indexes, and to index
    /// objects to the database
    /// </summary>
    /// <typeparam name="T">type used for retriving database information from class/es</typeparam>
    /// <typeparam name="T">type used for Nest binding matches to a MovieIndex/subclass</typeparam>
    public class ElasticService<T, J> 
        where T: class, IIndexableDB
        where J: MovieIndex
    {
        private readonly Uri URI = new Uri("http://localhost:9200/"); // ElasticSearch uri
        public  ElasticClient client; // database connection
        // a list used to allow for a-synchronous data insertions
        private List<Task<IndexResponse>> _asyncAwaitSyncList = new List<Task<IndexResponse>>();
        private T _indexService;

        /// <summary>
        /// Establishes the connection to the datase and ensures index has been created
        /// </summary>
        public ElasticService(T indexService)
        {
            this._indexService = indexService; var settings = new ConnectionSettings(URI);
            settings.DefaultIndex(this._indexService.GetIndexTitle());
            settings.ThrowExceptions(true);
            settings.PrettyJson(true);
            client = new ElasticClient(settings);
        }

        public void InitDB()
        {
            // TODO: insert try catch for Elasticsearch.Net.ElasticsearchClientException
            try
            {
                if (client.Indices.Exists(this._indexService.GetIndexTitle()).Exists)
                {
                    client.Indices.Delete(this._indexService.GetIndexTitle());
                }
                this._indexService.CreateIndex(client);
            }
            catch (Elasticsearch.Net.ElasticsearchClientException)
            {
                throw new Exception($"PerformIndexing::{this.GetType().Name} Could not establish database connection");
            }
        }

        /// <summary>
        /// Simple method that returns a string describing all databases in Elasticsearch
        /// </summary>
        /// <returns></returns>
        public string DescribeIndices()
        {
            return BasicWebRequest("_cat/indices?v");
        }

        /// <summary>
        /// Returns a MatchAll query of length num repsponses
        /// </summary>
        /// <param name="num">How many results to return</param>
        /// <returns>Collection of type J results, a movie index or one of its sub classes</returns>
        public IReadOnlyCollection<J> GetFullMatches(int num)
        {
            var response = client.Search<J>(s => s
                .From(0)
                .Size(num)
                .MatchAll()
            );
            return response.Documents;
        }

        /// <summary>
        /// This method is for returning MovieIndexKeyWord results for a given query input.
        /// This method has been generalized such that we can obtain MovieIndexKeywords fields
        /// </summary>
        /// <typeparam name="Q">MovieIndexKeyWords only</typeparam>
        /// <param name="terms">The list of parsed query term strings</param>
        /// <returns>List of MovieIndexKeyword results</returns>
        public List<Q> KeywordQuery<Q>(List<string> terms) where Q:
            MovieIndexKeyWords
        {
            List<Q> results = new List<Q>();
            foreach (string term in terms)
            {
                var response = client.Search<Q>(s => s
                    .Query(q => q
                        .Match(m => m
                            .Field(field => field.KeyWords)
                            .Query(term)
                        )
                    )
                    .From(0)
                    .Size(Program._numDocuments));
                foreach(var result in response.Documents)
                {
                    results.Add(result);
                }
            }
            return results;
        }

        /// <summary>
        /// Very simple method for querying the database with your own query string, this was
        /// mainly used in testing
        /// </summary>
        /// <param name="request">A query string for elasticsearch</param>
        /// <returns>Raw http results</returns>
        private string BasicWebRequest(string request)
        {
            WebRequest myReq = WebRequest.Create(URI.ToString() + request);
            WebResponse wr = myReq.GetResponse();
            Stream receiveStream = wr.GetResponseStream();
            StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Inserts document to database/elasticsearch
        /// </summary>
        /// <param name="doc">Generic class instance to be inserted to database</param>
        public void IndexDocument(object doc)
        {
            IndexResponse response = client.IndexDocument(doc);
        }

        /// <summary>
        /// Inserts document asynchronously
        /// </summary>
        /// <param name="doc">Generic class instance to be inserted to database</param>
        public async void IndexASync(object doc)
        {
            Task<IndexResponse> task = client.IndexDocumentAsync(doc);
            _asyncAwaitSyncList.Add(task);
            await task;
        }

        /// <summary>
        /// re-synchronises the threads so that program can continue once all data
        /// has been uploaded
        /// </summary>
        public void AwaitASync()
        {
            foreach (var task in _asyncAwaitSyncList)
            {
                task.Wait();
                do
                {
                    var response = task.GetAwaiter().GetResult();
                    Thread.Sleep(5);
                } while (!task.GetAwaiter().IsCompleted);
                task.GetAwaiter().GetResult();
                task.Dispose();
            }
            _asyncAwaitSyncList.Clear();
        }
    }
}
