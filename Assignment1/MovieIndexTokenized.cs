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
    [JsonConverter(typeof(MovieIndexTokenized))]
    public class MovieIndexTokenized: MovieIndex
    {
        // json attribute name constants
        private const string TOKENS_KEY = "Tokens",
            SENTENCES_KEY = "Sentences",
            BULLETPOINTS_KEY = "BulletPoints";

        [PropertyName(TOKENS_KEY)]
        public List<string> Tokens { get; set; } = new List<string>();
        [PropertyName(SENTENCES_KEY)]
        public List<string> Sentences { get; set; } = new List<string>();
        [PropertyName(BULLETPOINTS_KEY)]
        public List<string> BulletPoints { get; set; } = new List<string>();

        public MovieIndexTokenized(MovieIndex other, ProcessingPipeline pipe): base(other)
        {
            this.Tokens = pipe.Tokens;
            this.Sentences = pipe.Sentences;
            this.BulletPoints = pipe.BulletPoints;
        }

        public MovieIndexTokenized() : base() { }

        // Json serialization methods
        //serialise
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var index = value as MovieIndexTokenized;
            if (value == null)
                return;
            writer.WriteStartObject();
            SetWriter(index, writer, serializer);
            writer.WriteEndObject();
        }

        protected void SetWriter(MovieIndexTokenized index, JsonWriter writer, JsonSerializer serializer)
        {
            base.SetWriter(index, writer, serializer);
            writer.WritePropertyName(TOKENS_KEY);
            writer.Formatting = Formatting.None;
            writer.WriteStartArray();
            foreach (var token in index.Tokens)
                serializer.Serialize(writer, token);
            writer.WriteEndArray();
            writer.Formatting = Formatting.Indented;
            writer.WritePropertyName(SENTENCES_KEY);
            writer.Formatting = Formatting.None;
            writer.WriteStartArray();
            foreach (var sentence in index.Sentences)
                serializer.Serialize(writer, sentence);
            writer.WriteEndArray();
            writer.Formatting = Formatting.Indented;
            writer.WritePropertyName(BULLETPOINTS_KEY);
            writer.Formatting = Formatting.None;
            writer.WriteStartArray();
            foreach (var bp in index.BulletPoints)
                serializer.Serialize(writer, bp);
            writer.WriteEndArray();
            writer.Formatting = Formatting.Indented;
        }

        //deserialize
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var index = new MovieIndexTokenized();
            JObject jsonObj = JObject.Load(reader);
            SetReader(index, jsonObj);
            return index;
        }

        protected void SetReader(MovieIndexTokenized index, JObject jsonObj)
        {
            base.SetReader(index, jsonObj);
            var jsonTokensList = jsonObj[TOKENS_KEY].Value<JArray>();
            var tokensList = jsonTokensList.ToObject<List<string>>();
            index.Tokens.AddRange(tokensList);
            var jsonSentenceList = jsonObj[SENTENCES_KEY].Value<JArray>();
            var sentenceList = jsonSentenceList.ToObject<List<string>>();
            index.Tokens.AddRange(sentenceList);
            var jsonBPList = jsonObj[BULLETPOINTS_KEY].Value<JArray>();
            var bpList = jsonBPList.ToObject<List<string>>();
            index.Tokens.AddRange(bpList);
        }

        // simple serialization/deserialization methods for ease of access
        public override string Serialize()
        {
            string serialization = JsonConvert.SerializeObject(this, Formatting.Indented);
            return serialization;
        }

        public override MovieIndexTokenized Deserialize(string jsonSerialized)
        {
            try
            {
                return JsonConvert.DeserializeObject<MovieIndexTokenized>(jsonSerialized);
            }
            catch (Exception) { return null; }
        }
    }
}
