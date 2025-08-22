using System.Net;
using System.Text.Json;

namespace GrandExchangeLogReader.Wiki
{
    public class PriceClient : IPriceWikiClient
    {
        private const string ITEM_PATH = "api/v1/osrs/latest?id=";
        private readonly HttpClient _client;

        public PriceClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<PriceResult?> GetCurrentPrice(int itemId)
        {
            using var response = await _client.GetAsync(ITEM_PATH + itemId.ToString());
            if (response.IsSuccessStatusCode)
            {
                var item = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PriceResponse>(item, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }).Data[itemId];
            }

            return null;
        }
    }
}
