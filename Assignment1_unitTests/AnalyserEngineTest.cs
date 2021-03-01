using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assignment1;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Linq;
using Utils;
using System.Collections;

namespace Assignment1_unitTests
{
    [TestClass]
    public class TestAnalyserEngine
    {
        static Program prog = new Program(null);
        static BagOfWords bow = new BagOfWords();
        // documents
        static List<string> wordlist1 = new List<string>() { "hey", "there", "hey", "1" };
        static List<string> wordlist2 = new List<string>() { "hey", "2" };
        static List<string> wordlist3 = new List<string>() { "this", "should", "not", "match", "doc1" };
        static List<string> wordlist4 = new List<string>();
        static BagOfWords document; // will be a document with worldlist1 terms
        static BagOfWords doc2; // will be a document with wordlist3 terms
        static BagOfWords doc3; // will be a document with worldlist3 + worldlist2 terms
        //query
        static List<string> query = new List<string>() { "hey", "there" };
        static BagOfWords querDoc;
        static AnalyserEngine engine;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            prog.PerformFullIndexing(Program.DEFAULT_DATA_FILE, 50);
            prog.PerformTokenization();
            querDoc = BagOfWords.WithWords(query);
            bow.AddTerms(wordlist1);
            bow.AddTerms(wordlist2);
            string cast = "";
            for (int i = 0; i < wordlist1.Count; ++i)
            {
                cast += wordlist1[i] + " ";
                if (i < wordlist2.Count)
                    cast += wordlist2[i] + " ";
            }
            MovieIndex index = new MovieIndex();
            index.Cast = cast;
            cast = "";
            for (int i = 0; i < wordlist3.Count; ++i)
            {
                cast += wordlist3[i] + " ";
            }
            var index2 = new MovieIndex();
            index2.Cast = cast;
            var index3 = new MovieIndex(index2);
            wordlist4.AddRange(wordlist3);
            wordlist4.AddRange(wordlist2);
            index3.Plot += " hey 2";
            Dictionary<int, MovieIndex> dict = new Dictionary<int, MovieIndex>() { { 1, index }, { 2, index2 }, { 3, index3 } };
            engine = new AnalyserEngine(dict, null, new AnalyserEngineSettings());
            engine.GenerateTokenizatedPipes();
            engine.GeneratePhrases();
            engine.CalculateIDFs();
            document = BagOfWords.WithWords(wordlist1);
            doc2 = BagOfWords.WithWords(wordlist3);
            doc3 = BagOfWords.WithWords(wordlist4);
        }

        [TestMethod]
        public void CosineSimilarityExact()
        {
            double similarity = engine.CosineSimilarity(querDoc, querDoc);
            Assert.AreEqual(1, similarity);
        }

        [TestMethod]
        public void CosineSimilarityNoMatch()
        {
            double similarity = engine.CosineSimilarity(querDoc, doc2);
            Assert.AreEqual(0, similarity);
        }

        [TestMethod]
        public void CosineSimilarityHighMatch()
        {
            double similarity = engine.CosineSimilarity(querDoc, document);
            Assert.IsTrue(similarity > 0.8);
        }

        [TestMethod]
        public void CosineSimilaritLowMatch()
        {
            double similarity = engine.CosineSimilarity(querDoc, doc3);
            Assert.IsTrue(similarity > 0);
        }

        //[TestMethod]
        public void TestIDFSelection()
        {
            prog.AnalyserEngine.RemoveStopWords();
            prog.AnalyserEngine.GeneratePhrases();
            prog.AnalyserEngine.RemoveVeryInfrequentWords();
            prog.AnalyserEngine.CalculateIDFs();
        }
    }
}