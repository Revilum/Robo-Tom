using System.Text.Json;
using Discord;
using Discord.WebSocket;

namespace Robo_Tom;

public class RoboTom
{
    public static RoboTom Instance { get; } = new();
    public static Dictionary<string, string>? Config { get; } = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText("./config.json"));
    public DiscordSocketClient Client { get; } = new();

    public static Task Main(string[] args) => Instance.MainAsync();

    private RoboTom()
    {
        Client.Log += Log;
        Client.Ready += Ready;
        Client.SlashCommandExecuted += Commands.RunCommand;
    }
    
    private async Task MainAsync()
    {
        await Client.LoginAsync(TokenType.Bot, Config?["token"]);
        await Client.StartAsync();
        await Task.Delay(-1);
    }

    private Task Log(LogMessage log)
    {
        Console.WriteLine(log.Message + log.Exception);
        return Task.CompletedTask;
    }

    private async Task Ready()
    {
        string[] args = Environment.GetCommandLineArgs();
        if (args.Length > 1 && args[1] == "init")
        {
            await Commands.InitCommands();
        }
    }
}