using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
{
    public static class VectorOps
    {
        public static double[] Multiplication(double[] arr1, double[] arr2)
        {
            return Enumerable.Range(0, arr1.GetLength(0))
                        .Select(x => arr1[x] * arr2[x])
                        .ToArray();
        }
    }
}
