using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

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
        public bool IDFed { get; set; } = false;

        private SortedList<string, WordStats> listTerms = null;

        public static BagOfWords WithWords(List<string> words)
        {
            BagOfWords result = new BagOfWords();
            result.AddTerms(words);
            return result;
        }

        /// <summary>
        /// Adds each input term from given list to the bag of words by adding total frequency
        /// and adding unique appearence frequency (DocFreq)
        /// </summary>
        /// <param name="terms">The list of words to be added to bag of words</param>
        public void AddTerms(List<string> terms)
        {
            var uniqueTerms = terms.Distinct().ToList();
            foreach(string term in terms)
            {
                if (Terms.ContainsKey(term))
                {
                    Terms[term].TermFreq++;
                }
                else
                {
                    Terms[term] = new WordStats();
                    Terms[term].TermFreq = 1;
                }
            }
            foreach(string uniqueTerm in uniqueTerms)
            {
                Terms[uniqueTerm].DocFreq += 1;
            }
            IDFed = false;
        }

        /// <summary>
        /// Removes all given terms from the BOW's Term dictionary
        /// </summary>
        /// <param name="terms">List of words to be removed</param>
        public void RemoveTerms(List<string> terms)
        {
            foreach(var term in terms)
            {
                if (Terms.ContainsKey(term))
                    Terms.Remove(term);
            }
        }

        /// <summary>
        /// Retrives TFIDF feature vector of given documents BOW object
        /// </summary>
        /// <param name="documentsBOW">The document, or query, to get TFIDF of</param>
        /// <returns>TFIDF feature vector</returns>
        public double[] GetDocNormTFIDFVector(BagOfWords documentsBOW)
        {
            if (!IDFed)
                throw new Exception("Error BagOfWords::GetGeneralizedVector, attempt to retrieve TFIDF on "
                    + "BOW without having assigned IDFs to corpus");
            // instantiate sortedlist for easy lookup of term indices
            if (this.listTerms == null || this.listTerms.Count != Terms.Count)
                this.listTerms = new SortedList<string, WordStats>(Terms);
            // instantiate term feature vectors
            double[] docsNormalizedTfVector = new double[Terms.Count];
            double[] corpusIDFVector = new double[Terms.Count];
            // assign terms TF and IDF values in feature vectors if exists in corpus
            foreach(var termStatsPair in documentsBOW.Terms)
            {
                int termIndex = listTerms.IndexOfKey(termStatsPair.Key);
                if (termIndex >= 0)
                {
                    docsNormalizedTfVector[termIndex] = termStatsPair.Value.TermFreq;
                    corpusIDFVector[termIndex] = Terms[termStatsPair.Key].IDF;
                }
            }
            // return TF * IDF of feature vectors
            return VectorOps.Multiplication(docsNormalizedTfVector, corpusIDFVector);
        }
    }
}
