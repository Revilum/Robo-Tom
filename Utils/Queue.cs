namespace Robo_Tom.Utils;

public class Queue
{
    private readonly List<Playable.Playable> _list = new();
    public QueueTypes Type { get; set; } = QueueTypes.Queue;
    public bool Loop { get; set; } = false;
    private int _nextSong;

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
}