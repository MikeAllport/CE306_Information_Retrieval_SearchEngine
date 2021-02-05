using System;
using System.Collections.Generic;
using System.Text;

namespace Assignment1
{
    class MovieIndex
    {
        public int ReleaseYear  { get; set; }
        public string Title     { get; set; }
        public string Origin    { get; set; }
        public string Director  { get; set; }
        public string Cast      { get; set; }
        public string Genre     { get; set; }
        public string Wiki      { get; set; }
        public string Plot      { get; set; }

        public MovieIndex()
        {
            ReleaseYear = -1;
            Title = "";
            Origin = "";
            Director = "";
            Cast = "";
            Genre = "";
            Wiki = "";
            Plot = "";
        }
    }
}
