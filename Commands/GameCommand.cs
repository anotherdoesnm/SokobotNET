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
                "You already have an active game.\nUse !stop to stop your current game first"
            );
            return;
        }
        await ReplyAsync("Game created");
    }
}
