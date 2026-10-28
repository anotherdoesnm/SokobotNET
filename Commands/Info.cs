using Fluxer.Net;
using Fluxer.Net.Commands;
using Fluxer.Net.Rest;
using System.Collections.Generic;
public class InfoCommand : ModuleBase {
    [Command("info")]
    public async Task Info(){
      var embed = new EmbedBuilder()
      .WithTitle("SokobotNET")
      .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
      .WithDescription("Sokobot is a bot that lets you play Sokoban, the classic box-pushing puzzle game.\n")
      .WithColor(0xDD, 0x2E, 0x53)
      .AddField("How to Play","You are a **Sokoban** :flushed:");
      await ReplyAsync(embeds: new List<EmbedRequest>{embed.Build()});
    }
}