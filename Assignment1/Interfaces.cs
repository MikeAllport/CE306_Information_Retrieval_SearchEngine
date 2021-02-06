using System;
using System.Collections.Generic;
using System.Text;
using Nest;

namespace Assignment1
{
    /// <summary>
    /// IIndexedDataService's purpose is such that the ElasticService can interface
    /// with seperate index classes to create indexes and send correctly formatted
    /// indexes to the database
    /// </summary>
    public interface IIndexedDataService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetIndexTitle();
        public void CreateIndex(ElasticClient client);
    }

    /// <summary>
    /// ICSVEntity exists such that classes who realise it can add the value of a given column
    /// to its own data fields
    /// </summary>
    public interface ICSVEntity
    {
        public void AddValue(int column, string data, int lineNumberInFile);
        public void AddFullText(string text);
    }
}
