using System;
using System.Collections.Generic;
using System.Text;
using Nest;

namespace Assignment1
{
    /// <summary>
    /// MovieIndexService's purpose is to hold data members for the movie indexes
    /// and realise methods required to create the full text index on the database
    /// and perform actions from csv parsing to create a movie index
    /// </summary>
    [ElasticsearchType(RelationName = "full_text")]
    public class MovieIndexService: ICSVEntity, IIndexedDataService
    {

        public static ElasticService<MovieIndexService> service; // database adapter
        public static int IndexCount = 0; // static id number
        public static readonly string IndexTitle = "movies-full-text"; // index name
        public static Dictionary<int, MovieIndex> MovieIndexMap = new Dictionary<int, MovieIndex>();
        

        [Text(Name = "text")]
        public string FullText { get; set; } = ""; // attribute for the text
        [Text(Name = "index._id")]
        public int ID { get; set; } = 0; // instances id
        private MovieIndex _index = new MovieIndex();

        /// <summary>
        /// this method requires no action, as the full text class is not
        /// here to store data from fields
        /// </summary>
        public void AddValue(int column, string value, int lineNum)
        {
            switch (column)
            {
                case 0:
                    _index.ReleaseYear = int.Parse(value);
                    break;
                case 1:
                    _index.Title = value;
                    break;
                case 2:
                    _index.Origin = value;
                    break;
                case 3:
                    _index.Director = value;
                    break;
                case 4:
                    _index.Cast = value;
                    break;
                case 5:
                    _index.Genre = value;
                    break;
                case 6:
                    _index.Wiki = value;
                    break;
                case 7:
                    _index.Plot = value;
                    ID = IndexCount++;
                    _index.ID = ID;
                    MovieIndexMap[ID] = _index;
                    break;
            }
        }

        /// <summary>
        /// AddFullText is called from the CSVParser upon each new entity. This method
        /// sends the full text of an entity to the database, and updates ID counters
        /// </summary>
        /// <param name="value">The full text of an entity parsed from csv file</param>
        public void AddFullText(string value)
        {
            FullText = value;
            UploadData(service);
            FullText = "";
        }

        /// <summary>
        /// Sends data to ElasticService for data insertion into database
        /// </summary>
        /// <param name="service">The database connector</param>
        public void UploadData(ElasticService<MovieIndexService> service)
        {
            service.IndexASync(this);
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
    }
}
