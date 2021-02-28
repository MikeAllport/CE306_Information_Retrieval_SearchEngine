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
    public class TestBOW
    {
        BagOfWords bow = new BagOfWords();
        List<string> wordlist1 = new List<string>() { "hey", "there", "hey", "1" };
        List<string> wordlist2 = new List<string>() { "hey", "2" };
        public TestBOW()
        {
            bow.AddTerms(wordlist1);
            bow.AddTerms(wordlist2);
        }

        [TestMethod]
        public void TestAddTerms()
        {
            Assert.AreEqual(2, bow.Terms[wordlist1[0]].DocFreq);
            Assert.AreEqual(3, bow.Terms[wordlist1[0]].TermFreq);
            Assert.AreEqual(false, bow.Indexed);
        }

        [TestMethod]
        public void TestIndexed()
        {
            bow.IndexWords();
            Assert.AreEqual(0, bow.Terms["1"].Index);
            Assert.AreEqual(2, bow.Terms["hey"].Index);
        }

        [TestMethod]
        public void TestTermVectorExistsSucc()
        { 
            bow.IndexWords();
            double[] arr = new double[] { 0.0, 1.0, 1.0, 0.0 };
            var bowArr = bow.GetTermVector(BagOfWords.WithWords(wordlist2));
            for(int i = 0; i < arr.Length; ++i)
            {
                Assert.AreEqual(arr[i], bowArr[i]);
            }
        }

        [TestMethod]
        public void TestTermVectorNotIndexed()
        {
            var bowArr = bow.GetTermVector(BagOfWords.WithWords(wordlist1));
            Assert.IsNull(bowArr);
        }

        [TestMethod]
        public void TestTermVectorNormalizedTF()
        {
            bow.IndexWords();
            bow.AddNormalizedTermFreq();
            double[] arr = new double[] { 1/4.0, 0.0, 3/4.0, 1/4.0 };
            var bowArr = bow.GetNormalizedTFVector(BagOfWords.WithWords(wordlist1));
            for (int i = 0; i < arr.Length; ++i)
            {
                Assert.AreEqual(arr[i], bowArr[i]);
            }
        }

        [TestMethod]
        public void GeneralTestVectorMultiplication()
        {
            double[] arr = { 2, 3, 4 };
            double[] expectedResult = { 4, 9, 16 };
            double[] test = arr * arr;
        }
    }
}

