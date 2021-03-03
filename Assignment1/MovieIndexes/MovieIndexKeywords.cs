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
    [JsonConverter(typeof(MovieIndexKeyWords))]
    public class MovieIndexKeyWords : MovieIndex
    {
        // json attribute name constants
        protected const string KEYWORD_KEY = "Keywords";

        [PropertyName(KEYWORD_KEY), Keyword()]
        public List<string> KeyWords { get; set; } = new List<string>();

        public MovieIndexKeyWords(MovieIndex other, ProcessingPipeline pipe) : base(other)
        {
            this.KeyWords.AddRange(pipe.Keywords);
        }

        public MovieIndexKeyWords() : base() { }

        // Json serialization methods
        //serialise
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var index = value as MovieIndexKeyWords;
            if (value == null)
                return;
            writer.WriteStartObject();
            SetWriter(index, writer, serializer);
            writer.WriteEndObject();
        }

        protected void SetWriter(MovieIndexKeyWords index, JsonWriter writer, JsonSerializer serializer)
        {
            base.SetWriter(index, writer, serializer);
            writer.WritePropertyName(KEYWORD_KEY);
            writer.Formatting = Formatting.None;
            writer.WriteStartArray();
            foreach (var keyword in index.KeyWords)
                serializer.Serialize(writer, keyword);
            writer.WriteEndArray();
            writer.Formatting = Formatting.Indented;
        }

        //deserialize
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var index = new MovieIndexKeyWords();
            JObject jsonObj = JObject.Load(reader);
            SetReader(index, jsonObj);
            return index;
        }

        protected void SetReader(MovieIndexKeyWords index, JObject jsonObj)
        {
            base.SetReader(index, jsonObj);
            var jsonKeywordsPreList = jsonObj[KEYWORD_KEY].Value<JArray>();
            var kwordsPreList = jsonKeywordsPreList.ToObject<List<string>>();
            index.KeyWords.AddRange(kwordsPreList);
        }

        // simple serialization/deserialization methods for ease of access
        public override string Serialize()
        {
            string serialization = JsonConvert.SerializeObject(this, Formatting.Indented);
            return serialization;
        }

        public override MovieIndexKeyWords Deserialize(string jsonSerialized)
        {
            try
            {
                return JsonConvert.DeserializeObject<MovieIndexKeyWords>(jsonSerialized);
            }
            catch (Exception) { return null; }
        }
    }
}
