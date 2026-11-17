using Fluxer.Net;
using Fluxer.Net.Commands;

public class GameModule : ModuleBase
{
    [Command("play")]
    public async Task Play()
    {
        var session = new Session { Game = new Game() };

        if (!GameStore.Games.TryAdd(Context.User.Id, session))
        {
            await ReplyAsync(
                "You already have an active game.\nUse `!stop` to stop your current game first."
            );
            return;
        }

        await BoardMessage.Send(Context.Channel, session);
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
        if (GameStore.Games.TryGetValue(Context.User.Id, out var session))
        {
            // Re-post and re-attach arrows: the old message may be gone or out of sight.
            await BoardMessage.Send(Context.Channel, session);
        }
        else
        {
            await ReplyAsync("No active game. Use `!play` to start one.");
        }
    }
}
