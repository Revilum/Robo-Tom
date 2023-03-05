using Discord;
using Discord.Audio;
using Discord.WebSocket;
using Robo_Tom.Playable;
using Robo_Tom.Utils;

namespace Robo_Tom.Commands;

public class Play
{
    private static readonly Dictionary<ulong, Play> ActiveGuilds = new();
    private readonly CancellationTokenSource _cancelToken = new();
    public readonly DiscordMediaPlayer Player;
    public readonly IVoiceChannel Vc;

    private Play(ulong guildId, IVoiceChannel vc)
    {
        Vc = vc;
        Player = new DiscordMediaPlayer(guildId, _cancelToken);
    }

    public static Play? GetInstance(ulong guildId)
    {
        ActiveGuilds.TryGetValue(guildId, out Play? value);
        return value;
    }

    public static async Task<Play?> CreateInstance(SocketSlashCommand cmd)
    {
        ulong guildId = (ulong)cmd.GuildId!;
        if (ActiveGuilds.TryGetValue(guildId, out Play? instance))
        {
            return instance;
        }
        IVoiceChannel? vc = Tools.GetVoiceChannelFromUser(cmd);
        if (vc == null)
            return null;
        
        Play newInstance = new(guildId, vc);
        ActiveGuilds.Add(guildId, newInstance);
        await newInstance.JoinVoiceChannel(vc);
        return newInstance;
    }

    public static void RemoveInstance(ulong guildId)
    {
        ActiveGuilds.Remove(guildId);
    }

    public static async Task AddSongToQueue(SocketSlashCommand cmd, Playable.Playable? playable = null)
    {
        string query = cmd.Data.Options.First().Value.ToString()!;
        playable ??= new YouTube(query);
        
        Play? instance = await CreateInstance(cmd);
        if (instance == null)
        {
            await cmd.ModifyOriginalResponseAsync(x => x.Content = "Please join a voice channel.");
            return;
        }
        instance.Player.Queue.AddToQueue(playable);
        await cmd.ModifyOriginalResponseAsync(x => x.Embed = playable.ToEmbed());
        await instance.Player.Start();
    }
    
    private async Task JoinVoiceChannel(IAudioChannel vc)
    {
        IAudioClient audioClient = await vc.ConnectAsync();
        
        await Tools.RunInBackGround(async () =>
        {
            await using AudioOutStream discord = audioClient.CreatePCMStream(AudioApplication.Music);
            try
            {
                Player.Play();
                await Player.AudioOutputStream.CopyToAsync(discord, _cancelToken.Token);
            }
            catch (OperationCanceledException) {}
            finally
            {
                await discord.FlushAsync();
                await vc.DisconnectAsync();
            }
        });
    }
}