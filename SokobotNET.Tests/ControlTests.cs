using Xunit;

/// <summary>
/// The reaction path failed silently once: SocketReaction.Emoji is an IEmoji whose
/// ToString() is the type name, so every arrow was rejected before any move. These
/// tests pin the mapping down to the glyph Fluxer actually sends.
/// </summary>
public class ControlTests
{
    [Theory]
    [InlineData("⬆️", 0, -1)]
    [InlineData("⬆", 0, -1)]
    [InlineData("⬅️", -1, 0)]
    [InlineData("⬇️", 0, 1)]
    [InlineData("➡️", 1, 0)]
    public void Arrow_BecomesADirection(string emoji, int dx, int dy)
    {
        var direction = BoardMessage.Direction(emoji);
        Assert.NotNull(direction);
        Assert.Equal((dx, dy), direction!.Value);
    }

    [Theory]
    [InlineData("😳")]
    [InlineData("👍")]
    [InlineData("")]
    [InlineData(null)]
    public void AnythingElse_IsIgnored(string? emoji)
    {
        Assert.Null(BoardMessage.Direction(emoji));
    }

    [Fact]
    public void ControlsMatchTheDirectionTable()
    {
        Assert.Equal(4, BoardMessage.Controls.Length);
        for (int i = 0; i < BoardMessage.Controls.Length; i++)
            Assert.Equal(BoardMessage.Directions[i], BoardMessage.Direction(BoardMessage.Controls[i]));
    }
}
