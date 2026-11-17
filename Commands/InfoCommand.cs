using Fluxer.Net;
using Fluxer.Net.Commands;
using Fluxer.Net.Rest;
using System.Collections.Generic;
public class InfoCommand : ModuleBase {
    [Command("info")]
    public async Task Info(){
      var embed = new EmbedBuilder()
      .WithTitle("SokobotNET")
      .WithThumbnailUrl(Context.Gateway.CurrentUser?.GetAvatarUrl() ?? Context.Gateway.CurrentUser?.GetDefaultAvatarUrl())
      .WithDescription("SokobotNET is a bot that lets you play Sokoban, the classic box-pushing puzzle game.\n")
      .WithColor(0xDD, 0x2E, 0x53)
      .AddField("How to Play","You are a **Sokoban** :flushed:\n"
      + "Your job is to push **boxes**: :brown_square: on top of their **destinations** :negative_squared_cross_mark:.",false)
      .AddField("Features",":white_small_square:**Infinity levels**\n"
      + "The maps in SokobotNET are randomly generated, increasing in difficulty as you progress.\n"
      + ":white_small_square:**Varied controls**\n"
      + "SokobotNET has multiplie control options to improve the player's experience, including reactions and wasd commands!\n"
      + ":white_small_square:**Simultaneous games**\n"
      + "Thanks to the power of C# Dictionaries:tm:, multiple users can use the bot at the same time without interfering with one another",false)
      .AddField("Commands","``!play`` can be used to start a game if you are not currently in one.\n"
      + "``!stop`` can be used to stop your active game at any time.\n"
      + "``!board`` re-posts your board; ``!restart`` rerolls a level you got stuck in.\n"
      + "``!w`` ``!a`` ``!s`` ``!d`` move, and so do the arrows under the board.\n"
      + "``!prefix <chars>`` changes the command prefix for this server (no argument resets it).\n"
      + "``!info`` provides some useful details about the bot and rules of the game.\n"
      + "``!help`` does not.")
      .AddField("Source code","https://github.com/anotherdoesnm/SokobotNET")
      .WithFooter(footer => footer.Text = "Initially created in Java by PolyMars, ported to C# by doesnm");
      await ReplyAsync(embeds: new List<EmbedRequest>{embed.Build()});
    }
}