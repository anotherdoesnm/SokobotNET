using System.Collections.Concurrent;
using System.Linq;

/// <summary>
/// The command prefix, remembered per guild. Guild 0 is the bucket for direct messages,
/// where there is no guild to key on. A prefix lives in memory only: a restart puts every
/// guild back on the default, which is the safer failure for a command prefix.
/// </summary>
public static class PrefixStore
{
    public const string Default = "!";

    private static readonly ConcurrentDictionary<ulong, string> Prefixes = new();

    public static string Get(ulong guildId) =>
        Prefixes.TryGetValue(guildId, out var prefix) ? prefix : Default;

    /// <summary>One to three characters, no whitespace: longer prefixes collide with prose.</summary>
    public static bool IsValid(string? prefix) =>
        !string.IsNullOrWhiteSpace(prefix)
        && prefix.Length is >= 1 and <= 3
        && !prefix.Any(char.IsWhiteSpace);

    public static bool Set(ulong guildId, string prefix)
    {
        if (!IsValid(prefix)) return false;
        if (prefix == Default)
        {
            Prefixes.TryRemove(guildId, out _);
            return true;
        }
        Prefixes[guildId] = prefix;
        return true;
    }

    public static void Reset(ulong guildId) => Prefixes.TryRemove(guildId, out _);
}
