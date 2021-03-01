﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static Assignment1.ProcessingPipeline.Pipes;
using System.Linq;
using OpenNLP;
using System.Text.RegularExpressions;
using LemmaSharp;
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

        // member variables specific to each step, handy for debugging
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

        internal int _ngramNum;
        public int NGramNum { get { return _ngramNum; } set { _ngramNum = value; } }

        // some constants used for the OpenNLP model directories and ngram delimiter
        private static readonly string NGRAM_DELIM = ";;";

        private static readonly string MODEL_PATH = Assignment1.Program.SOLUTION_DIR +
            "libs/OpenNlp/Resources/Models/";
        private static readonly string SENTENCE_MODEL = MODEL_PATH + "EnglishSD.nbin";
        private static readonly string REGULAR_TOKENIZER = MODEL_PATH + "EnglishTok.nbin";


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
            var sentenceDetector = new OpenNLP.Tools.SentenceDetect.EnglishMaximumEntropySentenceDetector(SENTENCE_MODEL);
            foreach (string inputItem in _stringsInPipeline)
            {
                output = output.Union(sentenceDetector.SentenceDetect(inputItem)).ToList();
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
            foreach (string inputItem in _stringsInPipeline)
            {
                Regex pattern = new Regex(@"([\u2022][^\u2022\n$]*)|([0-9]{1,}\.\s[^0-9\n$]*)|([0-9]{1,}\s-\s[^0-9\n$]*)");
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
                _stringsInPipeline[i] = RemovePunc(_stringsInPipeline[i]);
            }
        }

        private string RemovePunc(string input)
        {
            Regex pattern = new Regex(@"'");
            input = pattern.Replace(input, "");
            pattern = new Regex(@"[\p{P}\p{S}]");
            input = pattern.Replace(input, " ");
            return input;
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
            var tokenizer = new OpenNLP.Tools.Tokenize.EnglishMaximumEntropyTokenizer(REGULAR_TOKENIZER);
            foreach (string value in _stringsInPipeline)
            {
                var s = tokenizer.Tokenize(value);
                foreach (string word in from str in tokenizer.Tokenize(value) select str)
                    _tokens.Add(word);
            }
            ResetTerms();
        }

        private void ResetTerms()
        {
            TermsAndStats = new BagOfWords();
            TermsAndStats.AddTerms(_tokens);
            TermsAndStats.IndexWords();
            TermsAndStats.AddNormalizedTermFreq();
        }

        /// <summary>
        /// RemoveTokens removes words from a given token list from this instances token list
        /// </summary>
        /// <param name="tokens">Words to be removed from this instances tokens</param>
        public void RemoveTokens(List<string> tokens)
        {
            _tokens = (from string t in _tokens where !tokens.Contains(t) select t).ToList();
            ResetTerms();
        }

        /// <summary>
        /// Creates NGrams of length _ngramNum 
        /// </summary>
        public void MakeNGrams()
        {
            List<string> ngrams = new List<string>();
            for(int ngramFirstWord = 0; ngramFirstWord + _ngramNum <= _tokens.Count; ngramFirstWord++)
            {
                string ngram = "";
                for (int nextNgramWord = ngramFirstWord; nextNgramWord < _ngramNum + ngramFirstWord; nextNgramWord++)
                {
                    ngram += _tokens[nextNgramWord] + NGRAM_DELIM;
                }
                ngram = ngram.Substring(0, ngram.Length - NGRAM_DELIM.Length);
                ngrams.Add(ngram);
            }
            _tokens.AddRange(ngrams);
            ResetTerms();
        }

        /// <summary>
        /// Stem's purpose is stemming, for each word in the instances token list the words stem is
        /// found and all stem words indexes in the token list is tracked. Once a dictionary of word stems
        /// and indexes have been made, all tokens are replaced with their stems
        /// </summary>
        public void Stem()
        {
            Dictionary<string, List<int>> wordsAndIndices = new Dictionary<string, List<int>>();
            _tokens = (from string token in _tokens select Stemmer.Stem(token)).ToList();
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
