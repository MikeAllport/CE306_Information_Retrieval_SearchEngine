using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Assignment1
{
    class DataMunger
    {
        private string _filename;
        public string Filename { get { return _filename; } }
        public DataMunger(string filename)
        {
            this._filename = filename;
        }

        public List<MovieIndex> GetMovieIndexes()
        {
            List<MovieIndex> indexList;
            using (StreamReader reader = File.OpenText(_filename))
            {

                string line = reader.ReadToEnd();
                CSVParser<MovieIndex> parser = new CSVParser<MovieIndex>(line);
                indexList = parser.EntityList;
                int x = 1;
            }
            return indexList;
        }
    }
}
