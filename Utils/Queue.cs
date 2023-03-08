using Discord;

namespace Robo_Tom.Utils;

public class Queue
{
    private readonly List<Playable.Playable> _list = new();
    public QueueTypes Type { get; set; } = QueueTypes.Queue;
    public bool Loop { get; set; }
    private int _nextSong;
    private Playable.Playable _current = null!;

    public void AddToQueue(Playable.Playable playable)
    {
        _list.Add(playable);
    }

    public Playable.Playable? GetNextItem()
    {
        if (!_list.Any())
            return null;

        Playable.Playable item;
        switch (Type)
        {
            case QueueTypes.Queue:
                item = _list[_nextSong];
                _current = item;
                if (Loop)
                    _nextSong = Tools.Mod(_nextSong + 1, _list.Count);
                else
                    _list.Remove(item);
                break;
            
            case QueueTypes.Stack:
                item = _list[_nextSong];
                _nextSong = Tools.Mod(_nextSong - 1, _list.Count);
                if (!Loop)
                    _list.Remove(item);
                break;
            
            case QueueTypes.Random:
                item = _list[Random.Shared.Next(0, _list.Count)];
                if (!Loop)
                    _list.Remove(item);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
        return item;
    }
    
    public Embed ToEmbed()
    {
        List<Playable.Playable> copy = _list.ToArray().ToList();
        copy.Insert(0, _current);
        
        List<string> indices = new();
        string[] titles = new[]{_current.GetTitle()}.Concat(_list.Select(song => song.GetTitle())).ToArray();
        List<string> durations = new();
        
        foreach (Playable.Playable song in copy)
        {
            string index = $"**{copy.IndexOf(song)}**";
            string duration = $"`{Tools.TimeSpanToString(song.GetDuration())}`";
            for (int i = 0; i < Tools.GetLineBreaks(titles[_list.IndexOf(song)-1]); i++)
            {
                index += "\n";
                duration += "\n";
            }
            indices.Add(index);
            durations.Add(duration);
        }

        indices[0] = "**current**";
        
        EmbedBuilder builder = new EmbedBuilder()
            .WithTitle("Playlist")
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .AddField("Position", string.Join("\n\n", indices), true)
            .AddField("Title", string.Join("\n\n", titles), true)
            .AddField("Duration", string.Join($"\n\n", durations), true);
        
        return builder.Build();
    }
}