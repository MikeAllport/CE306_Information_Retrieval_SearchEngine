using System;
using System.Collections.Generic;
using System.Text;

namespace Assignment1
{
    class MovieIndex: ICSVEntity
    {
        public int ReleaseYear  { get; set; }
        public string Title     { get; set; }
        public string Origin    { get; set; }
        public string Director  { get; set; }
        public string Cast      { get; set; }
        public string Genre     { get; set; }
        public string Wiki      { get; set; }
        public string Plot      { get; set; }
        public string FullText { get; set; }

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

        public void AddValue(int column, string value, int lineNum)
        {
            switch(column)
            {
                case 0:
                    ReleaseYear = int.Parse(value);
                    break;
                case 1:
                    Title = value;
                    break;
                case 2:
                    Origin = value;
                    break;
                case 3:
                    Director = value;
                    break;
                case 4:
                    Cast = value;
                    break;
                case 5:
                    Genre = value;
                    break;
                case 6:
                    Wiki = value;
                    break;
                case 7:
                    Plot = value;
                    break;
            }
        }

        public void AddFullText(string text)
        {
        }
    }
}
