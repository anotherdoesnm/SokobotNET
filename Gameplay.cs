using Fluxer.Net;
using System.Threading.Tasks;

/// <summary>
/// A single move, shared by the text commands and the reaction handler, so both behave
/// identically: the same push rules, the same win handling, the same message edit.
/// </summary>
public static class Gameplay
{
    public static async Task Move(Channel channel, Session session, int dx, int dy, ulong? moveMessageId = null)
    {
        var game = session.Game;

        if (!game.TryMove(dx, dy, out bool won))
        {
            await BoardMessage.Edit(channel, session, "🚧 you can't move there");
            return;
        }

        if (won)
        {
            int cleared = game.Level;
            await BoardMessage.Edit(channel, session, $"🎉 Level {cleared} complete");
            game.NextLevel();
            await BoardMessage.Send(channel, session);
        }
        else
        {
            await BoardMessage.Edit(channel, session);
        }
    }
}
