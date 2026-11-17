using DotNetEnv;
using Fluxer.Net;
using Fluxer.Net.Commands;
using Fluxer.Net.Gateway;
using System.Reflection;
using System.Runtime.InteropServices;

public class Program
{
    static async Task Main(string[] args)
    {
        Env.Load();

        var token = Environment.GetEnvironmentVariable("FLUXER_TOKEN")
            ?? throw new InvalidOperationException("FLUXER_TOKEN not found");

        // Unset means "whatever this build of Fluxer.Net talks to by default" (fluxer.app);
        // self-hosted instances pass their API root.
        var instance = Environment.GetEnvironmentVariable("FLUXER_INSTANCE");

        var client = new FluxerClient($"Bot {token}");
        if (string.IsNullOrWhiteSpace(instance))
            await client.LoginAsync();
        else
            await client.LoginAsync(instance);

        await client.StartAsync();

        // A prefix has to survive a restart, so it lives in a SQLite file.
        var storage = new Storage();
        PrefixStore.Initialize(storage);
        Console.WriteLine($"[sokobot] state file: {storage.FilePath}");

        var commands = new CommandService();
        await commands.AddModulesAsync(Assembly.GetExecutingAssembly());

        client.Gateway.MessageCreated += async (data) =>
        {
            try
            {
                // Ignore messages without an author (system messages, webhooks) and bots.
                if (data.Author == null || data.Author.IsBot)
                    return;

                // Stale gateway sessions deliver the same message more than once; answer once.
                if (!Events.FirstMessage(data.Id))
                    return;

                var prefix = PrefixStore.Get(data.Guild?.Id ?? 0);
                if (data.Content?.StartsWith(prefix) != true)
                    return;

                var context = new CommandContext(client, data);
                var result = await commands.ExecuteAsync(context, prefix.Length);
                if (result.IsSuccess)
                    return;

                if (result.Error is CommandError.BadArgCount or CommandError.ParseFailed
                    or CommandError.UnmetPrecondition or CommandError.Exception)
                {
                    try
                    {
                        await context.Channel.SendMessageAsync($"❌ Error: {result.Error}: {result.ErrorReason}");
                    }
                    catch { }
                }
            }
            catch (Exception error)
            {
                Console.Error.WriteLine($"[sokobot] message handler failed: {error}");
            }
        };

        // Reaction controls: the board carries four arrows, and only the player who owns that
        // board may drive it. Our own additions come back as events too, hence the self-check.
        client.Gateway.MessageReactionAdded += async (channel, _, reaction) =>
        {
            try
            {
                if (reaction.UserId == client.Gateway.CurrentUser?.Id)
                    return;

                if (!Events.FirstReaction(reaction.MessageId, reaction.UserId, reaction.Emoji?.Name))
                    return;

                var owner = GameStore.Games.FirstOrDefault(pair => pair.Value.MessageId == reaction.MessageId);
                if (owner.Value is null || owner.Key != reaction.UserId)
                    return;

                // Fluxer puts the glyph in IEmoji.Name; ToString() would hand back the type name.
                var direction = BoardMessage.Direction(reaction.Emoji?.Name);
                if (direction is null)
                    return;

                Console.WriteLine($"[sokobot] reaction {reaction.Emoji?.Name} from {reaction.UserId} -> "
                    + $"{direction.Value.dx},{direction.Value.dy}");
                await Gameplay.Move(channel, owner.Value, direction.Value.dx, direction.Value.dy);
                await BoardMessage.ClearPlayerReaction(channel, reaction.MessageId, reaction.Emoji?.Name, reaction.UserId);
            }
            catch (Exception error)
            {
                Console.Error.WriteLine($"[sokobot] reaction handler failed: {error}");
            }
        };

        // Close the gateway on shutdown: a session left open keeps receiving events, and the
        // next copy of the bot then answers the same message a second time.
        void shutdown(PosixSignalContext context)
        {
            context.Cancel = true;
            Console.WriteLine("[sokobot] shutting down");
            try { client.Gateway.Dispose(); } catch { }
            Environment.Exit(0);
        }

        using var term = PosixSignalRegistration.Create(PosixSignal.SIGTERM, shutdown);
        using var interrupt = PosixSignalRegistration.Create(PosixSignal.SIGINT, shutdown);

        await Task.Delay(-1);
    }
}
