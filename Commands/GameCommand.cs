using Fluxer.Net;
using Fluxer.Net.Commands;

public class GameModule : ModuleBase {
    public Dictionary<ulong,Game> games = new();
    [Command("play")]
    public async Task Play(){
      if(games.ContainsKey(Context.User.Id)){
      await ReplyAsync("You already have an active game.\nUse !stop to stop your current game first");
      }else{
      games.Add(Context.User.Id, new Game());
      await ReplyAsync("Game created");
      }
      Console.WriteLine($"Module: {GetHashCode()}, games: {games.Count}");
    }
}    