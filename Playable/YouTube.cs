using YoutubeExplode;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;

namespace Robo_Tom.Playable;

public class YouTube : IPlayable
{
    private static readonly YoutubeClient YoutubeClient = new();
    public async Task<Stream> GetStream(string input)
    {
        IAsyncEnumerable<VideoSearchResult> videos =
            YoutubeClient.Search.GetVideosAsync(input);
        string video = (await videos.FirstAsync()).Url;
        StreamManifest manifest = await YoutubeClient.Videos.Streams.GetManifestAsync(video);
        IStreamInfo streamInfo = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();
        return await YoutubeClient.Videos.Streams.GetAsync(streamInfo);
    }
}