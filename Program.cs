using System.Text.Json;
using Discord;
using Discord.WebSocket;
using Robo_Tom.Utils;

namespace Robo_Tom;

public static class RoboTom
{
    private static Dictionary<string, string>? Config { get; } = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText("./config.json"));
    public static DiscordSocketClient Client { get; } = new();

    public static async Task Main()
    {
        Client.Log += LogAsync;
        Client.Ready += Ready;
        Client.SlashCommandExecuted += Commands.RunCommand.Run;
        await Client.LoginAsync(TokenType.Bot, Config?["token"]);
        await Client.StartAsync();
        await Task.Delay(-1);
    }

    private static async Task Ready()
    {
        string[] args = Environment.GetCommandLineArgs();
        if (args.Length > 1 && args[1] == "init")
        {
            await Commands.RunCommand.InitCommands();
        }
    }

    public static Task LogAsync(LogMessage message)
    {
        if (message.Exception is InteractionException cmdException && cmdException.Interaction.GetType() == typeof(SocketSlashCommand))
        {
            Console.WriteLine($"[Command/{message.Severity}] {((SocketSlashCommand)cmdException.Interaction).CommandName}"
                              + $" failed to execute in {Client.GetChannel((ulong)cmdException.Interaction.ChannelId!)}.");
            Console.WriteLine(cmdException.Exception);
        }
        else 
            Console.WriteLine($"[General/{message.Severity}] {message}");

        return Task.CompletedTask;
    }

}