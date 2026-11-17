using System.Collections.Concurrent;

/// <summary>
/// One player's game plus the message that shows it. Keeping the message id lets every
/// move edit that message instead of posting a new board, which is how the original bot
/// behaved and what keeps a channel readable.
/// </summary>
public sealed class Session
{
    public required Game Game { get; init; }
    public ulong ChannelId { get; set; }
    public ulong MessageId { get; set; }
}

public static class GameStore
{
    public static ConcurrentDictionary<ulong, Session> Games { get; } = new();
}
