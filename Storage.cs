using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// The bot's small persistent state: one SQLite file, no server, no migrations library.
/// A prefix has to survive a restart, and a file is enough for that — but it is a real
/// database, so scores or per-user settings can join it later without a rewrite.
/// </summary>
public sealed class Storage
{
    private readonly string _path;
    private readonly Dictionary<ulong, string> _prefixes = new();

    public Storage(string? path = null)
    {
        _path = path
            ?? Environment.GetEnvironmentVariable("SOKOBOT_DB")
            ?? "sokobot.db";

        var directory = Path.GetDirectoryName(Path.GetFullPath(_path));
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS prefixes (
                guild_id INTEGER PRIMARY KEY,
                prefix   TEXT NOT NULL
            )
            """;
        command.ExecuteNonQuery();

        // One row per player, the whole board inside `state`: a level is generated
        // randomly, so there is nothing to regenerate from and the board has to be kept.
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS games (
                user_id    INTEGER PRIMARY KEY,
                state      TEXT NOT NULL,
                updated_at INTEGER NOT NULL
            )
            """;
        command.ExecuteNonQuery();

        command.CommandText = "SELECT guild_id, prefix FROM prefixes";
        using var reader = command.ExecuteReader();
        while (reader.Read())
            _prefixes[(ulong)reader.GetInt64(0)] = reader.GetString(1);
    }

    public string FilePath => _path;

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection($"Data Source={_path}");
        connection.Open();
        return connection;
    }

    public string Prefix(ulong guildId) =>
        _prefixes.TryGetValue(guildId, out var prefix) ? prefix : PrefixStore.Default;

    public void SetPrefix(ulong guildId, string prefix)
    {
        _prefixes[guildId] = prefix;
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO prefixes (guild_id, prefix) VALUES ($guild, $prefix)
            ON CONFLICT(guild_id) DO UPDATE SET prefix = excluded.prefix
            """;
        command.Parameters.AddWithValue("$guild", (long)guildId);
        command.Parameters.AddWithValue("$prefix", prefix);
        command.ExecuteNonQuery();
    }

    public void ClearPrefix(ulong guildId)
    {
        _prefixes.Remove(guildId);
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM prefixes WHERE guild_id = $guild";
        command.Parameters.AddWithValue("$guild", (long)guildId);
        command.ExecuteNonQuery();
    }

    /// <summary>Store the player's board as it is now, replacing any previous save.</summary>
    public void SaveGame(ulong userId, Game game)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO games (user_id, state, updated_at) VALUES ($user, $state, $now)
            ON CONFLICT(user_id) DO UPDATE SET state = excluded.state, updated_at = excluded.updated_at
            """;
        command.Parameters.AddWithValue("$user", (long)userId);
        command.Parameters.AddWithValue("$state", game.Serialize());
        command.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        command.ExecuteNonQuery();
    }

    /// <summary>The saved board, or null when there is none (or it no longer parses).</summary>
    public Game? LoadGame(ulong userId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT state FROM games WHERE user_id = $user";
        command.Parameters.AddWithValue("$user", (long)userId);
        if (command.ExecuteScalar() is not string state) return null;

        try
        {
            return Game.Deserialize(state);
        }
        catch (FormatException)
        {
            // A save written by an incompatible build is worthless; drop it instead of
            // failing every !play from now on.
            ForgetGame(userId);
            return null;
        }
    }

    public void ForgetGame(ulong userId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM games WHERE user_id = $user";
        command.Parameters.AddWithValue("$user", (long)userId);
        command.ExecuteNonQuery();
    }
}
