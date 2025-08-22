namespace GrandExchangeLogReader.Wiki
{
    public interface IPriceWikiClient
    {
        Task<PriceResult?> GetCurrentPrice(int itemId);
    }
}
