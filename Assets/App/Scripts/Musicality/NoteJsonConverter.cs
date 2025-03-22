using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonReader = Newtonsoft.Json.JsonReader;
using JsonWriter = Newtonsoft.Json.JsonWriter;

namespace Musicality
{
    // https://skrift.io/issues/bulletproof-interface-deserialization-in-jsonnet/
    public class NoteJsonConverter : JsonConverter
    {
        public override bool CanWrite => true;
        public override bool CanRead => true;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            Enum.TryParse<NoteType>(jsonObject["NoteType"].ToString(), out var noteType);
            // var noteType = jsonObject["NoteType"].Value<NoteType>(); // This didn't work
            
            INote noteObj = noteType switch
            {
                NoteType.EDO => new NoteEDO(),
                NoteType.Function => new NoteFunction(),
                NoteType.Irrational => new NoteIrrational(),
                NoteType.JI => new NoteJI(),
                NoteType.RawFreq => throw new NotImplementedException(),
                NoteType.UnpitchedMidi => new NoteUnpitchedMidi(),
                _ => throw new ArgumentOutOfRangeException()
            };
            serializer.Populate(jsonObject.CreateReader(), noteObj);
            return noteObj;
        }

        public override bool CanConvert(Type objectType)
        {
            return false;
        }
    }
}