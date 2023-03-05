using Discord;
using Discord.WebSocket;
using Robo_Tom.Utils;

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
                .WithDMPermission(false)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("song")
                    .WithDescription("Title of the song to be played")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithRequired(true))
                .Build()
        };
        await RoboTom.Client.BulkOverwriteGlobalApplicationCommandsAsync(commands);
    }
    
    public static Task Run(SocketSlashCommand cmd)
    {
        Tools.RunInBackGround(async () =>
        {
            try
            {
                await cmd.DeferAsync();
                switch (cmd.CommandName)
                {
                    case "play":
                        await Play.AddSongToQueue(cmd);
                        break;
                }
            }
            catch (Exception ex)
            {
                InteractionException newEx = new(cmd, ex);
                await RoboTom.LogAsync(new LogMessage(LogSeverity.Error, "SlashCommand", ex.Message, newEx));
                await cmd.ModifyOriginalResponseAsync(x => x.Content = "An error occured!");
            }
           
        });
        return Task.CompletedTask;
    }
}