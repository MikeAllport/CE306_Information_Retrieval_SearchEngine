using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assignment1;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Linq;

namespace Assignment1_unitTests
{
    [TestClass]
    public class ProcessingPipelineTest
    {
        string sentences = "Zafer, a sailor living with his mother DÃ¶ndÃ¼ in a coastal village in Izmir, has just separated from his girlfriend Mehtap whose father is also a sailor. While DÃ¶ndÃ¼ and her friend, Fahriye try to help Zafer to marry someone and have his own family, a famous and talented actress, AslÄ± surprisingly attends Zafer's boat tour. Then Asli and Zafer find themselves getting to know each other.\n" +
            "Irina and Raymond, the pianist and the composer, are only bruised, and their company is able to continue with their performance, albeit in bandages.\n" +
            "There is a happy ending for driver Jim. The final scene shows him waving goodbye to his wife, as he prepares to cycle across to the locomotive sheds on his first day in that nine - to - six job.\"";
        int sentenceCount = 6;
        [TestMethod]
        public void TestSentenceSplitter()
        {
            ProcessingPipeline pipe = new ProcessingPipeline.Builder(this.sentences).
                SplitSentences().
                Build();
            Assert.AreEqual(6, pipe.Sentences.Count);
        }

        [TestMethod]
        public void TestBulletPoints()
        {
            string input = "1. hello 2. this\n3. Is da bomb\n1 - if 2 - pipeline\n3 - works with\n• another • bullet\n• here";
            ProcessingPipeline pipe = new ProcessingPipeline.Builder(sentences + input).
                SplitBulletPoints().
                Build();
            Assert.AreEqual(9, pipe.BulletPoints.Count);
            Assert.AreEqual(9 + sentenceCount, pipe.StringsInPipeline.Count);
        }

        [TestMethod]
        public void TestRemovePunc()
        {
            string input = "i'll be there for you, maybe I won't, he'll do just fine";
            ProcessingPipeline pipe = new ProcessingPipeline.Builder(input).
                RemovePunctuation().
                Build();
            Assert.IsTrue(!pipe.StringsInPipeline[0].Contains("'"));
        }

        [TestMethod]
        public void TestNormalize()
        {
            string input = "Ill be There for YOUUUUUU";
            ProcessingPipeline pipe = new ProcessingPipeline.Builder(input).
                Normalize().
                Build();
            Regex pattern = new Regex("[A-Z]");
            var matches = from Match m in pattern.Matches(pipe.StringsInPipeline[0]) select m.Value;
            Assert.AreEqual(0, matches.Count());
        }

        [TestMethod]
        public void TestTokenization()
        {
            string input = "- oh what a wonderful life it has been been, Dr. Emerates found out. Whilst we were away.";
            ProcessingPipeline pipe = new ProcessingPipeline.Builder(input).
                SplitSentences().
                Tokenize().
                Build();
            Assert.AreEqual(21, pipe.Tokens.Count);
        }
    }
}
