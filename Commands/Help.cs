using Fluxer.Net;
using Fluxer.Net.Commands;

public class HelpModule : ModuleBase {
    [Command("help")]
    public async Task Help(){
      await ReplyAsync("You called for help. But nobody came");
    }
}