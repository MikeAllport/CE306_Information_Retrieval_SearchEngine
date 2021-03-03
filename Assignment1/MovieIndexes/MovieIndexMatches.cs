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
    [JsonConverter(typeof(MovieIndexMatches))]
    public class MovieIndexMatches :
        JsonConverter
    {
        // json attribute name constants
        protected const string MATCHLIST_KEY = "Results",
            QUERY_KEY = "QueryString",
            RESULTQTY_KEY = "ResultsFound";

        public SortedSet<MovieIndexQueryMatch> Matches { get; set; } = new SortedSet<MovieIndexQueryMatch>();
        public string Query { get; set; } = "";
        public int ResultsFound = 0;

        public MovieIndexMatches(int numResults, string query)
        {
            this.ResultsFound = numResults;
            this.Query = query;
        }

        public MovieIndexMatches() { }

        // Json serialization methods
        //serialise
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var index = value as MovieIndexMatches;
            if (value == null)
                return;
            writer.WriteStartObject();
            writer.WritePropertyName(QUERY_KEY);
            writer.WriteValue(index.Query);
            writer.WritePropertyName(RESULTQTY_KEY);
            writer.WriteValue(index.ResultsFound);
            writer.WritePropertyName(MATCHLIST_KEY);
            writer.WriteStartArray();
            foreach (var match in index.Matches)
            {
                match.WriteJson(writer, match, serializer);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        //deserialize
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var index = new MovieIndexMatches();
            JObject jsonObj = JObject.Load(reader);
            var jsonQuery = jsonObj[QUERY_KEY].Value<string>();
            index.Query = jsonQuery;
            var jsonMatches = jsonObj[MATCHLIST_KEY].Value<JArray>();
            var match = jsonMatches.ToObject<SortedSet<MovieIndexQueryMatch>>();
            index.Matches = new SortedSet<MovieIndexQueryMatch>(match);
            return index;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(MovieIndexMatches).IsAssignableFrom(objectType);
        }

        // simple serialization/deserialization methods for ease of access
        public string Serialize()
        {
            string serialization = JsonConvert.SerializeObject(this, Formatting.Indented);
            return serialization;
        }

        public MovieIndexMatches Deserialize(string jsonSerialized)
        {
            try
            {
                return JsonConvert.DeserializeObject<MovieIndexMatches>(jsonSerialized);
            }
            catch (Exception) { return null; }
        }
    }
}
