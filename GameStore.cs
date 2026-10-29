using System.Collections.Concurrent;

public static class GameStore
{
    public static ConcurrentDictionary<ulong, Game> Games { get; } = new();
}