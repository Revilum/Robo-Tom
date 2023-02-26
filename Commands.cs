using Discord;
using Discord.WebSocket;

namespace Robo_Tom;

public static class Commands
{
    public static async Task InitCommands()
    {
        ApplicationCommandProperties[] commands =
        {
            new SlashCommandBuilder()
                .WithName("play")
                .WithDescription("enter a search query for your song to be queued.")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("song")
                    .WithDescription("Title of the song to be played")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithRequired(true))
                .Build()
        };
        await RoboTom.Instance.Client.BulkOverwriteGlobalApplicationCommandsAsync(commands);
    }
    
    public static Task RunCommand (SocketSlashCommand cmd)
    {
        switch (cmd.CommandName)
        {
            case "play":
                break;
        }
        return Task.CompletedTask;
    }
}