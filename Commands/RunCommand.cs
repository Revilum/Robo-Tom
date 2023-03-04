using Discord;
using Discord.WebSocket;

namespace Robo_Tom.Commands;

public static class RunCommand
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
    
    public static async Task Run(SocketSlashCommand cmd)
    {
        await cmd.DeferAsync();
        switch (cmd.CommandName)
        {
            case "play":
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Play.PlayInGuild(cmd);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                });
                break;
        }
    }
}