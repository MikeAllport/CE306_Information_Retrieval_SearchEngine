using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assignment1;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Linq;

namespace Assignment1_unitTests
{
    [TestClass]
    public class TestMovieIndex
    {
        [TestMethod]
        public void TestSortedOrder()
        {
            MovieIndex index1 = new MovieIndex();
            index1.ReleaseYear = 2000;
            MovieIndex index2 = new MovieIndex();
            index2.ReleaseYear = 2001;
            SortedSet<MovieIndex> set = new SortedSet<MovieIndex>() { index1, index2 };
            List<MovieIndex> movieList = new List<MovieIndex>() { index2, index1 };
            var arr1 = new MovieIndex[set.Count];
            var arr2 = new MovieIndex[movieList.Count];
            set.CopyTo(arr1);
            movieList.CopyTo(arr2);
            CollectionAssert.AreEqual(arr1, arr2);
        }

        [TestMethod]
        public void TestEquality()
        {
            int id = 0;
            int year = 2001;
            string genre = "unknown";
            string director = "some director";
            MovieIndex index1 = new MovieIndex();
            MovieIndex index2 = new MovieIndex();
            index1.ID = id;
            index1.ReleaseYear = year;
            index1.Genre = genre;
            index1.Director = director;
            index2.ID = id;
            index2.ReleaseYear = year;
            index2.Genre = genre;
            index2.Director = director;
            var y = index2.GetHashCode(index2);
            SortedSet<MovieIndex> set = new SortedSet<MovieIndex>() { index1, index2 };
            Assert.IsTrue(index1.GetHashCode(index1).Equals(index2.GetHashCode(index2)));
            Assert.IsTrue(index1.Equals(index1, index2));
            Assert.IsTrue(set.Count == 1);
            Regex regex = new Regex(@"[ ]|(\+)");
            string[] test = regex.Split("this is a test +").Where(w => w != "").ToArray();
        }
    }
}
