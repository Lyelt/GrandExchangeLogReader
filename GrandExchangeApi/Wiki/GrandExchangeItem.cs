using System.Text.Json;
using System.Text.Json.Serialization;

namespace GrandExchangeLogReader.Wiki
{
    public record GrandExchangeResponse(GrandExchangeItem Item);

    public record GrandExchangeItem(int Id, string Name, string Icon);

    public record PriceResponse(Dictionary<int, PriceResult> Data);

    public record PriceResult(int High, int Low)
    {
        [JsonIgnore]
        public int Average => High + Low / 2;
    }

    public record GeLogEntry(DateOnly Date, TimeOnly Time, GrandExchangeLogEntryType State, int Item, int Qty, int Worth, int Max, int Offer)
    {
        [JsonIgnore]
        public DateTime Timestamp => Date.ToDateTime(Time);
    }

    public sealed record CompletedTransaction(string ItemName, long Profit);

    public sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        private const string Format = "yyyy-MM-dd";
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            DateOnly.ParseExact(reader.GetString()!, Format);

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.ToString(Format));
    }

    public sealed class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
    {
        private const string Format = "HH:mm:ss";
        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            TimeOnly.ParseExact(reader.GetString()!, Format);

        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.ToString(Format));
    }
}
