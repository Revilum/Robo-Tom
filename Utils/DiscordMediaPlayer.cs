using System.Diagnostics;
using LibVLCSharp.Shared;

namespace Robo_Tom.Utils;

public class DiscordMediaPlayer : IDisposable
{
    private static readonly LibVLC Vlc = new();
    private readonly MediaPlayer _player;
    private readonly int _sinkId;
    private readonly ulong _guildId;
    private readonly Process _outputProcess;
    private bool _justCreated;
    private readonly CancellationTokenSource _cancelToken;
    
    public readonly Stream AudioOutputStream;
    public readonly Queue Queue = new();

    public DiscordMediaPlayer(ulong guildId, CancellationTokenSource token)
    {
        string sinkName = guildId.ToString();
        _cancelToken = token;
        _guildId = guildId;
        _sinkId = CreateSink(sinkName);
        _outputProcess = CreateStream(sinkName);
        _justCreated = true;
        AudioOutputStream = _outputProcess.StandardOutput.BaseStream;
        
        _player = new MediaPlayer(Vlc);
        _player.SetOutputDevice(sinkName);
        _player.EndReached += async (_, _) => await PlayNext();
    }

    public async Task PlayNext()
    {
        Playable.Playable? nextItem = Queue.GetNextItem();
        if (nextItem == null)
        {
            Stop();
            return;
        }
        PlayStream(await nextItem.GetStream());
    }

    public void PlayStream(Stream stream)
    {
        _player.Play(new Media(Vlc, new StreamMediaInput(stream)));
    }

    public void Stop()
    {
        _cancelToken.Cancel();
        Dispose();
    }

    public async Task Start()
    {
        if (_justCreated)
        {
            _justCreated = false;
            await PlayNext();
        }
    }

    public void Play()
    {
        _player.Play();
    }

    public void Pause()
    {
        _player.Pause();
    }

    public void ChangeSpeed(ulong speed)
    {
        double newSpeed = speed / 100.0;
        _player.SetRate((float)newSpeed);
    }

    public void SetVolume(int volume)
    {
        _player.Volume = volume;
    }

    public void SkipTo(TimeSpan timeSpan)
    {
        _player.SeekTo(timeSpan);
    }
    
    private static Process CreateStream(string sink)
    {
        return Process.Start(new ProcessStartInfo
        {
            FileName = "parec",
            Arguments = $"-d {sink}.monitor --format=s16le --rate=48000 --channels=2",
            UseShellExecute = false,
            RedirectStandardOutput = true,
        })!;
    }

    private static int CreateSink(string name)
    {
        Process process = Process.Start(new ProcessStartInfo
        {
            FileName = "pactl",
            Arguments = $"load-module module-null-sink sink_name={name}",
            UseShellExecute = false,
            RedirectStandardOutput = true
        })!;
        using StreamReader reader = new(process.StandardOutput.BaseStream);
        return int.Parse(reader.ReadToEnd());
    }

    private static void DeleteSink(int id)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "pactl",
            Arguments = $"unload-module {id}",
            UseShellExecute = false
        })!.WaitForExit();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Commands.Play.ActiveGuilds.Remove(_guildId);
        _player.Dispose();
        _outputProcess.Kill();
        DeleteSink(_sinkId);
    }
}