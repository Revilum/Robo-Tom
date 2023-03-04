using Discord;

namespace Robo_Tom.Playable;

public abstract class Playable
{
    public abstract string GetTitle();
    public abstract TimeSpan? GetDuration();
    public abstract Task<Stream> GetStream();

    public Embed ToEmbed()
    {
        TimeSpan? duration = GetDuration();
        string timeSpan = duration == null ? "--:--" : $"{duration.Value.Minutes}:{duration.Value.Seconds % 60}";
        
        return new EmbedBuilder()
            .WithColor(Color.Green)
            .WithCurrentTimestamp()
            .WithTitle("Added Song to Queue")
            .WithDescription($"Added {GetTitle()} `{timeSpan}` to the queue.")
            .Build();
    }
}