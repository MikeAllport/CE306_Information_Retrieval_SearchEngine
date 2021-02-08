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
            ElasticService<MovieIndexService> service = new ElasticService<MovieIndexService>();
            MovieIndexService.service = service;
            DataMunger munger = new DataMunger(DATA_ABS_PATH);
            munger.GetMovieIndexes();
            service.AwaitASync();
            MovieIndexService ind = new MovieIndexService();
        }
    }
}
