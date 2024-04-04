using Newtonsoft.Json;

namespace EdgarFundFinder.DataImport
{
    public class CikConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(object);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                return reader.Value;
            }
            else if (reader.TokenType == JsonToken.Integer)
            {
                return reader.Value.ToString();
            }
            else
            {
                throw new JsonException();
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Write the value of the CikObject property as a string
            writer.WriteValue(value.ToString());
        }
    }
}