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
            + "`!w` `!a` `!s` `!d` (or `!up` `!left` `!down` `!right`) — move 😳\n"
            + "`!info` — about the bot\n"
            + "Push every 🟫 onto a ❎. 🟩 means a box is home."
        );
    }
}
