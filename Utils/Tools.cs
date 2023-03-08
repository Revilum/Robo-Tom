using Discord;
using Robo_Tom.Commands;

namespace Robo_Tom.Utils;

public static class Tools
{
    public static readonly Embed CommandFailedEmbed = new EmbedBuilder()
        .WithTitle("Error")
        .WithDescription("You or the bot must be in a voice channel in order to use the command.")
        .WithColor(Color.Red)
        .WithCurrentTimestamp()
        .Build();
    
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

    private static bool IsInVoice(IDiscordInteraction cmd)
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
        if (IsInVoice(cmd))
        {
            func(Play.GetInstance((ulong)cmd.GuildId!)!.Player);
            await cmd.ModifyOriginalResponseAsync(x => x.Embed = success);
            return;
        }
        await cmd.ModifyOriginalResponseAsync(x => x.Embed = CommandFailedEmbed);
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

    public static QueueTypes ConvertToQueueType(string name)
    {
        QueueTypes type = name switch
        {
            "queue" => QueueTypes.Queue,
            "stack" => QueueTypes.Stack,
            "shuffle" => QueueTypes.Random,
            _ => throw new IndexOutOfRangeException()
        };
        return type;
    }

    public static void StopAllPlayback()
    {
        foreach ((_, Play play) in Play.ActiveGuilds)
        {
            play.Player.Stop();
        }
    }

    public static string TimeSpanToString(TimeSpan? span)
    {
        return span == null ? "--:--" : ((TimeSpan)span).ToString( @"mm\:ss");
    }

    public static int GetLineBreaks(string title)
    {
        string[] split = title.Split(" ");
        int sum = 0;
        int final = 0;
        foreach (string s in split)
        {
            sum += s.Length;
            if (sum > 51)
            { 
                sum = s.Length;
                final += 1;
            }
        }

        return final;
    }
}