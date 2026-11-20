using Fluxer.Net;
using Fluxer.Net.Commands;
using Fluxer.Net.Rest;
using System.Collections.Generic;

public class InfoCommand : ModuleBase
{
    [Command("info")]
    public async Task Info()
    {
        // The prefix is per guild now, so the command list has to spell out the one this
        // server actually uses instead of hardcoding "!".
        var prefix = PrefixStore.Get(Context.Guild?.Id ?? 0);

        var commands = InfoText.Commands(prefix);

        var embed = new EmbedBuilder()
            .WithTitle("SokobotNET")
            .WithThumbnailUrl(Context.Gateway.CurrentUser?.GetAvatarUrl() ?? Context.Gateway.CurrentUser?.GetDefaultAvatarUrl())
            .WithDescription("SokobotNET is a bot that lets you play Sokoban, the classic box-pushing puzzle game.\n")
            .WithColor(0xDD, 0x2E, 0x53)
            .AddField("How to Play", "You are a **Sokoban** :flushed:\n"
                + "Your job is to push **boxes**: :brown_square: on top of their **destinations** :negative_squared_cross_mark:.", false)
            .AddField("Features", ":white_small_square:**Infinity levels**\n"
                + "Levels are generated backwards from a solved position, so every level is solvable and grows harder as you progress.\n"
                + ":white_small_square:**Varied controls**\n"
                + "Move with the arrows under the board or with wasd commands.\n"
                + ":white_small_square:**Simultaneous games**\n"
                + "Thanks to the power of C# Dictionaries:tm:, multiple users can play at the same time without interfering with one another", false)
            .AddField("Commands", commands)
            .AddField("Numbers", InfoNumbers())
            .AddField("Source code", "https://github.com/anotherdoesnm/SokobotNET")
            .WithFooter(footer => footer.Text = "Initially created in Java by PolyMars, ported to C# by doesnm");

        await ReplyAsync(embeds: new List<EmbedRequest> { embed.Build() });
    }

    /// <summary>Bot-wide counters; "no database" should still render something readable.</summary>
    private static string InfoNumbers()
    {
        var storage = GameStore.Storage;
        if (storage is null) return "no database — nothing is counted";

        return $"{storage.GuildCount()} guilds · {storage.PlayerCount()} players · "
            + $"{storage.TotalCleared()} levels cleared";
    }
}
