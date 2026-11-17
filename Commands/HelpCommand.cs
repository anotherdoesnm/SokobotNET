using Fluxer.Net;
using Fluxer.Net.Commands;

public class HelpModule : ModuleBase
{
    // A joke, on purpose. The real list of commands lives in !info.
    [Command("help")]
    public async Task Help()
    {
        await ReplyAsync("You called for help. But nobody came");
    }
}
