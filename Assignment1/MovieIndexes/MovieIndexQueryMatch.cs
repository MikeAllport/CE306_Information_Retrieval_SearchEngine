using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nest;
using System.Diagnostics.CodeAnalysis;

namespace Assignment1
{
    [JsonConverter(typeof(MovieIndexQueryMatch))]
    public class MovieIndexQueryMatch : 
        MovieIndexKeyWords,
        IComparable<MovieIndexQueryMatch>,
        IEqualityComparer<MovieIndexQueryMatch>
    {
        // json attribute name constants
        protected const string SIMILARITY_KEY = "SimilarityScore",
            FIELDMATCHED_KEY = "FieldMatched",
            MATCHNUM_KEY = "MatchRank";

        [PropertyName(SIMILARITY_KEY)]
        public double SimilarityScore{ get; set; } = 0;

        [PropertyName(FIELDMATCHED_KEY)]
        public bool FieldMatched { get; set; } = false;

        public bool MatchFieldQuery = false;

        [PropertyName(MATCHNUM_KEY)]
        public int ID { get; set; } = 0;

        public MovieIndexQueryMatch(double similarity, MovieIndexKeyWords other, bool matchField = false, bool fieldMatched = true) : base(other) {
            this.SimilarityScore = similarity;
            this.FieldMatched = fieldMatched;
            this.MatchFieldQuery = matchField;
            this.FieldMatched = fieldMatched;
        }

        public MovieIndexQueryMatch() : base() { }

        // Json serialization methods
        //serialise
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var index = value as MovieIndexQueryMatch;        
            if (value == null)
                return;
            writer.WriteStartObject();
            writer.WritePropertyName(SIMILARITY_KEY);
            writer.WriteValue(SimilarityScore);
            if (MatchFieldQuery)
            {
                writer.WritePropertyName(FIELDMATCHED_KEY);
                writer.WriteValue(FieldMatched ? "true" : "false");
            }
            writer.WritePropertyName(MATCHNUM_KEY);
            writer.WriteValue(ID);
            base.SetWriter(index, writer, serializer);
            writer.WriteEndObject();
        }

        //deserialize
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var index = new MovieIndexQueryMatch();
            JObject jsonObj = JObject.Load(reader);
            var jsonSimilarity = jsonObj[SIMILARITY_KEY].Value<double>();
            index.SimilarityScore = jsonSimilarity;
            var jsonMatched = jsonObj[FIELDMATCHED_KEY].Value<string>();
            if (jsonMatched != null)
                index.FieldMatched = jsonMatched.Equals("true") ? true : false; 
            var jsonID = jsonObj[MATCHNUM_KEY].Value<int>();
            index.ID = jsonID;
            base.SetReader(index, jsonObj);
            return index;
        }

        // simple serialization/deserialization methods for ease of access
        public override string Serialize()
        {
            string serialization = JsonConvert.SerializeObject(this, Formatting.Indented);
            return serialization;
        }

        public override MovieIndexQueryMatch Deserialize(string jsonSerialized)
        {
            try
            {
                return JsonConvert.DeserializeObject<MovieIndexQueryMatch>(jsonSerialized);
            }
            catch (Exception) { return null; }
        }

        /// <summary>
        /// Sorts in descending order of importance, with field matching prioritized i.e if query has been made
        /// requesting field matching, then checks if both has fieldmatched then returns similarity diff, or
        /// if other matched 1, or if this matched -1
        /// </summary>
        /// <param name="other">Other QueryMatch item to be compared against</param>
        /// <returns>positive if this is less value than other</returns>
        public int CompareTo(MovieIndexQueryMatch other)
        {
            if (Equals(this, other))
                return 0;
            if(MatchFieldQuery)
            {
                if (other.FieldMatched && this.FieldMatched)
                    return CompareSimilarities(other);
                if (other.FieldMatched)
                    return 1;
                if (FieldMatched)
                    return -1;
            }
            return CompareSimilarities(other);
        }

        /// <summary>
        /// Compares other items similarity
        /// </summary>
        /// <param name="other">other QueryMatch</param>
        /// <returns>positive if this is less value than other</returns>
        private int CompareSimilarities(MovieIndexQueryMatch other)
        {
            return other.SimilarityScore - this.SimilarityScore < 0 ? -1: 1;
        }

        public bool Equals([AllowNull] MovieIndexQueryMatch x, [AllowNull] MovieIndexQueryMatch y)
        {
            return base.Equals(x, y);
        }

        public int GetHashCode([DisallowNull] MovieIndexQueryMatch obj)
        {
            return base.GetHashCode(obj);
        }
    }
}
