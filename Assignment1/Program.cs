using System;
using System.IO;
using Elasticsearch;
using Nest;

namespace Assignment1
{
    class Program
    {
        private static readonly string DATA_ABS_PATH =
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\..\\documents.csv"));
        ElasticClient client;
        static void Main(string[] args)
        {
            ElasticService<FullTextIndexer> service = new ElasticService<FullTextIndexer>();
            FullTextIndexer.service = service;
            DataMunger munger = new DataMunger(DATA_ABS_PATH);
            munger.GetFullTextIndexes();
            service.AwaitASync();
            FullTextIndexer ind = new FullTextIndexer();
        }
    }
}
