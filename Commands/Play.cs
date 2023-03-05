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
    private readonly DiscordMediaPlayer _player;

    private Play(ulong guildId)
    {
        _player = new DiscordMediaPlayer(guildId, _cancelToken);
    }

    public static async Task<Play?> GetInstance(SocketSlashCommand cmd)
    {
        ulong guildId = (ulong)cmd.GuildId!;
        if (ActiveGuilds.TryGetValue(guildId, out Play instance))
        {
            return instance;
        }
        IVoiceChannel? vc = GetVoiceChannelFromUser(cmd);
        if (vc == null)
            return null;
        
        Play newInstance = new(guildId);
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
        
        Play? instance = await GetInstance(cmd);
        if (instance == null)
        {
            await cmd.ModifyOriginalResponseAsync(x => x.Content = "Please join a voice channel.");
            return;
        }
        instance._player.Queue.AddToQueue(playable);
        await cmd.ModifyOriginalResponseAsync(x => x.Embed = playable.ToEmbed());
        await instance._player.PlayNext();

    }
    
    private async Task JoinVoiceChannel(IAudioChannel vc)
    {
        IAudioClient audioClient = await vc.ConnectAsync();
        
        await Tools.RunInBackGround(async () =>
        {
            await using AudioOutStream discord = audioClient.CreatePCMStream(AudioApplication.Music);
            try
            {
                _player.Play();
                await _player.AudioOutputStream.CopyToAsync(discord, _cancelToken.Token);
            }
            catch (OperationCanceledException) {}
            finally
            {
                await discord.FlushAsync();
                await vc.DisconnectAsync();
            }
        });
    }

    private static IVoiceChannel? GetVoiceChannelFromUser(IDiscordInteraction cmd)
    {
        return RoboTom.Client.GetGuild((ulong)cmd.GuildId!).VoiceChannels.FirstOrDefault(voiceChannel =>
            voiceChannel.ConnectedUsers.Any(voiceUser => voiceUser.Id == cmd.User.Id));
    }
}