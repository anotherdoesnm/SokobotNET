using Fluxer.Net;
using Fluxer.Net.Commands;

public class MoveModule : ModuleBase
{
    [Command("w")]
    [Alias("up")]
    public Task Up() => Move(0, -1);

    [Command("s")]
    [Alias("down")]
    public Task Down() => Move(0, 1);

    [Command("a")]
    [Alias("left")]
    public Task Left() => Move(-1, 0);

    [Command("d")]
    [Alias("right")]
    public Task Right() => Move(1, 0);

    private async Task Move(int dx, int dy)
    {
        if (!GameStore.Games.TryGetValue(Context.User.Id, out var session))
        {
            await ReplyAsync("No active game. Use `!play` to start one.");
            return;
        }

        await Gameplay.Move(Context.Channel, session, dx, dy);
    }
}
