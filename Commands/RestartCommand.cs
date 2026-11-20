using Fluxer.Net;
using Fluxer.Net.Commands;

public class RestartModule : ModuleBase
{
    [Command("restart")]
    [Alias("reset")]
    public async Task Restart()
    {
        if (!GameStore.Games.TryGetValue(Context.User.Id, out var session))
        {
            var prefix = PrefixStore.Get(Context.Guild?.Id ?? 0);
            await ReplyAsync($"No active game. Use `{prefix}play` to start one.");
            return;
        }

        session.Game.Restart();
        GameStore.Storage?.SaveGame(session.UserId, session.Game);
        await BoardMessage.Edit(Context.Channel, session, $"♻️ Level {session.Game.Level} restarted");
    }
}
