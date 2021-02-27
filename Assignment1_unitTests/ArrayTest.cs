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
    public class TestArrays
    {
        [TestMethod]
        public void TestDataChange()
        {
            TwoDimensionalArray<int> array = new TwoDimensionalArray<int>(3, 3);
            for (int i = 0; i < 3; ++i)
                for (int j = 0; j < 3; ++j)
                    array[i, j] = j + i * 3;
            int[] arrCpy = array.GetRow(2);

        }
    }
}
