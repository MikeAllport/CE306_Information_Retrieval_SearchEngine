using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CsvHelper;

namespace Assignment1
{
    /// <summary>
    /// DataMungers main responsibility is to load a CSV file and instantiate
    /// CSVParser to extract MovieIndexes
    /// </summary>
    class DataMunger
    {
        private string _filename;
        public string Filename { get { return _filename; } }
        public DataMunger(string filename)
        {
            this._filename = filename;
        }

        /// <summary>
        /// Instantiates CSVParser and adds documents to list
        /// </summary>
        /// <returns></returns>
        public List<MovieIndex> GetMovieIndexes()
        {
            List<MovieIndex> indexList;
            using (StreamReader reader = File.OpenText(_filename))
            {

                string line = reader.ReadToEnd();
                CSVParser<MovieIndex> parser = new CSVParser<MovieIndex>(line);
                indexList = parser.EntityList;
            }
            return indexList;
        }
    }
}
