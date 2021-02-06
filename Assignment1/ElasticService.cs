using System;
using System.Collections.Generic;
using System.Text;
using Elasticsearch;
using Nest;
using System.Threading.Tasks;

namespace Assignment1
{
    ;
    /// <summary>
    /// ElasticService's purpose is to handle all communication to an open database connection.
    /// This class is generic, allowing for this to interface with and obtain
    /// indexing information, to call the classes create index method, and for this class to be aware of
    /// what type of class is being sent to the database
    /// 
    /// Its main responsibility is to create connection, delegate the creation of indexes, and to index
    /// objects to the database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ElasticService<T> where T: class, IIndexedDataService, new()
    {
        private readonly Uri URI = new Uri("http://localhost:9200/"); // ElasticSearch uri
        public  ElasticClient client; // database connection
        // a list used to allow for a-synchronous data insertions
        private List<Task<IndexResponse>> _asyncAwaitSyncList = new List<Task<IndexResponse>>();

        /// <summary>
        /// Establishes the connection to the datase and ensures index has been created
        /// </summary>
        public ElasticService()
        {
            T t = new T();
            var settings = new ConnectionSettings(URI);
            settings.DefaultIndex(t.GetIndexTitle());
            settings.ThrowExceptions(true);
            settings.PrettyJson(true);
            client = new ElasticClient(settings);
            if (!client.Indices.Exists(t.GetIndexTitle()).Exists)
            {
                t.CreateIndex(client);
            }
        }

        /// <summary>
        /// Inserts document to database/elasticsearch
        /// </summary>
        /// <param name="doc">Generic class instance to be inserted to database</param>
        public void IndexDocument(T doc)
        {
            IndexResponse response = client.IndexDocument<T>(doc);
        }

        /// <summary>
        /// Inserts document asynchronously
        /// </summary>
        /// <param name="doc">Generic class instance to be inserted to database</param>
        public async void IndexASync(T doc)
        {
            Task<IndexResponse> task = client.IndexDocumentAsync<T>(doc);
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
                var response = task.GetAwaiter().GetResult();
            }
        }
    }
}
