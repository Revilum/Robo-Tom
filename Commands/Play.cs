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

    private Play(IDiscordInteraction cmd)
    {
        _player = new DiscordMediaPlayer(cmd.GuildId.ToString()!, _cancelToken);
    }

    public static async Task PlayInGuild(SocketSlashCommand cmd)
    {
        ulong guildId = (ulong)cmd.GuildId!;
        string query = cmd.Data.Options.First().Value.ToString()!;
        YouTube playable = new(query);
        if (ActiveGuilds.TryGetValue(guildId, out Play party))
        {
            party._player.Queue.AddToQueue(playable);
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
    
    private async Task PlayStream(IAudioChannel vc, Playable.Playable toPlay)
    {
        _player.Queue.AddToQueue(toPlay);
        IAudioClient audioClient = await vc.ConnectAsync();
        
        await using AudioOutStream discord = audioClient.CreatePCMStream(AudioApplication.Music);
        try
        {
            await _player.PlayNext();
            await _player.AudioOutputStream.CopyToAsync(discord, _cancelToken.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Playback was canceled");
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