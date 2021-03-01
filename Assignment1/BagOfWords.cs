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
        enum VectorType
        {
            IDF,
            NORMTF,
            EXISTS,
            DOCNORMTF_IDF
        }
        private SortedDictionary<string, WordStats> _terms = new SortedDictionary<string, WordStats>();
        public SortedDictionary<string, WordStats> Terms { get { return _terms; } }

        public bool Indexed { get; set; } = false;
        public bool NormalizedTF { get; set; } = false;
        public bool IDFed { get; set; } = false;

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
            Indexed = false;
            NormalizedTF = false;
            IDFed = false;
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
        /// Creates a feature vector of 1's and 0s if term exists in bow
        /// </summary>
        /// <param name="inputTerms">input words to get feature from</param>
        /// <returns>double array term vector</returns>
        public double[] GetTermVector(BagOfWords document)
        {
            return GetGeneralizedVector(document, VectorType.EXISTS);
        }

        /// <summary>
        /// Creates a feature vector of IDF's
        /// </summary>
        /// <param name="inputTerms">input words to get feature from</param>
        /// <returns>double array term vector</returns>
        public double[] GetIDFTermVector(BagOfWords document)
        {
            return GetGeneralizedVector(document, VectorType.IDF);
        }

        /// <summary>
        /// Creates a feature vector Normalized term frequencies
        /// </summary>
        /// <param name="inputTerms">input words to get feature from</param>
        /// <returns>double array term vector</returns>
        public double[] GetNormalizedTFVector(BagOfWords document)
        {
            if (!NormalizedTF)
            {
                throw new Exception("Error BagOfWords::GetNormalizedTFVector, attempt to get vector on"
                    + "bow without normalizing terms");
            }
            return GetGeneralizedVector(document, VectorType.NORMTF);
        }

        public double[] GetDocNormTFTimesIDFVector(BagOfWords document)
        {
            if (!IDFed || !document.NormalizedTF)
            {
                throw new Exception("Error BagOfWords::GetDocNormTimeIDFVector, atttempt to get feature" +
                    " vector on IDFs when IDFs not set or document TF not normalized");
            }
            return GetGeneralizedVector(document, VectorType.DOCNORMTF_IDF);
        }

        /// <summary>
        /// Creates a feature vector for a given type of features dependent upon
        /// VectorType enum
        /// </summary>
        /// <param name="inputTerms">input words to get feature from</param>
        /// <param name="type">Enum specifying which type of feature vector to return</param>
        /// <returns>double array term vector</returns>
        private double[] GetGeneralizedVector(BagOfWords inputBOW, VectorType type)
        {
            if (NotIndexed())
                return null;
            double[] result = new double[Terms.Count];
            for (int i = 0; i < inputBOW.Terms.Count; ++i)
            {
                string term = inputBOW.Terms.ElementAt(i).Key;
                if (Terms.ContainsKey(term))
                    switch(type)
                    {
                        case VectorType.IDF:
                            result[Terms[term].Index] = Terms[term].IDF;
                            break;
                        case VectorType.EXISTS:
                            result[Terms[term].Index] = 1.0;
                            break;
                        case VectorType.NORMTF:
                            result[Terms[term].Index] = Terms[term].NormalizedTermFreq;
                            break;
                        case VectorType.DOCNORMTF_IDF:
                            result[Terms[term].Index] = Terms[term].IDF * inputBOW.Terms[term].NormalizedTermFreq;
                            break;
                    }
            }
            return result;
        }

        private bool NotIndexed()
        {
            if (!Indexed)
            {
                throw new Exception("Error BagOfWords, attempt to get vector on unindexed"
                    + "BOW");
            }
            return false;
        }

        public void AddNormalizedTermFreq()
        {
            foreach (var term in Terms)
            {
                Terms[term.Key].NormalizedTermFreq = Terms[term.Key].TermFreq / (float)Terms.Count;
            }
            NormalizedTF = true;
        }
    }
}
