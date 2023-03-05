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
                .Build(),
            
            new SlashCommandBuilder()
                .WithName("skip")
                .WithDescription("Skips the currently playing song.")
                .WithDMPermission(false)
                .Build(),
            
            new SlashCommandBuilder()
                .WithName("pause")
                .WithDescription("play/pause the current song.")
                .WithDMPermission(false)
                .Build(),
            
            new SlashCommandBuilder()
                .WithName("stop")
                .WithDescription("disconnects the bot.")
                .WithDMPermission(false)
                .Build(),
            
            new SlashCommandBuilder()
                .WithName("volume")
                .WithDescription("changes the volume of the song. (only use this command if the audio is really quiet)")
                .WithDMPermission(false)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("volume percent")
                    .WithDescription("0 - 300 is the supported volume range.")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.Integer)
                    .WithMinValue(0)
                    .WithMaxValue(300))
                .Build(),
            
            new SlashCommandBuilder()
                .WithName("speed")
                .WithDescription("changes the playback speed.")
                .WithDMPermission(false)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("speed percent")
                    .WithDescription("1 - 300 is the supported speed range")
                    .WithType(ApplicationCommandOptionType.Integer)
                    .WithMinValue(1)
                    .WithMaxValue(300))
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
                    case "skip":
                        await Tools.ChangePlayer(cmd, x => x.PlayNext().Wait(),
                            Tools.GetSuccessEmbed("Skipped", "Skipped current Song."));
                        break;
                    case "pause":
                        await Tools.ChangePlayer(cmd, x => x.Pause(),
                            Tools.GetSuccessEmbed("Paused", "Paused current Song."));
                        break;
                    case "stop":
                        await Tools.ChangePlayer(cmd, x => x.Stop(),
                            Tools.GetSuccessEmbed("Stopped", "Stopped playing and disconnecting."));
                        break;
                    case "volume":
                        break;
                    case "speed":
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