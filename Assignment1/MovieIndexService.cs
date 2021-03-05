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
    /// 
    /// This class is generic, proving use for any class which extends MovieIndex, so that other
    /// Movie index types can be used
    /// </summary>
    [ElasticsearchType(RelationName = "full_text")]
    public class MovieIndexService<T> : IIndexableDB
        where T : MovieIndex
    {
        public static readonly string IndexTitle = "movies-full-text"; // index name
        public Dictionary<int, T> MovieIndexMap = new Dictionary<int, T>();
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
        public void AddDocuments(IEnumerable<T> collection)
        {
            foreach (T document in collection)
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
        public virtual string GetIndexTitle()
        {
            return IndexTitle;
        }

        /// <summary>
        /// Creates index in elasticsearch
        /// </summary>
        /// <param name="client">Database connecter</param>
        public virtual void CreateIndex(ElasticClient client)
        {
            client.Indices.Create(IndexTitle, c => c
                .Map<T>(m => m
                    .AutoMap()
                    )
                );
        }


        /// <summary>
        /// Sends data to ElasticService for data insertion into database
        /// </summary>
        /// <param name="service">The database connector</param>
        public void UploadData(ElasticService<MovieIndexService<T>, T> service)
        {
            foreach (KeyValuePair<int, T> document in MovieIndexMap)
                service.IndexASync(document.Value);
        }
    }

    /// <summary>
    /// MovieIndexServiceProcessed is used for creation of a seperate database, "movie-index-processed"
    /// </summary>
    /// <typeparam name="T">The type of MovieIndex base or sub to use</typeparam>
    [ElasticsearchType(RelationName = "processed")]
    public class MovieIndexServiceProcessed<T> : MovieIndexService<T>
    where T : MovieIndex
    {
        public static readonly string ProcessedIndexTitle = "movies-processed"; // index name

        /// <summary>
        /// Allows for the parameterized ElasticService to access the index name
        /// </summary>
        /// <returns>Name of the index</returns>
        public override string GetIndexTitle()
        {
            return ProcessedIndexTitle;
        }

        /// <summary>
        /// Creates index in elasticsearch
        /// </summary>
        /// <param name="client">Database connecter</param>
        public override void CreateIndex(ElasticClient client)
        {
            client.Indices.Create(ProcessedIndexTitle, c => c
                .Map<T>(m => m
                    .AutoMap()
                    )
                );
        }
    }
}
