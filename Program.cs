using DotNetEnv;
using Fluxer.Net;
using Fluxer.Net.Commands;
using Fluxer.Net.Gateway;
using System.Reflection;
public class Program
{
    static async Task Main(string[] args)
    {
        Env.Load();
        var token = Environment.GetEnvironmentVariable("FLUXER_TOKEN")
    ?? throw new InvalidOperationException("FLUXER_TOKEN not found");
        var _client = new FluxerClient($"Bot {token}");
        var instance = Environment.GetEnvironmentVariable("FLUXER_INSTANCE") ?? "https://fluxer.doesnmlab.xyz/api";
        await _client.LoginAsync(instance);
        await _client.StartAsync();
        var _commands = new CommandService();
        await _commands.AddModulesAsync(Assembly.GetExecutingAssembly());
        string prefix = "!";
        _client.Gateway.MessageCreated += async (data) =>
    {
        // Ignore messages without an author (system messages, webhooks) and exclude bots.
        if (data.Author == null || data.Author.IsBot)
            return;

        // Check if message starts with the prefix
        int argPos = 0;
        if (data.Content?.StartsWith(prefix) == true)
            argPos = prefix.Length;

        if (argPos == 0)
            return;
        
        // Create a command context
        CommandContext context = new CommandContext(_client, data);

        // Execute the command
        IResult result = await _commands.ExecuteAsync(context, argPos);

        if (result.IsSuccess)
            return;

        // Optionally send error message to user
        if (result.Error == CommandError.BadArgCount ||
            result.Error == CommandError.ParseFailed ||
            result.Error == CommandError.UnmetPrecondition ||
            result.Error == CommandError.Exception)
        {
            try
            {
                await context.Channel.SendMessageAsync($"❌ Error: {result.Error}: {result.ErrorReason}");
            }
            catch { }
        }
    };
        await Task.Delay(-1);
    }
}
