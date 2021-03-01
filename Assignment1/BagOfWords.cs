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
            Indexed = false;
            NormalizedTF = false;
        }

        public double[] GetDocNormTFIDFVector(BagOfWords documentsBOW)
        {
            if (!documentsBOW.NormalizedTF)
                documentsBOW.AssignTFNorms();
            if (!IDFed)
                throw new Exception("Error BagOfWords::GetGeneralizedVector, attempt to retrieve TFIDF on "
                    + "BOW without having assigned IDFs to corpus");
            return GetGeneralizedVector(documentsBOW, VectorType.DOCNORMTF_IDF);
        }

        /// <summary>
        /// Assigns the terms element location to each term in Terms, this is used for creating
        /// feature vectors to associate element location to term
        /// </summary>
        private void AssignIndexes()
        {
            for(int i = 0; i < Terms.Count; ++i)
            {
                Terms.ElementAt(i).Value.Index = i;
            }
            Indexed = true;
        }


        /// <summary>
        /// Assigns normalized term frequency to each word in Terms
        /// normalized is TF/NumTermsInDoc
        /// </summary>
        /// Knowledge for normalization gained from:
        /// https://janav.wordpress.com/2013/10/27/tf-idf-and-cosine-similarity/
        /// This makes it so TFIDF does not favour documents with high amount of words
        private void AssignTFNorms()
        {
            foreach(KeyValuePair<string, WordStats> termStatPair in Terms)
            {
                WordStats termStats = termStatPair.Value;
                termStats.NormalizedTF = termStats.TermFreq / (double)Terms.Count;
            }
            NormalizedTF = true;
        }

        /// <summary>
        /// Creates a feature vector based on a corpus's complete words (this instance) such that feature 
        /// vector will be of length corpusBOW.Terms.Length
        /// </summary>
        /// <param name="inputTerms">The document top be compared</param>
        /// <param name="type">Enum specifying which type of feature vector to return</param>
        /// <returns>double array term vector</returns>
        private double[] GetGeneralizedVector(BagOfWords documentBOW, VectorType type)
        {
            if (!Indexed)
                AssignIndexes();
            double[] result = new double[Terms.Count];
            for (int i = 0; i < Terms.Count; ++i)
            {
                string term = Terms.ElementAt(i).Key;
                if (documentBOW.Terms.ContainsKey(term))
                    switch (type)
                    {
                        case VectorType.IDF:
                            result[Terms[term].Index] = Terms[term].IDF;
                            break;
                        case VectorType.EXISTS:
                            result[Terms[term].Index] = 1.0;
                            break;
                        case VectorType.NORMTF:
                            result[Terms[term].Index] = documentBOW.Terms[term].NormalizedTF;
                            break;
                        case VectorType.DOCNORMTF_IDF:
                            result[Terms[term].Index] = Terms[term].IDF * documentBOW.Terms[term].NormalizedTF;
                            break;
                    }
            }
            return result;
        }
    }
}
