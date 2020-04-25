using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Globalization;

namespace SmartEE.WeatherForecast.Common.Models.Location
{
    public partial class LocationModel
    {
        public int CityId { get; set; }
        public string CityName { get; set; }
        public long QueryElapsedMilliseconds { get; set; }
        //[JsonProperty("place_id")]
        //[JsonConverter(typeof(ParseStringConverter))]
        //public long PlaceId { get; set; }

        //[JsonProperty("licence")]
        //public Uri Licence { get; set; }

        //[JsonProperty("osm_type")]
        //public OsmType OsmType { get; set; }

        //[JsonProperty("osm_id")]
        //public string OsmId { get; set; }

        //[JsonProperty("boundingbox")]
        //public string[] Boundingbox { get; set; }

        [JsonProperty("lat")]
        public string Lat { get; set; }

        [JsonProperty("lon")]
        public string Lon { get; set; }

        //[JsonProperty("display_name")]
        //public string DisplayName { get; set; }

        //[JsonProperty("class")]
        //public string Class { get; set; }

        //[JsonProperty("type")]
        //public string Type { get; set; }

        //[JsonProperty("importance")]
        //public double Importance { get; set; }

        //[JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        //public Uri Icon { get; set; }
    }

    public enum OsmType { Node, Relation, Way };

    public partial class Deserialize
    {
        public static LocationModel[] FromJson(string json) => JsonConvert.DeserializeObject<LocationModel[]>(json, SmartEE.WeatherForecast.Common.Models.Location.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this LocationModel[] self) => JsonConvert.SerializeObject(self, SmartEE.WeatherForecast.Common.Models.Location.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                OsmTypeConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class OsmTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(OsmType) || t == typeof(OsmType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "node":
                    return OsmType.Node;
                case "relation":
                    return OsmType.Relation;
                case "way":
                    return OsmType.Way;
            }
            throw new Exception("Cannot unmarshal type OsmType");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (OsmType)untypedValue;
            switch (value)
            {
                case OsmType.Node:
                    serializer.Serialize(writer, "node");
                    return;
                case OsmType.Relation:
                    serializer.Serialize(writer, "relation");
                    return;
                case OsmType.Way:
                    serializer.Serialize(writer, "way");
                    return;
            }
            throw new Exception("Cannot marshal type OsmType");
        }

        public static readonly OsmTypeConverter Singleton = new OsmTypeConverter();
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}
