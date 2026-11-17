using Fluxer.Net;
using Fluxer.Net.Rest;
using System.Collections.Generic;

/// <summary>
/// Renders the board as an embed, the way the original Java Sokobot did. The plain
/// text reply worked, but an embed gives the level a title and keeps the controls in
/// the footer instead of repeating them in every message.
/// </summary>
public static class BoardRender
{
    public const string Controls = "`!w` up · `!a` left · `!s` down · `!d` right";

    public static List<EmbedRequest> Build(Game game, string? note = null)
    {
        var lines = game.ToString().Split('\n', System.StringSplitOptions.RemoveEmptyEntries);
        // The last line of ToString() is the "Level N • Moves M" caption: the title and
        // footer carry that now, so it would be said twice.
        var board = string.Join('\n', lines, 0, lines.Length - 1);

        var embed = new EmbedBuilder()
            .WithTitle($"Level {game.Level}")
            .WithDescription(board)
            .WithColor(0xDD, 0x2E, 0x53)
            .WithFooter(footer => footer.Text = note ?? $"Moves {game.Moves} · {Controls}");

        return new List<EmbedRequest> { embed.Build() };
    }
}
