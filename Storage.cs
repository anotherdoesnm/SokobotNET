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

        // Per-guild leaderboard: one row per player, updated on every level they clear.
        // Efficiency is the running average of moves per cleared level, so a player who
        // takes a detour does not lose the level they already finished.
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS scores (
                guild_id    INTEGER NOT NULL,
                user_id     INTEGER NOT NULL,
                best_level  INTEGER NOT NULL,
                cleared     INTEGER NOT NULL,
                total_moves INTEGER NOT NULL,
                updated_at  INTEGER NOT NULL,
                PRIMARY KEY (guild_id, user_id)
            )
            """;
        command.ExecuteNonQuery();

        // Where the bot has been seen, so "in N guilds" survives a restart even before
        // the gateway announces every guild again.
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS guilds (
                guild_id   INTEGER PRIMARY KEY,
                first_seen INTEGER NOT NULL
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

    /// <summary>A cleared level: bump the counter, remember the best level and the moves.</summary>
    public void RecordClear(ulong guildId, ulong userId, int level, int moves)
    {
        if (guildId == 0) return; // no leaderboard for DMs

        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO scores (guild_id, user_id, best_level, cleared, total_moves, updated_at)
            VALUES ($guild, $user, $level, 1, $moves, $now)
            ON CONFLICT(guild_id, user_id) DO UPDATE SET
                best_level  = MAX(scores.best_level, excluded.best_level),
                cleared     = scores.cleared + 1,
                total_moves = scores.total_moves + excluded.total_moves,
                updated_at  = excluded.updated_at
            """;
        command.Parameters.AddWithValue("$guild", (long)guildId);
        command.Parameters.AddWithValue("$user", (long)userId);
        command.Parameters.AddWithValue("$level", level);
        command.Parameters.AddWithValue("$moves", moves);
        command.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        command.ExecuteNonQuery();
    }

    /// <summary>Top players of one guild: furthest level first, then the lowest average.</summary>
    public List<GuildScore> Top(ulong guildId, int limit = 10)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT user_id, best_level, cleared, total_moves
            FROM scores WHERE guild_id = $guild
            ORDER BY best_level DESC, (total_moves * 1.0 / cleared) ASC, user_id ASC
            LIMIT $limit
            """;
        command.Parameters.AddWithValue("$guild", (long)guildId);
        command.Parameters.AddWithValue("$limit", limit);

        var rows = new List<GuildScore>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
            rows.Add(new GuildScore((ulong)reader.GetInt64(0), reader.GetInt32(1),
                reader.GetInt32(2), reader.GetInt64(3)));
        return rows;
    }

    /// <summary>The player's own row, or null when they have not cleared anything yet.</summary>
    public GuildScore? PlayerRow(ulong guildId, ulong userId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT user_id, best_level, cleared, total_moves
            FROM scores WHERE guild_id = $guild AND user_id = $user
            """;
        command.Parameters.AddWithValue("$guild", (long)guildId);
        command.Parameters.AddWithValue("$user", (long)userId);
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;
        return new GuildScore((ulong)reader.GetInt64(0), reader.GetInt32(1),
            reader.GetInt32(2), reader.GetInt64(3));
    }

    /// <summary>1-based place in the guild, or null when the player has no row.</summary>
    public int? Rank(ulong guildId, ulong userId)
    {
        var row = PlayerRow(guildId, userId);
        if (row is null) return null;

        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*) FROM scores
            WHERE guild_id = $guild
              AND (best_level > $level
                   OR (best_level = $level AND (total_moves * 1.0 / cleared) < $avg)
                   OR (best_level = $level AND (total_moves * 1.0 / cleared) = $avg AND user_id < $user))
            """;
        command.Parameters.AddWithValue("$guild", (long)guildId);
        command.Parameters.AddWithValue("$level", row.BestLevel);
        command.Parameters.AddWithValue("$avg", (double)row.TotalMoves / row.Cleared);
        command.Parameters.AddWithValue("$user", (long)userId);
        return Convert.ToInt32(command.ExecuteScalar()) + 1;
    }

    /// <summary>Players with at least one cleared level in this guild.</summary>
    public long GuildPlayerCount(ulong guildId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM scores WHERE guild_id = $guild";
        command.Parameters.AddWithValue("$guild", (long)guildId);
        return Convert.ToInt64(command.ExecuteScalar());
    }

    /// <summary>Every level cleared anywhere, for the bot-wide numbers in !info.</summary>
    public long TotalCleared()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COALESCE(SUM(cleared), 0) FROM scores";
        return Convert.ToInt64(command.ExecuteScalar());
    }

    /// <summary>How many guilds the bot has been seen in.</summary>
    public long GuildCount()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM guilds";
        return Convert.ToInt64(command.ExecuteScalar());
    }

    public void RecordGuild(ulong guildId)
    {
        if (guildId == 0) return;

        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO guilds (guild_id, first_seen) VALUES ($guild, $now)
            ON CONFLICT(guild_id) DO NOTHING
            """;
        command.Parameters.AddWithValue("$guild", (long)guildId);
        command.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        command.ExecuteNonQuery();
    }

    /// <summary>Players who have ever joined a game or cleared a level.</summary>
    public long PlayerCount()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*) FROM (
                SELECT user_id FROM games
                UNION
                SELECT user_id FROM scores
            )
            """;
        return Convert.ToInt64(command.ExecuteScalar());
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

/// <summary>One row of a guild leaderboard.</summary>
public sealed record GuildScore(ulong UserId, int BestLevel, int Cleared, long TotalMoves)
{
    /// <summary>Moves spent per cleared level: the tie-breaker after the best level.</summary>
    public double AverageMoves => Cleared == 0 ? 0 : (double)TotalMoves / Cleared;
}
