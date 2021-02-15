using System;
using System.Collections.Generic;
using System.Text;
using Nest;

namespace Assignment1
{
    /// <summary>
    /// MovieIndexService's purpose is to generate a mapping of MovieIndex documents to a generated
    /// ID, and to contain member functions enabling uploading the corpus's documents
    /// to ElasticSearch though the realised interface IDocumentable.
    /// 
    /// This class is also responsible for how many documents get indexed, by tracking ids issued
    /// 
    /// MovieIndexService also contains member functions required to create an index on the elasticsearch 
    /// database by realising IIndexableDB interface
    /// </summary>
    [ElasticsearchType(RelationName = "full_text")]
    public class MovieIndexService: IIndexableDB, IDocumentable<MovieIndexService>
    {
        public static readonly string IndexTitle = "movies-full-text"; // index name
        public Dictionary<int, MovieIndex> MovieIndexMap = new Dictionary<int, MovieIndex>();
        private int ID { get; set; } = 1;
        public int NumDocuments { get; set; } = -1;

        /// <summary>
        /// AddDocuments purpose is to attain the first 1000 documents, which has been sorted by
        /// ReleaseYear
        /// 
        /// This is the main control method of how many documents get uploaded and indexed
        /// by tracking the number of id's issued
        /// </summary>
        /// <param name="collection"></param>
        public void AddDocuments(IEnumerable<MovieIndex> collection)
        {
            foreach (MovieIndex document in collection)
            {
                document.ID = ID++;
                MovieIndexMap[ID] = document;
                if (NumDocuments > 0 && ID > NumDocuments)
                    break;
            }
        }


        /// <summary>
        /// Allows for the parameterized ElasticService to access the index name
        /// </summary>
        /// <returns>Name of the index</returns>
        public string GetIndexTitle()
        {
            return IndexTitle;
        }

        /// <summary>
        /// Creates index in elasticsearch
        /// </summary>
        /// <param name="client">Database connecter</param>
        public void CreateIndex(ElasticClient client)
        {
            client.Indices.Create(IndexTitle, c => c
                .Map<MovieIndexService>(m => m
                    .AutoMap()
                    )
                );
        }


        /// <summary>
        /// Sends data to ElasticService for data insertion into database
        /// </summary>
        /// <param name="service">The database connector</param>
        public void UploadData(ElasticService<MovieIndexService> service)
        {
            foreach (KeyValuePair<int, MovieIndex> document in MovieIndexMap)
                service.IndexASync(document.Value);
        }
    }
}
