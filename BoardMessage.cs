using Fluxer.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Sends and edits the board message, and owns the reaction controls attached to it.
/// Editing needs no permissions beyond being the author, unlike reactions — those want
/// Add Reactions, and deleting wants Manage Messages, which is why reactions are the
/// optional part of the feature set.
/// </summary>
public static class BoardMessage
{
    /// <summary>Arrow reactions in board order: up, left, down, right.</summary>
    public static readonly string[] Controls = { "⬆️", "⬅️", "⬇️", "➡️" };

    public static readonly (int dx, int dy)[] Directions = { (0, -1), (-1, 0), (0, 1), (1, 0) };

    /// <summary>Post a fresh board and attach the reaction controls to it.</summary>
    public static async Task Send(Channel channel, Session session, string? note = null)
    {
        var message = await channel.SendMessageAsync(embeds: BoardRender.Build(session.Game, note));
        session.ChannelId = channel.Id;
        session.MessageId = message.Id;

        foreach (var emoji in Controls)
        {
            try { await channel.AddReactionAsync(message.Id, emoji); }
            catch { /* reactions are optional: a server may forbid them */ }
        }
    }

    /// <summary>Rewrite the board in place; falls back to a new message if it is gone.</summary>
    public static async Task Edit(Channel channel, Session session, string? note = null)
    {
        if (session.MessageId == 0)
        {
            await Send(channel, session, note);
            return;
        }

        try
        {
            await channel.EditMessageAsync(session.MessageId, embeds: BoardRender.Build(session.Game, note));
        }
        catch
        {
            // The message was deleted (or reactions/topic purged): start a new one.
            session.MessageId = 0;
            await Send(channel, session, note);
        }
    }

    /// <summary>Which way an emoji points, or null when it is not one of ours.</summary>
    public static (int dx, int dy)? Direction(string emoji)
    {
        for (int i = 0; i < Controls.Length; i++)
            if (emoji.StartsWith(Controls[i][0].ToString()) || emoji == Controls[i])
                return Directions[i];
        return null;
    }
}
