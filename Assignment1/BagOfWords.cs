using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment1
{
    /// <summary>
    /// BagOfWords stores a sorted dictionary of seen terms in a collection using WordStats objects
    /// to keep track of document frequency (how many documents a term has been seen in) and the total
    /// term frequency seen.
    /// 
    /// BOW also contains methods for giving the words an index to its location in the dictionary
    /// and also creating a term vector
    /// </summary>
    public class BagOfWords
    {
        private SortedDictionary<string, WordStats> _terms = new SortedDictionary<string, WordStats>();
        public SortedDictionary<string, WordStats> Terms { get { return _terms; } }

        public bool Indexed { get; set; } = false;

        /// <summary>
        /// Adds each input term from given list to the bag of words by adding total frequency
        /// and adding unique appearence frequency (DocFreq)
        /// </summary>
        /// <param name="terms">The list of words to be added to bag of words</param>
        public void AddTerms(List<string> terms)
        {
            HashSet<string> uniqueTerms = new HashSet<string>();
            foreach(string term in terms)
            {
                uniqueTerms.Add(term);
                if (Terms.ContainsKey(term))
                {
                    Terms[term].TotalFreq++;
                }
                else
                {
                    Terms[term] = new WordStats();
                    Terms[term].TotalFreq = 1;
                }
            }
            foreach(string uniqueTerm in uniqueTerms)
            {
                Terms[uniqueTerm].DocFreq += 1;
            }
            Indexed = false;
        }

        public void RemoveTerms(List<string> terms)
        {
            foreach(var term in terms)
            {
                if (Terms.ContainsKey(term))
                    Terms.Remove(term);
            }
            IndexWords();
        }

        /// <summary>
        /// Sets the index location for each term in the dictionary 
        /// </summary>
        public void IndexWords()
        {
            for (int i = 0; i < Terms.Count; ++i)
                Terms.ElementAt(i).Value.Index = i;
            Indexed = true;
        }

        /// <summary>
        /// Creates a feature vector for a given input list of terms
        /// </summary>
        /// <param name="inputTerms"></param>
        /// <returns>Bit array term vector</returns>
        public BitArray GetTermVector(List<string> inputTerms)
        {
            if (!Indexed)
            {
                Console.WriteLine("Error BagOfWords::GetTermVector, attempt to get vector on unindexed"
                    + "BOW");
                return null;
            }
            BitArray result = new BitArray(Terms.Count, false);
            foreach (string term in inputTerms)
            {
                if (Terms.ContainsKey(term))
                {
                    result[Terms[term].Index] = true;
                }
            }
            return result;
        }
    }
}
