using System.Linq;

/// <summary>
/// Uses the shared <see cref="Storage"/> for persistence and keeps validation rules here,
/// so a prefix is judged the same way whether it arrives from a command or from a test.
/// </summary>
public static class PrefixStore
{
    public const string Default = "!";

    private static Storage? _storage;

    public static void Initialize(Storage storage) => _storage = storage;

    public static bool IsValid(string? prefix) =>
        !string.IsNullOrWhiteSpace(prefix)
        && prefix.Length is >= 1 and <= 3
        && !prefix.Any(char.IsWhiteSpace);

    public static string Get(ulong guildId) => _storage?.Prefix(guildId) ?? Default;

    public static bool Set(ulong guildId, string prefix)
    {
        if (!IsValid(prefix) || _storage is null) return false;
        if (prefix == Default) { _storage.ClearPrefix(guildId); return true; }
        _storage.SetPrefix(guildId, prefix);
        return true;
    }
}
