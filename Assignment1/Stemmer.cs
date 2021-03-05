using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Assignment1
{
    /// <summary>
    /// Stemmer is a static class who's purpose is to load given stemming dataset as
    /// pre-stemmed / stemmed term pairs in a dictionary. Stem method then finds an input
    /// word as key in the dictionary, and returns the stemmed equivelent if found
    /// </summary>
    class Stemmer
    {
        private static readonly string DATA_PATH = Assignment1.Program.SOLUTION_DIR +
            "libs/StemmingData/diffs.txt";
        private static Dictionary<string, string> Stems = InitStems();

        // instantiates the dataset in dictionary by parsing with regex
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

        /// <summary>
        /// Produces the associated stemmed word of a given input word if exists
        /// </summary>
        /// <param name="inputword">Term to be evaluated</param>
        /// <returns>Stemmed version if exists, inputword otherwise</returns>
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
