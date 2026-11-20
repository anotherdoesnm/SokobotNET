using System.Text;

/// <summary>
/// The command list in !info, kept out of the command module so it can be tested: the
/// prefix it prints is per guild, and a list that silently says "!" on a server using
/// "?" is exactly the bug this replaces.
/// </summary>
public static class InfoText
{
    public static string Commands(string prefix) =>
        new StringBuilder()
            .AppendLine($"``{prefix}play`` starts a game, or resumes the one you saved.")
            .AppendLine($"``{prefix}stop`` puts your game aside; the level is kept.")
            .AppendLine($"``{prefix}board`` re-posts your board.")
            .AppendLine($"``{prefix}restart`` rerolls the current level when a box is stuck.")
            .AppendLine($"``{prefix}new`` forgets the saved game and starts over from level 1.")
            .AppendLine($"``{prefix}w`` ``{prefix}a`` ``{prefix}s`` ``{prefix}d`` move, and so do the arrows under the board.")
            .AppendLine($"``{prefix}prefix <chars>`` changes the command prefix here; without an argument it goes back to ``!``.")
            .AppendLine($"``{prefix}info`` shows this.")
            .Append($"``{prefix}help`` does not.")
            .ToString();
}
