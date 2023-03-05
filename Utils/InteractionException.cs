using Discord;

namespace Robo_Tom.Utils;

public class InteractionException : Exception
{
    public readonly IDiscordInteraction Interaction;
    public readonly Exception Exception;

    public InteractionException(IDiscordInteraction interaction, Exception exception)
    {
        Interaction = interaction;
        Exception = exception;
    }
}