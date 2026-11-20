using Fluxer.Net;
using Fluxer.Net.Commands;

public class StatsModule : ModuleBase
{
    [Command("stats")]
    [Alias("me")]
    public async Task Stats()
    {
        var guildId = Context.Guild?.Id ?? 0;
        var userId = Context.User.Id;
        var prefix = PrefixStore.Get(guildId);
        var storage = GameStore.Storage;

        if (storage is null)
        {
            await ReplyAsync("Stats are unavailable: this bot runs without a database.");
            return;
        }

        var row = storage.PlayerRow(guildId, userId);
        if (row is null)
        {
            await ReplyAsync($"Nothing to show yet — you have not cleared a level here. `{prefix}play`.");
            return;
        }

        var rank = storage.Rank(guildId, userId);
        var total = storage.GuildPlayerCount(guildId);

        await ReplyAsync(
            $"**Your record here:** best level **L{row.BestLevel}**, {row.Cleared} cleared, "
            + $"{row.AverageMoves:0.0} moves per level"
            + (rank is null ? "" : $" · place **{rank}** of {total}")
            + $"\n`{prefix}top` shows the whole board.");
    }
}
