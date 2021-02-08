﻿using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CsvHelper;

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

        public List<MovieIndexService> GetMovieIndexes()
        {
            List<MovieIndexService> indexList;
            using (StreamReader reader = File.OpenText(_filename))
            {

                string line = reader.ReadToEnd();
                CSVParser<MovieIndexService> parser = new CSVParser<MovieIndexService>(line);
                indexList = parser.EntityList;
                int x = 1;
            }
            return indexList;
        }
    }
}
