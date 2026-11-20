using Fluxer.Net;
using Fluxer.Net.Commands;
using Fluxer.Net.Rest;
using System.Collections.Generic;
using System.Linq;

public class TopModule : ModuleBase
{
    private const int Limit = 10;

    [Command("top")]
    [Alias("leaderboard", "ladder")]
    public async Task Top()
    {
        var guildId = Context.Guild?.Id ?? 0;
        var prefix = PrefixStore.Get(guildId);
        var storage = GameStore.Storage;

        if (storage is null)
        {
            await ReplyAsync("The leaderboard is unavailable: this bot runs without a database.");
            return;
        }

        var rows = storage.Top(guildId, Limit);
        if (rows.Count == 0)
        {
            await ReplyAsync($"Nobody here has cleared a level yet. `{prefix}play` and be the first.");
            return;
        }

        var plays = storage.GuildPlayerCount(guildId);
        var lines = rows.Select((row, index) =>
            $"`{index + 1}.` <@{row.UserId}> — **L{row.BestLevel}** · "
            + $"{row.Cleared} cleared · {row.AverageMoves:0.0} moves/level");

        var embed = new EmbedBuilder()
            .WithTitle($"🏆 {Context.Guild?.Name ?? "Leaderboard"}")
            .WithDescription(string.Join('\n', lines))
            .WithColor(0xDD, 0x2E, 0x53)
            .WithFooter(footer => footer.Text =
                $"furthest level first, then the lowest average · "
                + $"{plays} {InfoCommand.Plural(plays, "player", "players")}");

        await ReplyAsync(embeds: new List<EmbedRequest> { embed.Build() });
    }
}
