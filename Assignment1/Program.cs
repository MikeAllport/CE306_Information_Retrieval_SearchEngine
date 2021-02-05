using System;
using System.IO;


namespace Assignment1
{
    class Program
    {
        private static readonly string DATA_ABS_PATH =
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\..\\documents.csv"));
        static void Main(string[] args)
        {
            DataMunger munger = new DataMunger(DATA_ABS_PATH);
        }
    }
}
