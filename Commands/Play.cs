using Discord;
using Discord.Audio;
using Discord.WebSocket;
using Robo_Tom.Playable;
using Robo_Tom.Utils;

namespace Robo_Tom.Commands;

public class Play
{
    private static readonly Dictionary<ulong, Play> ActiveGuilds = new();
    
    private readonly Queue _queue = new();
    private readonly CancellationTokenSource _cancelToken = new();
    private readonly DiscordMediaPlayer _player;

    private Play(IDiscordInteraction cmd)
    {
        _player = new DiscordMediaPlayer(cmd.GuildId.ToString()!);
    }

    public static async Task PlayInGuild(SocketSlashCommand cmd)
    {
        ulong guildId = (ulong)cmd.GuildId!;
        string query = cmd.Data.Options.First().Value.ToString()!;
        YouTube playable = new(query);
        if (ActiveGuilds.TryGetValue(guildId, out Play party))
        {
            party._queue.AddToQueue(playable);
            await cmd.ModifyOriginalResponseAsync(x => x.Embed = playable.ToEmbed());
        }
        else
        {
            IVoiceChannel? vc = GetVoiceChannelFromUser(cmd);
            if (vc == null)
            {
                await cmd.ModifyOriginalResponseAsync(x => x.Content = "Please join a voice channel.");
                return;
            }
            
            Play newInstance = new(cmd);
            ActiveGuilds.Add(guildId, newInstance);
            await cmd.ModifyOriginalResponseAsync(x => x.Embed = playable.ToEmbed());
            await newInstance.PlayStream(vc, playable);
        }
        
    }
    
    private async Task PlayStream(IVoiceChannel vc, Playable.Playable toPlay)
    {
        _queue.AddToQueue(toPlay);
        
        IAudioClient audioClient = await vc.ConnectAsync();
        
        
        await using AudioOutStream discord = audioClient.CreatePCMStream(AudioApplication.Music);
        try
        {
            foreach (Playable.Playable playable in _queue)
            {
                _player.PlayStream(await playable.GetStream());
                await _player.AudioOutputStream.CopyToAsync(discord, _cancelToken.Token);
            }
        }
        finally
        {
            await discord.FlushAsync();
            await vc.DisconnectAsync();
        }
    }

    private static IVoiceChannel? GetVoiceChannelFromUser(IDiscordInteraction cmd)
    {
        return RoboTom.Instance.Client.GetGuild((ulong)cmd.GuildId!).VoiceChannels.FirstOrDefault(voiceChannel =>
            voiceChannel.ConnectedUsers.Any(voiceUser => voiceUser.Id == cmd.User.Id));
    }
}