using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static Assignment1.ProcessingPipeline.Pipes;
using System.Linq;
using OpenNLP;
using System.Text.RegularExpressions;

namespace Assignment1
{
    public class ProcessingPipeline

    {
        public enum Pipes
        {
            SENTENCE_SPLIT,
            SPLIT_BULLETS,
            EXTRACT_ENTITIES,
            REMOVE_PUNCT,
            NORMALIZE,
            TOKENIZE,
        }

        internal List<Pipes> _pipeList;
        public string OriginalText { get; }
        public SortedSet<string> KeyWords { get; } = new SortedSet<string>();
        public SortedSet<string> Phrases { get; } = new SortedSet<string>();
        private List<string> _sentences = new List<string>();
        public List<string> Sentences { get { return _sentences; } }
        private List<string> _bulletPoints = new List<string>();
        public List<string> BulletPoints { get { return _bulletPoints; } }
        private List<string> _stringsInPipeline = new List<string>();
        public List<string> StringsInPipeline { get { return _stringsInPipeline; } }
        private Dictionary<string, SortedSet<string>> _entities = new Dictionary<string, SortedSet<string>>()
        {
            { "dates", new SortedSet<string>() },
            { "persons", new SortedSet<string>() },
            { "locations", new SortedSet<string>() },
            { "moneys", new SortedSet<string>() },
            { "organizations", new SortedSet<string>() },
            { "times", new SortedSet<string>() }
        };
        public Dictionary<string, SortedSet<string>> Entities { get { return _entities; } }
        private static readonly string MODEL_PATH = Assignment1.Program.SOLUTION_DIR +
            "libs/OpenNlp/Resources/Models/";
        private static readonly string SENTENCE_MODEL = MODEL_PATH + "EnglishSD.nbin";
        private static readonly string ENTITY_PATH = MODEL_PATH + "NameFind/";

        public ProcessingPipeline(string inputText)
        {
            OriginalText = inputText;
        }

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
                    case EXTRACT_ENTITIES:
                        ExtractEntities();
                        break;
                    case REMOVE_PUNCT:
                        RemovePunc();
                        break;
                    case NORMALIZE:
                        Normalize();
                        break;
                    case TOKENIZE:
                        break;
                }
            }
        }

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

        private void SplitBulletPoints()
        {
            List<string> output = new List<string>();
            foreach (string inputItem in _stringsInPipeline)
            {
                Regex pattern = new Regex("([\u2022][^\u2022\n$]*)|([0-9].[^0-9\n$]*)|([0-9]-[^0-9\n$]*)");
                var hits = pattern.Matches(inputItem).Where(match => !IsBlank(match.Value));
                _bulletPoints = _bulletPoints.Union(from Match m in pattern.Matches(inputItem) select m.Value).ToList();
                output = output.Union(pattern.Split(inputItem).Where(match => !IsBlank(match))).ToList();
            }
            _stringsInPipeline = output;
        }

        private void ExtractEntities()
        {
            List<string> output = new List<string>();
            var nameFinder = new OpenNLP.Tools.NameFind.EnglishNameFinder(ENTITY_PATH);
            // specify which types of entities you want to detect
            var models = new string[]{ "date", "location", "money", "organization", "percentage", "person", "time" };
            foreach (string input in _stringsInPipeline)
            {
                List<string> matches = new List<string>();
                var hits = nameFinder.GetNames(models, input);
                Regex pattern = new Regex("(?<=<date>).*?(?=</date>)");
                _entities["dates"].UnionWith(from Match m in pattern.Matches(hits) select m.Value);
                pattern = new Regex("(?<=<person>).*?(?=</person>)");
                _entities["persons"].UnionWith(from Match m in pattern.Matches(hits) select m.Value);
                pattern = new Regex("(?<=<location>).*?(?=</location>)");
                _entities["locations"].UnionWith(from Match m in pattern.Matches(hits) select m.Value);
                pattern = new Regex("(?<=<organization>).*?(?=</organization>)");
                _entities["organizations"].UnionWith(from Match m in pattern.Matches(hits) select m.Value);
                pattern = new Regex("(?<=<moneys>).*?(?=</moneys>)");
                _entities["moneys"].UnionWith(from Match m in pattern.Matches(hits) select m.Value);
                pattern = new Regex("(?<=<times>).*?(?=</times>)");
                _entities["times"].UnionWith(from Match m in pattern.Matches(hits) select m.Value);
            }
        }

        private void RemovePunc()
        {
            for (int i = 0; i < _stringsInPipeline.Count; ++i)
            {
                Regex pattern = new Regex("['`]");
                _stringsInPipeline[i] = pattern.Replace(_stringsInPipeline[i], "");
            }
        }

        private void Normalize()
        {
            for (int i = 0; i < _stringsInPipeline.Count; ++i)
            {
                _stringsInPipeline[i] = _stringsInPipeline[i].ToLower();
            }
        }

        private bool IsBlank(string input)
        {
            return input == "" || input == "\n";
        }


        public class Builder
        {
            List<Pipes> _pipeList = new List<Pipes>();
            string _inputText;

            public Builder(string inputText)
            {
                this._inputText = inputText;
            }

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

            public Builder ExtractEntities()
            {
                _pipeList.Add(EXTRACT_ENTITIES);
                return this;
            }

            public ProcessingPipeline Build()
            {
                ProcessingPipeline pipe = new ProcessingPipeline(_inputText);
                pipe._pipeList = new List<Pipes>(_pipeList);
                pipe.Run();
                return pipe;
            }
        }
    }
}
