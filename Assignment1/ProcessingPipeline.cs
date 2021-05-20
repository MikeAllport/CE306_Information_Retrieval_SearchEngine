using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static Assignment1.ProcessingPipeline.Pipes;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;

namespace Assignment1
{
    /// <summary>
    /// ProcessingPipeline's purpose is to perform all of the operations required in tokenizing inlucing
    /// sentence splitting, bullet point detencion, removing mid-word punctuation such as apostrophes  ,
    /// normalizing the text to lowercase, tokenizing, and creating ngrams from tokens.
    /// 
    /// The whole process is controlled with a builder pattern, which a nested class
    /// </summary>
    public class ProcessingPipeline

    {
        /// <summary>
        /// Pipes purpose is to enumerate all available steps in the pipeline process
        /// </summary>
        public enum Pipes
        {
            SENTENCE_SPLIT,
            SPLIT_BULLETS,
            REMOVE_PUNCT,
            NORMALIZE,
            TOKENIZE,
            DO_COUNT
        }

        /*******************************************************************/
        /*************      ProcessingPipeline           *******************/
        /*************   (Builder class further below)   *******************/
        /*******************************************************************/

        // stores what processing to be done from builder
        internal List<Pipes> _pipeList;
        public string OriginalText { get; }
        private List<string> _tokens = new List<string>();
        public List<string> Tokens { get { return _tokens; } }

        private List<string> _sentences = new List<string>();
        public List<string> Sentences { get { return _sentences; } }

        private List<string> _bulletPoints = new List<string>();
        public List<string> BulletPoints { get { return _bulletPoints; } }

        private List<string> _stringsInPipeline = new List<string>();
        public List<string> StringsInPipeline { get { return _stringsInPipeline; } }

        public BagOfWords TermsAndStats { get; set; } = new BagOfWords();
        private List<string> _keywords = new List<string>();
        public List<string> Keywords { get { return _keywords; } }

        internal int _ngramNum;
        public int NGramNum { get { return _ngramNum; } set { _ngramNum = value; } }
        private List<string> _NGrams = new List<string>();
        public List<string> NGrams { get { return _NGrams; } }

        // some constants used for the OpenNLP model directories and ngram delimiter
        private static readonly string NGRAM_DELIM = ";;";

        /// <summary>
        /// Private internal constructor, so this cannot be used without a builder
        /// </summary>
        /// <param name="inputText">The text to be processed</param>
        internal ProcessingPipeline(string inputText)
        {
            OriginalText = inputText;
        }

        /// <summary>
        /// Loops through the constructed pipeline and performs the associated
        /// operations built with the builder
        /// </summary>
        internal void Run()
        {
            _stringsInPipeline = new List<string>() { OriginalText };
            foreach (Pipes pipe in _pipeList)
            {
                switch(pipe)
                {
                    case SENTENCE_SPLIT:
                        SplitSentences();
                        break;
                    case SPLIT_BULLETS:
                        SplitBulletPoints();
                        break;
                    case REMOVE_PUNCT:
                        RemovePunc();
                        break;
                    case NORMALIZE:
                        Normalize();
                        break;
                    case TOKENIZE:
                        Tokenize();
                        break;
                }
            }
        }

        /// <summary>
        /// Splits the pipelines input text into sentences through use of OpenNLP model
        /// </summary>
        private void SplitSentences()
        {
            List<string> output = new List<string>();
            //Regex pattern = new Regex(@"(.*?([.!?$]|(\.[A-Z]))(?:\s|$))|.*?$"); 
            Regex pattern = new Regex(@"(.*?[.!?$](?:\n|\s|$))|\w.*(?!:[$\n])");
            foreach (string inputItem in _stringsInPipeline)
            {
                var match = pattern.Matches(inputItem);
                output.AddRange((from Match m in pattern.Matches(inputItem) select m.Value).ToList());
            }
            _sentences = _sentences.Union(output).ToList();
            _stringsInPipeline = output;
        }

        /// <summary>
        /// Splits all bullet points strings in the pipeline list with regex pattern matching:
        /// Number. bullets
        /// Numer- bullets
        /// Ascii(\u2022) bullets (the round ones)
        /// </summary>
        private void SplitBulletPoints()
        {
            List<string> output = new List<string>();
            Regex pattern = new Regex(@"([\u2022][^\u2022\n$]*)|([0-9]{1,}\.\s[^0-9\n$]*)|([0-9]{1,}\s-\s[^0-9\n$]*)");
            foreach (string inputItem in _stringsInPipeline)
            {
                var hits = pattern.Matches(inputItem).Where(match => !IsBlank(match.Value));
                _bulletPoints = _bulletPoints.Union(from Match m in pattern.Matches(inputItem) select m.Value).ToList();
                output = output.Union(pattern.Split(inputItem).Where(match => !IsBlank(match))).ToList();
            }
            _stringsInPipeline = output;
        }

        /// <summary>
        /// Strips any punctuation from the strings in the pipeline, replacing apostrophies with blank
        /// characters so that words do loose meaning by seperating the endings during tokenization,
        /// while other symbols are replaced with spaces
        /// </summary>
        private void RemovePunc()
        {
            for (int i = 0; i < _stringsInPipeline.Count; ++i)
            {
                string termForRemoval = _stringsInPipeline[i];
                Regex pattern = new Regex(@"'");
                termForRemoval = pattern.Replace(termForRemoval, "");
                pattern = new Regex(@"[\p{P}\p{S}_\n]");
                termForRemoval = pattern.Replace(termForRemoval, " ");
                _stringsInPipeline[i] = termForRemoval;
            }
        }

