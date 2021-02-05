using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Assignment1
{
    public class CSVParser
    {
        public enum ParseState
        {
            SUCCESS, ERROR, PARSING
        }

        public struct Token
        {
            public string Value;
            public int StartIndex;
            public int EndIndex;
            public Token(string value, int start, int end)
            {
                this.Value = value;
                this.StartIndex = start;
                this.EndIndex = end;
            }
        }

        public struct Entity
        {
            public int LineNumber;
            public Dictionary<string, Token> Values;
            public Entity(int lineNumber)
            {
                this.LineNumber = lineNumber;
                this.Values = new Dictionary<string, Token>();
            }
        }

        private ParseState? _state = null;
        public ParseState State { get
            {
                if (_state == null)
                    return ParseState.ERROR;
                else
                    return (ParseState)_state;
            } }

        private List<Entity> _entities = new List<Entity>();
        public List<Entity> EntityList { get { return _entities; } }
        private List<char> _delims = new List<char>() { ',', '\"', '\n' };
        private List<string> _keys = new List<string>();
        private Parser parser = new Parser();
        private List<string> _columnTitles = new List<string>();
        private Entity _currentEntity;
        private readonly int NUM_COLUMNS;
        private bool _hasHeader;
        private string _text = "";
        private int _columnProcessing = 0;
        private int _currentValueIndex = 0;
        private int _currentEntityStartIndex = 0;
        private int _currentLine = 0;
        private int _totalLines = 0;

        public CSVParser(string text, int numColumns = 8, bool hasHeader = true)
        {
            if (numColumns == 0)
                throw new Exception("Attempt to parse CSV without any column count");
            NUM_COLUMNS = numColumns;
            _hasHeader = hasHeader;
            _text = text.Replace("\r\n", "\n");
            if (hasHeader)
                InitHeader();
            else
                InitBlankHeader();
            ParseFile();
        }

        private void InitBlankHeader()
        {
            for (int i = 0; i < NUM_COLUMNS; ++i)
                _columnTitles.Add($"Key{i}");
            _currentEntity = new Entity(0);
        }

        private void InitHeader()
        {
            // will process first NUM_COLUMNS until new line character reached
            // adding values to _columnTitles and incrementing _currentValueIndex & 
            // _currentEntityStartIndex
            int valueCount = 0;
            int index = 0;
            do
            {
                if(_text[index] == _delims[0] || _text[index] == _delims[2])
                {
                    _columnTitles.Add(_text.Substring(_currentValueIndex, index - _currentValueIndex));
                    valueCount++;
                    _currentValueIndex = index + 1;
                }
            } while (_text[index++] != _delims[2]);
            if (index == _text.Length || valueCount != NUM_COLUMNS)
                throw new Exception("Error processing CSV with incorrect # columns in header or"
                    + "empty CSV file past header");
            _currentEntityStartIndex = index;
            _currentLine++;
            _totalLines++;
            _currentValueIndex = index;
            _currentEntity = new Entity(_currentLine);
        }

        public void ParseFile()
        {
            int index = _currentValueIndex;

            _state = ParseState.PARSING;
            while (index < _text.Length)
            {
                index = ParseValue(index);
                index++;
            }
            _state = ParseState.SUCCESS;
        }

        public int ParseValue(int index)
        {
            _currentValueIndex = index;
            while(_text[index] != _delims[0] && _text[index] != _delims[2])
            {
                if(_text[index] == _delims[1])
                {
                    index = ParseQuoted(index); 
                }
                index++;
            }
            MakeValue(index);
            return index;
        }

        public int ParseQuoted(int index)
        {
            index++;
            while (_text[index] != _delims[1])
            {
                index++;
            }
            return index;
/*            Stack<char> stack = new Stack<char>();
            stack.Push(_delims[1]);
            bool closing = false;
            do
            {
                char ch = _text[index];
                if(_text[index] == _delims[2])
                {
                    _totalLines++;
                }
                if (_text[++index] == _delims[1])
                {
                    if (!closing)
                        stack.Push(_delims[1]);
                    else
                        stack.Pop();
                }
                else if (stack.Count > 0)
                {
                    closing = true;
                }
                else
                {
                    closing = false;
                }
                if (stack.Count == 0)
                {
                    closing = true;
                }
            } while (index < _text.Length - 1 && (!closing || stack.Count > 0));
            string newText = _text.Substring(_currentValueIndex, index - _currentValueIndex);
            return index;*/
        }

        public void MakeValue(int index)
        {
            string value = _text.Substring(_currentValueIndex, index - _currentValueIndex);
            if (_columnProcessing == 0)
            {
                int year = int.Parse(value);
            }
            Token token = new Token(value, _currentValueIndex, index);
            _currentEntity.Values[_columnTitles[_columnProcessing]] = token;
            _currentValueIndex += index;
            _columnProcessing++;
            if (_columnProcessing == NUM_COLUMNS)
            {
                _totalLines++;
                _currentLine = _totalLines;
                _currentEntityStartIndex = index + 1;
                _columnProcessing = 0;
                _entities.Add(_currentEntity);
                _currentEntity = new Entity(_currentLine);
            }
        }

    }
}
