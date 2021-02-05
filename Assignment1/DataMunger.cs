using System;
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
            ProcessDocument();
        }

        private void ProcessDocument()
        {
            using (StreamReader reader = File.OpenText(_filename))
            {

                string line = reader.ReadToEnd();
                CSVParser parser = new CSVParser(line);
                int x = 1;
            }
        }
    }
}
