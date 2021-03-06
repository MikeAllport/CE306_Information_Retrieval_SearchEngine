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
    [JsonConverter(typeof(MovieIndexKeyWordStemmed))]
    class MovieIndexKeyWordStemmed: MovieIndexKeyWords
    {
        // json attribute name constants
        private const string KEYWORDS_PRESTEM_KEY = "KeywordsPreStemStem";

        [PropertyName(KEYWORDS_PRESTEM_KEY), Keyword]
        public List<string> KeywordsPreStem { get; set; } = new List<string>();

        public MovieIndexKeyWordStemmed(MovieIndex other, ProcessingPipeline pipe) : base(other, pipe)
        {
        }

        public MovieIndexKeyWordStemmed() : base() { }

        public void AddKeywordStemmedFromPipe(ProcessingPipeline pipe)
        {
            this.KeywordsPreStem.AddRange(this.KeyWords);
            this.KeyWords.Clear();
            this.KeyWords.AddRange(pipe.Keywords);
        }

        // Json serialization methods
        //serialise
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var index = value as MovieIndexKeyWordStemmed;
            if (value == null)
                return;
            writer.WriteStartObject();
            SetWriter(index, writer, serializer);
            writer.WriteEndObject();
        }

        protected void SetWriter(MovieIndexKeyWordStemmed index, JsonWriter writer, JsonSerializer serializer)
        {
            base.SetWriter(index, writer, serializer);
            writer.Formatting = Formatting.Indented;
            writer.WritePropertyName(KEYWORDS_PRESTEM_KEY);
            writer.Formatting = Formatting.None;
            writer.WriteStartArray();
            foreach (var keywordStemmed in index.KeywordsPreStem)
                serializer.Serialize(writer, keywordStemmed);
            writer.WriteEndArray();
            writer.Formatting = Formatting.Indented;
        }

        //deserialize
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var index = new MovieIndexKeyWordStemmed();
            JObject jsonObj = JObject.Load(reader);
            SetReader(index, jsonObj);
            return index;
        }

        protected void SetReader(MovieIndexKeyWordStemmed index, JObject jsonObj)
        {
            base.SetReader(index, jsonObj);
            var jsonKeywordsPreList = jsonObj[KEYWORD_KEY].Value<JArray>();
            var kwordsPreList = jsonKeywordsPreList.ToObject<List<string>>();
            index.KeyWords.AddRange(kwordsPreList);
            var jsonKeywordsPost = jsonObj[KEYWORDS_PRESTEM_KEY].Value<JArray>();
            var keywordsPostList = jsonKeywordsPost.ToObject<List<string>>();
            index.KeywordsPreStem.AddRange(keywordsPostList);
        }

        // simple serialization/deserialization methods for ease of access
        public override string Serialize()
        {
            string serialization = JsonConvert.SerializeObject(this, Formatting.Indented);
            return serialization;
        }

        public override MovieIndexKeyWordStemmed Deserialize(string jsonSerialized)
        {
            try
            {
                return JsonConvert.DeserializeObject<MovieIndexKeyWordStemmed>(jsonSerialized);
            }
            catch (Exception) { return null; }
        }
    }
}
