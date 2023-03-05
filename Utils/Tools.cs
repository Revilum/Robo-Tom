using Discord;
using Discord.WebSocket;
using Robo_Tom.Commands;

namespace Robo_Tom.Utils;

public static class Tools
{
    public static Task RunInBackGround(Func<Task> task)
    {
        Task.Run(task).ContinueWith(t =>
        {
            if (t.IsFaulted)
                throw t.Exception!;
        });
        return Task.CompletedTask;
    }
    
    public static IVoiceChannel? GetVoiceChannelFromUser(IDiscordInteraction cmd)
    {
        return RoboTom.Client.GetGuild((ulong)cmd.GuildId!).VoiceChannels.FirstOrDefault(voiceChannel =>
            voiceChannel.ConnectedUsers.Any(voiceUser => voiceUser.Id == cmd.User.Id));
    }

    public static bool IsInVoice(IDiscordInteraction cmd)
    {
        Play? instance = Play.GetInstance((ulong)cmd.GuildId!);
        if (instance == null)
            return false;
        return ((IGuildUser)cmd.User).VoiceChannel == instance.Vc;
    }

    public static int Mod(int n, int m)
    {
        int buffer = n % m;
        if (buffer < 0)
            return m + buffer;
        return buffer;
    }

    public static async Task ChangePlayer(IDiscordInteraction cmd, Action<DiscordMediaPlayer> func, Embed success)
    {
        Embed notInVcEmbed = new EmbedBuilder()
            .WithTitle("Error")
            .WithDescription("You must be in a voice channel in order to use the command.")
            .WithColor(Color.Red)
            .WithCurrentTimestamp()
            .Build();
        
        if (IsInVoice(cmd))
        {
            func(Play.GetInstance((ulong)cmd.GuildId!)!.Player);
            await cmd.ModifyOriginalResponseAsync(x => x.Embed = success);
            return;
        }
        await cmd.ModifyOriginalResponseAsync(x => x.Embed = notInVcEmbed);
    }

    public static Embed GetSuccessEmbed(string title, string description)
    {
        return new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(description)
            .WithColor(Color.Green)
            .WithCurrentTimestamp()
            .Build();
    }
}