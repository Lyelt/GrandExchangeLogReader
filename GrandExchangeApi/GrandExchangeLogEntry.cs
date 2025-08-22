namespace GrandExchangeLogReader
{
    public enum GrandExchangeLogEntryType
    {
        EMPTY,
        SELLING,
        SOLD,
        CANCELLED_SELL,
        BUYING,
        BOUGHT,
        CANCELLED_BUY
    }
    public record GrandExchangeLogEntry(DateTimeOffset Timestamp, GrandExchangeLogEntryType EntryType, int ItemId, int Quantity, int TotalValue, int OfferValueEach);
}