        /// <summary>
        /// Converts all strings in the pipeline to lowercase, Normalization
        /// </summary>
        private void Normalize()
        {
            for (int i = 0; i < _stringsInPipeline.Count; ++i)
            {
                _stringsInPipeline[i] = _stringsInPipeline[i].ToLower();
            }
        }

        /// <summary>
        /// Generates a list of tokens from all strings in the pipeline with OpenNLP's tokenizer and 
        /// places wordcount in dictionary
        /// </summary>
        private void Tokenize()
        {
            foreach (string value in _stringsInPipeline)
            {
                _tokens.AddRange(value.Split(" "));
            }
            ResetTerms();
        }

        private void ResetTerms()
        {
            TermsAndStats = BagOfWords.WithWords(_tokens);
        }

        /// <summary>
        /// RemoveTokens removes words from a given token list from this instances token list
        /// </summary>
        /// <param name="tokens">Words to be removed from this instances tokens</param>
        public void RemoveTokens(List<string> tokens)
        {
            _tokens = _tokens.Except(tokens).ToList();
            ResetTerms();
        }

        public void RemoveTokensBOW(BagOfWords corpus)
        {
            List<string> newtokens = new List<string>();
            foreach(var token in _tokens)
            {
                if (corpus.Terms.ContainsKey(token))
                    newtokens.Add(token);
            }
            _tokens = newtokens;
            ResetTerms();
        }

        /// <summary>
        /// Creates NGrams of word length 2 - _ngramNum 
        /// </summary>
        public void MakeNGrams()
        {
            _NGrams.Clear();
            for(int ngramFirstWord = 0; ngramFirstWord < _tokens.Count - 1; ngramFirstWord++)
            {
                for (int ngramCombo = 1; 
                    ngramCombo < _ngramNum && ngramCombo + ngramFirstWord < _tokens.Count; ngramCombo++)
                {
                    string ngram = "";
                    for (int nextNgramWord = ngramFirstWord; 
                        nextNgramWord < ngramCombo + ngramFirstWord + 1 && nextNgramWord < Tokens.Count; 
                        nextNgramWord++)
                    {
                        ngram += _tokens[nextNgramWord] + NGRAM_DELIM;
                    }
                    ngram = ngram.Substring(0, ngram.Length - NGRAM_DELIM.Length);
                    _NGrams.Add(ngram);
                }
            }
            _tokens.AddRange(_NGrams);
            ResetTerms();
        }

        /// <summary>
        /// Adds the current tokens to keywords list
        /// </summary>
        public void AddTokensToKeywords()
        {
            _keywords.AddRange(_tokens);
        }

        /// <summary>
        /// Stem's purpose is stemming, for each word in the instances token list the words stem is
        /// found and all stem words indexes in the token list is tracked. Once a dictionary of word stems
        /// and indexes have been made, all tokens are replaced with their stems
        /// </summary>
        public void GetStemmedKeywords()
        {
            _keywords = (from string keyword in _keywords select Stemmer.Stem(keyword)).ToList();
            _tokens = new List<string>();
            _tokens.AddRange(_keywords);
            ResetTerms();
        }

        /// <summary>
        /// Utility function to see in a string is blank
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private bool IsBlank(string input)
        {
            return input == "" || input == "\n";
        }

        /*******************************************************************/
        /*************      Builder           ******************************/
        /*******************************************************************/

        /// <summary>
        /// Builders purpose is to generate a pipeline and construct/run the pipeline through its .Build function
        /// All over functions simply add their respective enumeration to the pipelist which is then input into
        /// the generated ProcessingPipeline object returned in the .Build function
        /// </summary>
        public class Builder
        {
            List<Pipes> _pipeList = new List<Pipes>();
            string _inputText;
            int ngrams = 0;

            /// <summary>
            /// Constructor which takes the text to be processed as input
            /// </summary>
            /// <param name="inputText"></param>
            public Builder(string inputText)
            {
                this._inputText = inputText;
            }

            /// <summary>
            /// Main build function, constructs a ProcessingPipeline, runs it, and returns it
            /// (this is the last function called when building)
            /// </summary>
            /// <returns></returns>
            public ProcessingPipeline Build()
            {
                ProcessingPipeline pipe = new ProcessingPipeline(_inputText);
                pipe._pipeList = new List<Pipes>(_pipeList);
                pipe._ngramNum = ngrams;
                pipe.Run();
                return pipe;
            }

            /// <summary>
            /// All following functions simply add the enum to the pipeline list
            /// </summary>

            public Builder SplitSentences()
            {
                _pipeList.Add(SENTENCE_SPLIT);
                return this;
            }

            public Builder SplitBulletPoints()
            {
                _pipeList.Add(SPLIT_BULLETS);
                return this;
            }

            public Builder RemovePunctuation()
            {
                _pipeList.Add(REMOVE_PUNCT);
                return this;
            }

            public Builder Normalize()
            {
                _pipeList.Add(NORMALIZE);
                return this;
            }

            public Builder Tokenize()
            {
                _pipeList.Add(TOKENIZE);
                return this;
            }
        }
    }
}
