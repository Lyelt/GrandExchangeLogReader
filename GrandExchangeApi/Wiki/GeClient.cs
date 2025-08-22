using System.Net;
using System.Text.Json;

namespace GrandExchangeLogReader.Wiki
{
    public class GeClient : IGrandExchangeClient
    {
        private const string ITEM_PATH = "api/catalogue/detail.json?item=";
        private readonly HttpClient _client;

        public GeClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<GrandExchangeItem?> GetGrandExchangeItemAsync(int itemId)
        {
            using var response = await _client.GetAsync(ITEM_PATH + itemId.ToString());
            if (response.IsSuccessStatusCode)
            {
                var item = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<GrandExchangeResponse>(item, new JsonSerializerOptions { PropertyNameCaseInsensitive = true } ).Item;
            }

            return null;
        }
    }
}
