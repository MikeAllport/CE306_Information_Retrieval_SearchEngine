using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nest;

namespace Assignment1
{
    [JsonConverter(typeof(MovieIndex))]
    public class MovieIndex: 
        JsonConverter,
        ICSVEntity, 
        IComparable<MovieIndex>, 
        IEqualityComparer<MovieIndex>
    {
        // json attribute name constants
        private const string ID_KEY = "ID",
            RELEASEY_KEY = "ReleaseYear",
            TITLE_KEY = "Title",
            ORIGIN_KEY = "Origin",
            DIRECTOR_KEY = "Director",
            CAST_KEY = "Cast",
            GENRE_KEY = "Genre",
            WIKI_KEY = "Wiki",
            PLOT_KEY = "Plot";
        public int Complexity = 0;
        [Number(Name = ID_KEY)]
        public int ID { get; set; } = -1;
        [Number(Name = RELEASEY_KEY)]
        public int ReleaseYear { get; set; } = -1;
        [Text(Name = TITLE_KEY)]
        public string Title { get; set; } = "";
        [Text(Name = ORIGIN_KEY)]
        public string Origin { get; set; } = "";
        [Text(Name = DIRECTOR_KEY)]
        public string Director { get; set; } = "";
        [Text(Name = CAST_KEY)]
        public string Cast { get; set; } = "";
        [Text(Name = GENRE_KEY)]
        public string Genre { get; set; } = "";
        [Text(Name = WIKI_KEY)]
        public string Wiki { get; set; } = "";
        [Text(Name = PLOT_KEY)]
        public string Plot { get; set; } = "";

        public MovieIndex(MovieIndex other)
        {
            this.ID = other.ID;
            this.ReleaseYear = other.ReleaseYear;
            this.Title = other.Title;
            this.Origin = other.Origin;
            this.Director = other.Director;
            this.Cast = other.Cast;
            this.Genre = other.Genre;
            this.Wiki = other.Wiki;
            this.Plot = other.Plot;
        }

        public MovieIndex() { }

        public string GetFullText()
        {
            return $"{ReleaseYear} {Title} {Origin} {Director} {Cast} {Genre} {Wiki} {Plot}";
        }

        /// <summary>
        /// this method requires no action, as the full text class is not
        /// here to store data from fields
        /// </summary>
        public void AddValue(int column, string value, int lineNum)
        {
            switch (column)
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

        public bool Equals([AllowNull] MovieIndex x, [AllowNull] MovieIndex y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return Equals(x.ID, y.ID) && Equals(x.ReleaseYear, y.ReleaseYear) &&
                Equals(x.Title, y.Title) && Equals(x.Origin, y.Origin) &&
                Equals(x.Director, y.Director) && Equals(x.Cast, y.Cast) &&
                Equals(x.Genre, y.Genre) && Equals(x.Wiki, y.Wiki) &&
                Equals(x.Plot, y.Plot);
        }

        public int Compare([DisallowNull] MovieIndex x, [DisallowNull] MovieIndex y)
        {
            return y.ReleaseYear - x.ReleaseYear;
        }


        /* Adapted from: https://stackoverflow.com/questions/59375124/how-to-use-system-hashcode-combine-with-more-than-8-values */
        /// <summary>
        /// GetHashCode overrides default hash code method and returns a unique hash related to this object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode([DisallowNull] MovieIndex obj)
        {
            HashCode hash = new HashCode();
            hash.Add(obj.ID); hash.Add(obj.ReleaseYear); hash.Add(obj.Title);
            hash.Add(obj.Origin); hash.Add(obj.Director); hash.Add(obj.Cast);
            hash.Add(obj.Genre); hash.Add(obj.Wiki); hash.Add(obj.Plot);
            return hash.ToHashCode();
        }

        /// <summary>
        /// Makes sorting of MovieEndexes in descending order of complexity or release year
        /// if complexisties equal
        /// </summary>
        public int CompareTo([AllowNull] MovieIndex other)
        {
            if (other.Complexity != this.Complexity)
                return other.Complexity - this.Complexity;
            return other.ReleaseYear - this.ReleaseYear;
        }

        // Json serialization methods
        //serialise
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var index = value as MovieIndex;
            if (value == null)
                return;
            writer.WriteStartObject();
            SetWriter(index, writer, serializer);
            writer.WriteEndObject();
        }

        protected virtual void SetWriter(MovieIndex index, JsonWriter writer, JsonSerializer serializer)
        {
            writer.WritePropertyName(ID_KEY);
            serializer.Serialize(writer, index.ID.ToString());
            writer.WritePropertyName(RELEASEY_KEY);
            serializer.Serialize(writer, index.ReleaseYear.ToString());
            writer.WritePropertyName(TITLE_KEY);
            serializer.Serialize(writer, index.Title);
            writer.WritePropertyName(ORIGIN_KEY);
            serializer.Serialize(writer, index.Origin);
            writer.WritePropertyName(DIRECTOR_KEY);
            serializer.Serialize(writer, index.Director);
            writer.WritePropertyName(CAST_KEY);
            serializer.Serialize(writer, index.Cast);
            writer.WritePropertyName(GENRE_KEY);
            serializer.Serialize(writer, index.Genre);
            writer.WritePropertyName(WIKI_KEY);
            serializer.Serialize(writer, index.Wiki);
            writer.WritePropertyName(PLOT_KEY);
            serializer.Serialize(writer, index.Plot);
        }

        //deserialize
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var index = new MovieIndex();
            JObject jsonObj = JObject.Load(reader);
            SetReader(index, jsonObj);
            return index;
        }

        protected virtual void SetReader(MovieIndex index, JObject jsonObj)
        {
            var id = jsonObj[ID_KEY].Value<int>();
            var releaseYear = jsonObj[RELEASEY_KEY].Value<int>();
            var title = jsonObj[TITLE_KEY].Value<string>();
            var origin = jsonObj[ORIGIN_KEY].Value<string>();
            var director = jsonObj[DIRECTOR_KEY].Value<string>();
            var cast = jsonObj[CAST_KEY].Value<string>();
            var genre = jsonObj[GENRE_KEY].Value<string>();
            var wiki = jsonObj[WIKI_KEY].Value<string>();
            var plot = jsonObj[PLOT_KEY].Value<string>();
            index.ID = id;
            index.ReleaseYear = releaseYear;
            index.Title = title;
            index.Origin = origin;
            index.Director = director;
            index.Cast = cast;
            index.Genre = genre;
            index.Wiki = wiki;
            index.Plot = plot;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(MovieIndex).IsAssignableFrom(objectType);
        }

        // simple serialization/deserialization methods for ease of access
        public virtual string Serialize()
        {
            string serialization = JsonConvert.SerializeObject(this, Formatting.Indented);
            return serialization;
        }

        public virtual MovieIndex Deserialize(string jsonSerialized)
        {
            try
            {
                return JsonConvert.DeserializeObject<MovieIndex>(jsonSerialized);
            }
            catch (Exception) { return null; }
        }
    }
}
