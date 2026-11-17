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
            await ReplyAsync("No active game. Use `!play` to start one.");
            return;
        }

        session.Game.Restart();
        await BoardMessage.Edit(Context.Channel, session, $"♻️ Level {session.Game.Level} restarted");
    }
}
