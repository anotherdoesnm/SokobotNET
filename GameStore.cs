using System.Collections.Concurrent;

/// <summary>
/// One player's game plus the message that shows it. Keeping the message id lets every
/// move edit that message instead of posting a new board, which is how the original bot
/// behaved and what keeps a channel readable.
/// </summary>
public sealed class Session
{
    public required Game Game { get; init; }

    /// <summary>Who is playing: the save file is keyed by this, not by the guild.</summary>
    public required ulong UserId { get; init; }

    /// <summary>Where the prefix comes from, and where scores will belong later.</summary>
    public required ulong GuildId { get; init; }

    public ulong ChannelId { get; set; }
    public ulong MessageId { get; set; }

    public string Prefix => PrefixStore.Get(GuildId);
}

public static class GameStore
{
    public static ConcurrentDictionary<ulong, Session> Games { get; } = new();

    /// <summary>Set once at startup; a null Storage means "play without saving".</summary>
    public static Storage? Storage { get; set; }
}
