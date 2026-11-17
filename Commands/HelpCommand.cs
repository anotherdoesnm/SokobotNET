using Fluxer.Net;
using Fluxer.Net.Commands;

public class HelpModule : ModuleBase
{
    [Command("help")]
    public async Task Help()
    {
        await ReplyAsync(
            "**Commands**\n"
            + "`!play` — start a game\n"
            + "`!stop` — stop your active game\n"
            + "`!board` — re-render your board\n"
            + "`!restart` — same level, new layout (use it when a box is stuck)\n"
            + "`!w` `!a` `!s` `!d` (or `!up` `!left` `!down` `!right`) — move 😳\n"
            + "`!info` — about the bot\n"
            + "Push every 🟫 onto a ❎. 🟩 means a box is home."
        );
    }
}
