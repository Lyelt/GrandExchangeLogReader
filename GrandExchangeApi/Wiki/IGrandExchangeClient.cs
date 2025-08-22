namespace GrandExchangeLogReader.Wiki
{
    public interface IGrandExchangeClient
    {
        Task<GrandExchangeItem?> GetGrandExchangeItemAsync(int itemId);
    }
}
