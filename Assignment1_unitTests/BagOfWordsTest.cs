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
            Assert.AreEqual(3, bow.Terms[wordlist1[0]].TotalFreq);
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
        public void TestTermVectorSucc()
        { 
            bow.IndexWords();
            BitArray arr = new BitArray(new bool[] { true, false, true, true });
            var bowArr = bow.GetTermVector(wordlist1);
            for(int i = 0; i < arr.Length; ++i)
            {
                Assert.AreEqual(arr[i], bowArr[i]);
            }
        }

        [TestMethod]
        public void TestTermVectorNotIndexed()
        {
            var bowArr = bow.GetTermVector(wordlist1);
            Assert.IsNull(bowArr);
        }
    }
}

