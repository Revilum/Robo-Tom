using YoutubeExplode;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;

namespace Robo_Tom.Playable;

public class YouTube : Playable
{
    private static readonly YoutubeClient YoutubeClient = new();
    private readonly VideoSearchResult _video;

    public YouTube(string query)
    {
        _video = YoutubeClient.Search.GetVideosAsync(query).FirstAsync().GetAwaiter().GetResult();
    }
    public override string GetTitle()
    {
        return _video.Title;
    }

    public override TimeSpan? GetDuration()
    {
        return _video.Duration;
    }

    public override async Task<Stream> GetStream()
    {
        StreamManifest manifest = await YoutubeClient.Videos.Streams.GetManifestAsync(_video.Url);
        IStreamInfo streamInfo = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();
        return await YoutubeClient.Videos.Streams.GetAsync(streamInfo);
    }
}