using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Assignment1
{
    /**
     * CSVParser's responsibility is to lexicographically parse a given string, the full file
     * contents, and filer out values for an entity based on the length of its columns whilst
     * ignoring any new lines and escape characters contained within blocked ' " 's.
     * 
     * This class is generic taking any class that realises CSVEntity passing through a value
     * and its related column number to the entity to deal with
     * */
    public class CSVParser<T> where T: class, ICSVEntity, new()
    {
        private List<T> _entities = new List<T>(); // list of entities
        public List<T> EntityList { get { return _entities; } }
        private List<char> _delimiters = new List<char>() { ',', '\"', '\n' }; 
        private List<string> _columnTitles = new List<string>();
        private T _currentEntity; 
        private readonly int NUM_COLUMNS; 
        private string _text = ""; 

        // following values are pointers to the positions being processed in file
        private int _currentColumn = 0;
        private int _currentValueIndex = 0;
        private int _currentLine = 0;
        private int _totalLines = 0;
        private int _currentEntityStartIndex = 0;

        /**
         * Basic constructor that  instantiates the headers, pointers, and local variables
         * required to parse a csv file
         * @args:
         *      string text - the file contents to be parsed
         *      int numColumns - the column count for an entity in the file
         *      bool hasHeader - whether or not to ignore the first row
         * */
        public CSVParser(string text, int numColumns = 8, bool hasHeader = true)
        {
            if (numColumns == 0)
                throw new Exception($"PerferomIndexing::{this.GetType().Name} Attempt to parse CSV without any column count");
            NUM_COLUMNS = numColumns;
            _text = text.Replace("\r\n", "\n");
            _currentEntityStartIndex = 0;
            if (hasHeader)
                InitHeader();
            _currentEntity = new T();
            ParseFile();
        }

        /**
         * InitHeader's purpose is to parse the first line of a CSV file, extracting the
         * name of the headers, and incrementing pointers
         * */
        private void InitHeader()
        {
            int valueCount = 0;
            int index = 0;
            do
            {
                if(_text[index] == _delimiters[0] || _text[index] == _delimiters[2])
                {
                    _columnTitles.Add(_text.Substring(_currentValueIndex, index - _currentValueIndex));
                    valueCount++;
                    _currentValueIndex = index + 1;
                }
            } while (_text[index++] != _delimiters[2]);
            if (index == _text.Length || valueCount != NUM_COLUMNS)
                throw new Exception($"PerferomIndexing::{this.GetType().Name} Error processing CSV with incorrect # columns in header or"
                    + "empty CSV file past header");
            _currentEntityStartIndex = index;
            _currentLine++;
            _totalLines++;
            _currentValueIndex = index;
        }

        /**
         * ParseFile's responsibility is the main running loop of the program, calling
         * ParseValue on any new column and incrementing the index
         * */
        public void ParseFile()
        {
            int index = _currentValueIndex;
            while (index < _text.Length)
            {
                index = ParseValue(index);
                index++;
            }
        }

        /**
         * ParseValue's purpose is to parse a column, whilst calling ParseQuoted helper
         * function to ignore any values contained within double quotes, many times when
         * quotes have nested quotes
         * */
        public int ParseValue(int index)
        {
            _currentValueIndex = index;
            while(_text[index] != _delimiters[0] && _text[index] != _delimiters[2])
            {
                if(_text[index] == _delimiters[1])
                {
                    index = ParseQuoted(index); 
                }
                index++;
            }
            MakeValue(index);
            return index;
        }

        /**
         * ParseQuoted purpose is to increment value pointers past a pair of quotes
         * */
        public int ParseQuoted(int index)
        {
            index++;
            while (_text[index] != _delimiters[1])
            {
                index++;
            }
            return index;
        }

        /**
         * MakeValue's purpose is to extract a value from between the current pointers values
         * and to inrement the column count, give the entity the value, and if the value being
         * processed is the last value for the entity create a new instance
         * */
        public void MakeValue(int index)
        {
            int startIndex = _currentValueIndex;
            int endIndex = index;
            if (_text[startIndex] == _delimiters[1] && _text[endIndex-1] == _delimiters[1])
            {
                startIndex++;
                endIndex--;
            }
            string value = _text.Substring(startIndex, endIndex - startIndex);
            _currentEntity.AddValue(_currentColumn, value, _currentLine);
            _currentValueIndex += index;
            _currentColumn++;
            if (_currentColumn == NUM_COLUMNS)
            {
                _totalLines++;
                _currentLine = _totalLines;
                _currentEntityStartIndex = index + 1;
                _currentColumn = 0;
                _entities.Add(_currentEntity);
                _currentEntity = new T();
            }
        }

    }
}
