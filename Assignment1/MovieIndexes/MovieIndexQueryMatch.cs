using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nest;

namespace Assignment1
{
    [JsonConverter(typeof(MovieIndexQueryMatch))]
    public class MovieIndexQueryMatch : MovieIndex
    {
        // json attribute name constants
        protected const string SIMILARITY_KEY = "SimilarityScore",
            FIELDMATCHED_KEY = "FieldMatched",
            MATCHNUM_KEY = "MatchRank";

        [PropertyName(SIMILARITY_KEY)]
        public double SimilarityScore{ get; set; } = 0;

        [PropertyName(FIELDMATCHED_KEY)]
        public bool FieldMatched { get; set; } = false;

        [PropertyName(MATCHNUM_KEY)]
        public int ID { get; set; } = 0;

        public MovieIndexQueryMatch(MovieIndex other, ProcessingPipeline pipe) : base(other)
        {
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
            writer.WritePropertyName(FIELDMATCHED_KEY);
            writer.WriteValue(FieldMatched ? "true" : "false");
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
    }
}
