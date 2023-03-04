using Robo_Tom.Playable;

namespace Robo_Tom.Utils;

public class Queue : List<Playable.Playable>
{
    private QueueTypes Type { get; set; } = 0;

    public void AddToQueue(Playable.Playable playable)
    {
        switch (Type)
        {
            case QueueTypes.Queue:
                Add(playable);
                break;
            case QueueTypes.Stack:
                Insert(0, playable);
                break;
            case QueueTypes.Random:
                Insert(Random.Shared.Next(0, Count), playable);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(playable));
        }
    }
}