namespace Robo_Tom.Playable;

public interface IPlayable
{
    public abstract Task<Stream> GetStream(string input);
}