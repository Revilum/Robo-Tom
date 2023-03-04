using System.Diagnostics;
using LibVLCSharp.Shared;

namespace Robo_Tom.Utils;

public class DiscordMediaPlayer : IDisposable
{
    private readonly LibVLC _vlc = new();
    private readonly MediaPlayer _player;
    private readonly int _sinkId;
    private readonly Process _outputProcess;
    public readonly Stream AudioOutputStream;

    public DiscordMediaPlayer(string sinkName)
    {
        _sinkId = CreateSink(sinkName);
        _outputProcess = CreateStream(sinkName);
        AudioOutputStream = _outputProcess.StandardOutput.BaseStream;
        
        _player = new MediaPlayer(_vlc);
        _player.SetAudioOutput(sinkName);
    }

    public void PlayStream(Stream stream)
    {
        _player.Play(new Media(_vlc, new StreamMediaInput(stream)));
    }

    public void Play()
    {
        _player.Play();
    }

    public void Pause()
    {
        _player.Pause();
    }

    public void ChangeSpeed(float speed)
    {
        _player.SetRate(speed);
    }

    public void SetVolume(int volume)
    {
        _player.Volume = volume;
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
        });
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _outputProcess.Close();
        _vlc.Dispose();
        DeleteSink(_sinkId);
    }
}