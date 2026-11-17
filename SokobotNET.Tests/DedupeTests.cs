using Xunit;

public class DedupeTests
{
    [Fact]
    public void AMessageIsHandledOnce()
    {
        ulong id = 900_000 + (ulong)System.DateTime.UtcNow.Ticks % 1000;
        Assert.True(Events.FirstMessage(id));
        Assert.False(Events.FirstMessage(id));
    }

    [Fact]
    public void TheSameArrowIsSwallowedBriefly_ButCanBePressedAgain()
    {
        ulong message = 4242 + (ulong)System.DateTime.UtcNow.Ticks % 1000;
        const ulong user = 7;

        Assert.True(Events.FirstReaction(message, user, "⬇️"));
        Assert.False(Events.FirstReaction(message, user, "⬇️"));
        // A different direction is a different event.
        Assert.True(Events.FirstReaction(message, user, "⬆️"));
    }
}
