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
    [JsonConverter(typeof(MovieIndexStemmed))]
    class MovieIndexStemmed: MovieIndex
    {
        // json attribute name constants
        private const string TOKENSPRE_KEY = "TokensPreStem",
            TOKENSPOST_KEY = "TokensPostStem";

        [PropertyName(TOKENSPRE_KEY)]
        public List<string> TokensPreStem { get; set; } = new List<string>();
        [PropertyName(TOKENSPOST_KEY)]
        public List<string> TokensPostStem { get; set; } = new List<string>();

        public MovieIndexStemmed(MovieIndex other, ProcessingPipeline pipe) : base(other)
        {
            this.TokensPreStem.AddRange(pipe.Tokens);
        }

        public MovieIndexStemmed() : base() { }

        public void AddStemmedTokensFromPipe(ProcessingPipeline pipe)
        {
            this.TokensPostStem.AddRange(pipe.Tokens);
        }

        // Json serialization methods
        //serialise
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var index = value as MovieIndexStemmed;
            if (value == null)
                return;
            writer.WriteStartObject();
            SetWriter(index, writer, serializer);
            writer.WriteEndObject();
        }

        protected void SetWriter(MovieIndexStemmed index, JsonWriter writer, JsonSerializer serializer)
        {
            base.SetWriter(index, writer, serializer);
            writer.WritePropertyName(TOKENSPRE_KEY);
            writer.Formatting = Formatting.None;
            writer.WriteStartArray();
            foreach (var token in index.TokensPreStem)
                serializer.Serialize(writer, token);
            writer.WriteEndArray();
            writer.Formatting = Formatting.Indented;
            writer.WritePropertyName(TOKENSPOST_KEY);
            writer.Formatting = Formatting.None;
            writer.WriteStartArray();
            foreach (var sentence in index.TokensPostStem)
                serializer.Serialize(writer, sentence);
            writer.WriteEndArray();
            writer.Formatting = Formatting.Indented;
        }

        //deserialize
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var index = new MovieIndexStemmed();
            JObject jsonObj = JObject.Load(reader);
            SetReader(index, jsonObj);
            return index;
        }

        protected void SetReader(MovieIndexStemmed index, JObject jsonObj)
        {
            base.SetReader(index, jsonObj);
            var jsonTokensPreList = jsonObj[TOKENSPRE_KEY].Value<JArray>();
            var tokensPreList = jsonTokensPreList.ToObject<List<string>>();
            index.TokensPreStem.AddRange(tokensPreList);
            var jsonTokensPostList = jsonObj[TOKENSPOST_KEY].Value<JArray>();
            var tokensPostList = jsonTokensPostList.ToObject<List<string>>();
            index.TokensPostStem.AddRange(tokensPostList);
        }

        // simple serialization/deserialization methods for ease of access
        public override string Serialize()
        {
            string serialization = JsonConvert.SerializeObject(this, Formatting.Indented);
            return serialization;
        }

        public override MovieIndexStemmed Deserialize(string jsonSerialized)
        {
            try
            {
                return JsonConvert.DeserializeObject<MovieIndexStemmed>(jsonSerialized);
            }
            catch (Exception) { return null; }
        }
    }
}
