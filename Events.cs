using System;
using System.Collections.Generic;

/// <summary>
/// Duplicate suppression. A restart leaves the old gateway session alive for a while on the
/// server, and every session delivers its own copy of an event — the bot then answers the
/// same message twice or thrice. Messages are deduplicated by id forever (bounded), and
/// reactions by (message, user, emoji) for a short window only, because pressing the same
/// arrow again after it is cleared is legitimate.
/// </summary>
public static class Events
{
    private const int Capacity = 512;
    private static readonly TimeSpan ReactionWindow = TimeSpan.FromMilliseconds(1500);

    private static readonly object Gate = new();
    private static readonly Queue<ulong> MessageOrder = new();
    private static readonly HashSet<ulong> Messages = new();
    private static readonly Dictionary<string, DateTime> Reactions = new();

    public static bool FirstMessage(ulong messageId)
    {
        lock (Gate)
        {
            if (!Messages.Add(messageId)) return false;
            MessageOrder.Enqueue(messageId);
            while (MessageOrder.Count > Capacity) Messages.Remove(MessageOrder.Dequeue());
            return true;
        }
    }

    public static bool FirstReaction(ulong messageId, ulong userId, string? emoji)
    {
        var key = $"{messageId}:{userId}:{emoji}";
        var now = DateTime.UtcNow;
        lock (Gate)
        {
            if (Reactions.TryGetValue(key, out var seen) && now - seen < ReactionWindow) return false;
            Reactions[key] = now;
            if (Reactions.Count > Capacity)
            {
                foreach (var stale in new List<string>(Reactions.Keys).FindAll(k => now - Reactions[k] > ReactionWindow))
                    Reactions.Remove(stale);
            }
            return true;
        }
    }
}
