using Fluxer.Net;
using Fluxer.Net.Commands;

public class RestartModule : ModuleBase
{
    [Command("restart")]
    [Alias("reset")]
    public async Task Restart()
    {
        if (!GameStore.Games.TryGetValue(Context.User.Id, out var game))
        {
            await ReplyAsync("No active game. Use `!play` to start one.");
            return;
        }

        game.Restart();
        await ReplyAsync(embeds: BoardRender.Build(game, $"♻️ Level {game.Level} restarted"));
    }
}
