using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Assignment1
{
    class Stemmer
    {
        private static readonly string DATA_PATH = Assignment1.Program.SOLUTION_DIR +
            "libs/StemmingData/diffs.txt";
        private static Dictionary<string, string> Stems = InitStems();

        public static Dictionary<string, string> InitStems()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            var file = File.OpenText(DATA_PATH);
            string line;
            while((line = file.ReadLine()) != null)
            {
                var wordpair = Regex.Split(line, @"[\s]{1,}");
                result[wordpair[0]] = wordpair[1];
            }
            return result;
        }

        public static string Stem(string inputword)
        {
            if(Stems.ContainsKey(inputword))
            {
                return Stems[inputword];
            }
            return inputword;
        }
    }
}
