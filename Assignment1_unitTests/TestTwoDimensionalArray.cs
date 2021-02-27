using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assignment1;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Linq;
using Utils;

namespace Assignment1_unitTests
{
    [TestClass]
    class TestTwoDimensionalArray
    {
        [TestMethod]
        public void TestDataChangeRow()
        {
            TwoDimensionalArray<int> array = new TwoDimensionalArray<int>(3, 3);
            for (int i = 0; i < 3; ++i)
                for (int j = 0; j < 3; ++j)
                    array[i, j] = j + i * 3;
            int[] arrCpy = array.GetRow(2);
            for (int i = 0; i < 3; ++i)
            {
                arrCpy[i] = 0;
            }
            array.SetRow(arrCpy, 2);
            for (int i = 0; i < 3; ++i)
            {
                Assert.AreEqual(0, array[2, i]);
            }
            for (int i = 0; i < 3; ++i)
            {
                array[1, i] = 1;
                Assert.AreEqual(1, array[1, i]);
            }
        }

        [TestMethod]
        public void TestDataChangeColumn()
        {
            TwoDimensionalArray<int> array = new TwoDimensionalArray<int>(3, 3);
            for (int i = 0; i < 3; ++i)
                for (int j = 0; j < 3; ++j)
                    array[i, j] = j + i * 3;
            int[] arrCpy = array.GetColumn(2);
            for (int i = 0; i < 3; ++i)
            {
                arrCpy[i] = 0;
            }
            array.SetColumn(arrCpy, 2);
            for (int i = 0; i < 3; ++i)
            {
                Assert.AreEqual(0, array[i, 2]);
            }
            for (int i = 0; i < 3; ++i)
            {
                array[i, 1] = 1;
                Assert.AreEqual(1, array[i, 1]);
            }
        }
    }
}
