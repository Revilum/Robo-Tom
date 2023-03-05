namespace Robo_Tom.Utils;

public class Queue
{
    private readonly List<Playable.Playable> _list = new();
    public QueueTypes Type { get; set; } = QueueTypes.Queue;
    public bool Deleting { get; set; } = true;

    public void AddToQueue(Playable.Playable playable)
    {
        switch (Type)
        {
            case QueueTypes.Queue:
                _list.Add(playable);
                break;
            case QueueTypes.Stack:
                _list.Insert(0, playable);
                break;
            case QueueTypes.Random:
                _list.Insert(Random.Shared.Next(0, _list.Count), playable);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(playable));
        }
    }

    public Playable.Playable? GetNextItem()
    {
        if (!_list.Any())
            return null;
            
        Playable.Playable item = _list.First();
        if (Deleting)
            _list.Remove(item);

        return item;
    }
}