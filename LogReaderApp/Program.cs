using GrandExchangeLogReader;
using GrandExchangeLogReader.Wiki;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Text.Json.Serialization;

try
{
    var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddOptions()
            .RegisterApiClients()
            ;
    })
    .Build();

    await host.StartAsync();

    IServiceProvider sp = host.Services;
    var geClient = sp.GetRequiredService<IGrandExchangeClient>();
    var priceClient = sp.GetRequiredService<IPriceWikiClient>();

    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
    options.Converters.Add(new DateOnlyJsonConverter());
    options.Converters.Add(new TimeOnlyJsonConverter());

    var all = new List<GeLogEntry>();
    var directoryPath = @"C:\Users\nicho\.runelite\exchange-logger";
    foreach (var file in Directory.EnumerateFiles(directoryPath))
    {
        foreach (var line in File.ReadLines(file))
        {
            if (string.IsNullOrWhiteSpace(line) || line[0] != '{') continue;
            try
            {
                var entry = JsonSerializer.Deserialize<GeLogEntry>(line, options);
                if (entry is not null) all.Add(entry);
            }
            catch
            {
                // ignore bad lines
            }
        }
    }

    // Oldest → newest; only qty == 1; only the fills (BOUGHT/SOLD)
    var fills = all
        .Where(e => e.Qty == 1 && (e.State is GrandExchangeLogEntryType.BOUGHT or GrandExchangeLogEntryType.SOLD))
        .OrderBy(e => e.Timestamp)
        .ToList();

    var openBuys = new Dictionary<int, Queue<GeLogEntry>>(); // itemId -> FIFO buys

    const int NAME_W = 32;
    const int ID_W = 6;
    const int PRICE_W = 15;
    Console.WriteLine($"{"Item",-NAME_W}     {"Bought For",PRICE_W}  {"Sold For",PRICE_W}    --> {"Profit/Loss",PRICE_W}");
    Console.WriteLine(new string('-', NAME_W + ID_W + PRICE_W * 3 + 12));

    List<CompletedTransaction> completedTransactions = [];
    foreach (var e in fills)
    {
        if (e.State == GrandExchangeLogEntryType.BOUGHT)
        {
            if (!openBuys.TryGetValue(e.Item, out var q))
                openBuys[e.Item] = q = new Queue<GeLogEntry>();
            q.Enqueue(e);
        }
        else // SOLD
        {
            if (!openBuys.TryGetValue(e.Item, out var q) || q.Count == 0)
            {
                // Sell without prior buy in the log -> ignore
                continue;
            }

            var buy = q.Dequeue();

            static string Fit(string s, int w)
            {
                if (s.Length <= w) return s.PadRight(w);
                return s[..(w - 1)] + "…"; // truncate with ellipsis
            }

            var actualSellPrice = (e.Worth * .98); // 2% GE tax
            long pnl = (long)(actualSellPrice - buy.Worth);
            GrandExchangeItem? ge = null;
            try
            {
                ge = await geClient.GetGrandExchangeItemAsync(e.Item);
            }
            catch
            {
                await Task.Delay(10000); // Rate limit handling
                try
                {
                    ge = await geClient.GetGrandExchangeItemAsync(e.Item);
                }
                catch 
                {
                }
            }
            var name = Fit(ge?.Name ?? $"Unknown Item ({e.Item})", NAME_W);

            completedTransactions.Add(new CompletedTransaction(name, pnl));
            Console.WriteLine(
                $"{name,-NAME_W}     " +
                $"{buy.Worth,PRICE_W:N0}  " +
                $"{actualSellPrice,PRICE_W:N0}    --> " +
                $"{pnl,PRICE_W:N0}");
        }
    }

    var best = completedTransactions.MaxBy(t => t.Profit);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Best flip: {best.Profit:n0} on {best.ItemName}");
    Console.ForegroundColor = ConsoleColor.Red;
    var worst = completedTransactions.MinBy(t => t.Profit);
    Console.WriteLine($"Worst flip: {worst.Profit:n0} on {worst.ItemName}");
    Console.ResetColor();
    Console.WriteLine($"Total profit/loss: {completedTransactions.Sum(t => t.Profit):n0}");

    Console.ReadLine();
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);
}