using Fluxer.Net;
using Fluxer.Net.Commands;

public class GameModule : ModuleBase
{
    [Command("play")]
    public async Task Play()
    {
        var userId = Context.User.Id;
        var guildId = Context.Guild?.Id ?? 0;
        var prefix = PrefixStore.Get(guildId);

        if (GameStore.Games.ContainsKey(userId))
        {
            await ReplyAsync(
                $"You already have an active game.\n"
                + $"Use `{prefix}board` to show it again, or `{prefix}stop` to put it aside."
            );
            return;
        }

        // A saved board means the player stopped earlier (or the bot restarted): resume it
        // instead of throwing the level away. `!new` is the way to start over on purpose.
        var saved = GameStore.Storage?.LoadGame(userId);
        var session = new Session
        {
            Game = saved ?? new Game(),
            UserId = userId,
            GuildId = guildId,
        };

        if (!GameStore.Games.TryAdd(userId, session))
        {
            await ReplyAsync($"You already have an active game.\nUse `{prefix}board` to show it again.");
            return;
        }

        GameStore.Storage?.SaveGame(userId, session.Game);
        await BoardMessage.Send(Context.Channel, session, saved is null ? null : "▶️ resumed where you left off");
    }

    [Command("stop")]
    public async Task Stop()
    {
        var userId = Context.User.Id;
        var prefix = PrefixStore.Get(Context.Guild?.Id ?? 0);

        if (GameStore.Games.TryRemove(userId, out var session))
        {
            // The board is already saved after every move; save once more so a stop right
            // after a resumed game cannot leave an older version behind.
            GameStore.Storage?.SaveGame(userId, session.Game);
            await ReplyAsync($"Game stopped. Progress saved — `{prefix}play` puts you back on that level.");
            return;
        }

        if (GameStore.Storage?.LoadGame(userId) is not null)
            await ReplyAsync($"No game is running, but a saved one is waiting: `{prefix}play`.");
        else
            await ReplyAsync($"You don't have an active game. Use `{prefix}play` to start one.");
    }

    [Command("new")]
    [Alias("newgame")]
    public async Task New()
    {
        var userId = Context.User.Id;
        var guildId = Context.Guild?.Id ?? 0;

        GameStore.Games.TryRemove(userId, out _);
        GameStore.Storage?.ForgetGame(userId);

        var session = new Session { Game = new Game(), UserId = userId, GuildId = guildId };
        GameStore.Games[userId] = session;
        GameStore.Storage?.SaveGame(userId, session.Game);

        await BoardMessage.Send(Context.Channel, session, "🆕 level 1, a fresh start");
    }

    [Command("board")]
    public async Task Board()
    {
        var prefix = PrefixStore.Get(Context.Guild?.Id ?? 0);

        if (GameStore.Games.TryGetValue(Context.User.Id, out var session))
        {
            // Re-post and re-attach arrows: the old message may be gone or out of sight.
            await BoardMessage.Send(Context.Channel, session);
        }
        else
        {
            await ReplyAsync($"No active game. Use `{prefix}play` to start one.");
        }
    }
}
