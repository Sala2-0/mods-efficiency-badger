using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

class Player
{
    [JsonPropertyName("shipId")] public required long ShipId { get; init; }
    [JsonPropertyName("relation")] public required int Relation { get; init; }
    [JsonPropertyName("id")] public required int Id { get; init; } // Ingen aning vad detta är än
    [JsonPropertyName("name")] public required string Name { get; set; }

    private int playerId_;
    private string statsLink_ = string.Empty;
    private int baseExp_;
    private int time_ = 0; // Sekunder
    private HttpClient client_ = new();

    public async Task GetPlayerId()
    {
        if (Name.Contains(':')) return;

        try
        {
            var res = await client_.GetAsync($"https://api.worldofwarships.eu/wows/account/list/?application_id=899b9d71d2f18b5f0b42f3055eb8bb26&search={Name}&type=exact");

            JsonDocument doc = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());
            JsonElement data = doc.RootElement.GetProperty("data");

            playerId_ = data.EnumerateArray()
                .First()
                .GetProperty("account_id")
                .GetInt32();
            statsLink_ = $"https://vortex.worldofwarships.eu/api/accounts/{playerId_}/ships/{ShipId}";

            await GetBaseExp();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to get player id for '{Name}': {e.Message}");
        }
    }

    public async Task GetBaseExp()
    {
        var res = await client_.GetAsync(statsLink_);

        JsonDocument doc = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());
        baseExp_ = doc.RootElement
            .GetProperty("data")
            .EnumerateObject().First().Value
            .GetProperty("statistics")
            .EnumerateObject().First().Value
            .GetProperty("pvp")
            .GetProperty("original_exp").GetInt32();
    }

    public async Task Loop()
    {
        while (time_ <= 1200)
        {
            var res = await client_.GetAsync(statsLink_);

            JsonDocument doc = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());
            int apiBaseExp = doc.RootElement
                .GetProperty("data")
                .EnumerateObject().First().Value
                .GetProperty("statistics")
                .EnumerateObject().First().Value
                .GetProperty("pvp")
                .GetProperty("original_exp").GetInt32();

            if (apiBaseExp > baseExp_)
            {
                int baseExpDiff = apiBaseExp - baseExp_;

                Console.WriteLine($"Player '{Name}' got {baseExpDiff} base XP for ship '{ShipId}'");

                using StringContent json = new(
                    JsonSerializer.Serialize(new
                    {
                        shipId = ShipId,
                        baseExp = baseExpDiff
                    }),
                    Encoding.UTF8,
                    "application/json"
                );

                var postRes = await client_.PostAsync(Config.Server + "/insert", json);
                postRes.EnsureSuccessStatusCode();

                return;
            }

            time_ += 10;
            await Task.Delay(10000);
        }

        Console.WriteLine($"Aborted loop for player '{Name}': Unchanged data for 20 min");
    }
}