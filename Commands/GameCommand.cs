using Fluxer.Net;
using Fluxer.Net.Commands;

public class GameModule : ModuleBase
{
    [Command("play")]
    public async Task Play()
    {
        if (!GameStore.Games.TryAdd(Context.User.Id, new Game()))
        {
            await ReplyAsync(
                "You already have an active game.\nUse `!stop` to stop your current game first."
            );
            return;
        }
        await ReplyAsync(embeds: BoardRender.Build(GameStore.Games[Context.User.Id]));
    }

    [Command("stop")]
    public async Task Stop()
    {
        if (GameStore.Games.TryRemove(Context.User.Id, out _))
            await ReplyAsync("Game stopped. Progress lost, as all things eventually are.");
        else
            await ReplyAsync("You don't have an active game. Use `!play` to start one.");
    }

    [Command("board")]
    public async Task Board()
    {
        if (GameStore.Games.TryGetValue(Context.User.Id, out var game))
            await ReplyAsync(embeds: BoardRender.Build(game));
        else
            await ReplyAsync("No active game. Use `!play` to start one.");
    }
}
