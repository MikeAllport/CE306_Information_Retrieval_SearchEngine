using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment1Program
{
    class BagOfWords
    {
        private SortedDictionary<string, WordStats> _corpusTerms = new SortedDictionary<string, WordStats>();
        public SortedDictionary<string, WordStats> CorpusTerms { get { return _corpusTerms; } }


    }
}
