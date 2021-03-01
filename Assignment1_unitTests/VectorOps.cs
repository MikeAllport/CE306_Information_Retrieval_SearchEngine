using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assignment1;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Linq;
using Utils;
using System.Collections;
using Utils;

namespace Assignment1_unitTests
{
    [TestClass]
    public class TestVectorOps
    {
        [TestMethod]
        public void GeneralTestVectorMultiplication()
        {
            double[] arr = { 2, 3, 4 };
            double[] expectedResult = { 4, 9, 16 };
            double[] test = VectorOps.Multiplication(arr, arr);
            Enumerable.Range(0, expectedResult.Length)
                .Select(x =>
                {
                    Assert.AreEqual(expectedResult[x], test[x]);
                    return x;
                });
        }
    }
}

