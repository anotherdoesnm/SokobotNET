using Fluxer.Net;
using System.Threading.Tasks;

/// <summary>
/// A single move, shared by the text commands and the reaction handler, so both behave
/// identically: the same push rules, the same win handling, the same message edit.
/// </summary>
public static class Gameplay
{
    /// <summary>How long the finished board stays visible before the next one appears.</summary>
    public const int LevelRolloverMs = 1200;

    public static async Task Move(Channel channel, Session session, int dx, int dy, ulong? moveMessageId = null)
    {
        var game = session.Game;
        var prefix = session.Prefix;

        if (!game.TryMove(dx, dy, out bool won))
        {
            var reason = game.LastBlock switch
            {
                Game.BlockReason.BoxBehind => "🚧 there is another box behind that one - one box at a time",
                Game.BlockReason.Wall => "🚧 that way is a wall",
                _ => "🚧 you can't move there",
            };
            await BoardMessage.Edit(channel, session, reason);
            return;
        }

        if (won)
        {
            // Same message for the whole game: show the finished board, then roll it over
            // to the next level in place. A new message per level buried the old one and
            // left stale arrows behind.
            int cleared = game.Level;
            // Score before NextLevel() resets the move counter for the new board.
            GameStore.Storage?.RecordClear(session.GuildId, session.UserId, cleared, game.Moves);
            await BoardMessage.Edit(channel, session, $"🎉 Level {cleared} complete");
            game.NextLevel();
            await BoardMessage.Edit(channel, session);
        }
        else
        {
            await BoardMessage.Edit(channel, session);
        }

        // Save after every move, win included: a restart of the bot must not cost a level.
        GameStore.Storage?.SaveGame(session.UserId, game);
    }
}
