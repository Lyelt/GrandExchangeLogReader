using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

namespace GrandExchangeLogReader.Wiki
{
    public static class HttpClientExtensions
    {
        public static IServiceCollection RegisterApiClients(this IServiceCollection services)
        {
            services.AddHttpClient<IGrandExchangeClient, GeClient>(client =>
            {
                client.BaseAddress = new Uri("https://secure.runescape.com/m=itemdb_oldschool/");
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("GrandExchangeLogReader/1.0 (+nicholas.ghobrial@gmail.com)");
            });

            services.AddHttpClient<IPriceWikiClient, PriceClient>(client =>
            {
                client.BaseAddress = new Uri("https://prices.runescape.wiki/");
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("GrandExchangeLogReader/1.0 (+nicholas.ghobrial@gmail.com)");
            });

            services.RemoveAll<IHttpMessageHandlerBuilderFilter>();

            return services;
        }
    }
}
