using System.Diagnostics;
using System.Text.Json;

string currentDir = AppContext.BaseDirectory;
string[] folders = Directory.GetDirectories(currentDir);

string replaysPath;

string currentGame = string.Empty;

if (!folders.Contains("replays") || folders.Length == 0)
{
    Console.Write("Replay directory path: ");
    string? input;

    do
    {
        input = Console.ReadLine();

        if (string.IsNullOrEmpty(input) || !Path.Exists(input))
            Console.Write("Invalid path\n> ");
    }
    while (!Path.Exists(input));

    replaysPath = input;
}
else
    replaysPath = Path.Combine(currentDir, "replays");

Console.WriteLine("Active");

while (true)
{
    foreach (string file in Directory.GetFiles(replaysPath, "*", SearchOption.AllDirectories))
    {
        string extentionName = file.Split('.')[1];
        if (extentionName != "json")
            continue;

        string root = await File.ReadAllTextAsync(file);
        if (root == currentGame)
            continue;

        currentGame = root;

        JsonElement doc = JsonDocument.Parse(root).RootElement;
        string content = doc.GetProperty("vehicles").GetRawText();

        Player[] players = JsonSerializer.Deserialize<Player[]>(content)!;

        foreach (var player in players)
        {
            await player.GetPlayerId();
            player.Loop();

            await Task.Delay(1000);
        }
    }

    await Task.Delay(10000);
}

class Config
{
    public static string Server = Debugger.IsAttached
        ? "http://localhost:4000"
        : "http://172.232.156.69:4000";
}