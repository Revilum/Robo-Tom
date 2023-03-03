using System.Diagnostics;
using Discord;
using Discord.Audio;
using Discord.WebSocket;
using LibVLCSharp.Shared;
using Robo_Tom.Playable;
using YoutubeExplode;

namespace Robo_Tom.Commands;

public class Play
{
    private static readonly YoutubeClient YtClient = new();
    private static readonly Dictionary<ulong, Play> ActiveGuilds = new();
    
    private ulong GuildId { get; }
    private int SinkId { get; }
    private LibVLC Vlc { get; }

    private Play(SocketSlashCommand cmd)
    {
        GuildId = (ulong)cmd.GuildId!;
        SinkId = CreateSink(GuildId);
        Vlc = new LibVLC();
    }

    public static async Task PlayInGuild(SocketSlashCommand cmd)
    {
        ulong guildId = (ulong)cmd.GuildId!;
        if (ActiveGuilds.ContainsKey(guildId))
        {
            await cmd.RespondAsync("Bot is already playing!");
        }
        else
        {
            Play newInstance = new(cmd);
            ActiveGuilds.Add(guildId, newInstance);
            await newInstance.PlayStream(cmd, new YouTube());
        }
        
    }
    
    private async Task PlayStream(SocketSlashCommand cmd, IPlayable toPlay)
    {
        IVoiceChannel vc = (IVoiceChannel)RoboTom.Instance.Client.GetChannel(437588500263600140);
        IAudioClient audioClient = await vc.ConnectAsync();
        
        using Process rawProcess = CreateStream(GuildId.ToString())!;
        Media media = new(Vlc, new StreamMediaInput(await toPlay.GetStream(cmd.Data.Options.First().Value.ToString()!)));
        MediaPlayer player = new(media);
        player.SetOutputDevice(GuildId.ToString());

        await using Stream output = rawProcess.StandardOutput.BaseStream;
        await using AudioOutStream discord = audioClient.CreatePCMStream(AudioApplication.Music);
        try
        {
            player.Play();
            await output.CopyToAsync(discord);

        }
        finally
        {
            await discord.FlushAsync();
            DeleteSink(SinkId);
            await vc.DisconnectAsync();
        }
    }

    private static Process? CreateStream(string sink)
    {
        return Process.Start(new ProcessStartInfo
        {
            FileName = "parec",
            Arguments = $"-d {sink}.monitor --format=s16le --rate=48000 --channels=2",
            UseShellExecute = false,
            RedirectStandardOutput = true,
        });
    }

    private static int CreateSink(ulong guildId)
    {
        Process process = Process.Start(new ProcessStartInfo
        {
            FileName = "pactl",
            Arguments = $"load-module module-null-sink sink_name={guildId}",
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
}