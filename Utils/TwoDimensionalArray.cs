using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
{
    public class TwoDimensionalArray<T>
    {
        private T[,] array;

        public T this[int y, int x] { get { return array[y, x]; } set { this.array[y, x] = value; } }
        public TwoDimensionalArray(int sizey, int sizex)
        {
            this.array = new T[sizey, sizex];
        }

        // Following two functions have been adapted from:
        // https://stackoverflow.com/questions/27427527/how-to-get-a-complete-row-or-column-from-2d-array-in-c-sharp
        public T[] GetColumn(int columnNumber)
        {
            return Enumerable.Range(0, array.GetLength(0))
                    .Select(x => array[x, columnNumber])
                    .ToArray();
        }

        public T[] GetRow(int rowNumber)
        {
            return Enumerable.Range(0, array.GetLength(1))
                    .Select(x => array[rowNumber, x])
                    .ToArray();
        }
        public void SetRow(T[] row, int rowNum)
        {
            for(int i = 0; i < array.Length; i++)
            {
                array[rowNum, i] = row[i];
            }
        }

        public void SetColumn(T[] column, int columnNumber)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i, columnNumber] = column[i];
            }
        }
    }
}
