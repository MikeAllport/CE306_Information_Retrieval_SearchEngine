using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment1
{
    public class WordStats
    {
        public int Index { get; set; } = 0;
        public int DocFreq { get; set; } = 0;
        public int TermFreq { get; set; } = 0;
        public double NormalizedTF { get; set; } = 0;
        public double TFIDF { get; set; } = 0;
        public double IDF { get; set; } = 0;
    }
}
