using Xunit;

public class PrefixTests
{
    [Theory]
    [InlineData("?")]
    [InlineData("..")]
    [InlineData("soko")]
    [InlineData("~")]
    public void ReasonablePrefixes_AreAccepted(string prefix)
    {
        // "soko" is four characters: that is deliberately rejected, see below.
        Assert.Equal(prefix.Length <= 3, PrefixStore.IsValid(prefix));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("! ")]
    [InlineData("abcd")]
    public void Junk_IsRejected(string? prefix)
    {
        Assert.False(PrefixStore.IsValid(prefix));
    }

    [Fact]
    public void SetAndGet_ArePerGuild()
    {
        PrefixStore.Reset(1);
        PrefixStore.Reset(2);

        Assert.True(PrefixStore.Set(1, "?"));
        Assert.Equal("?", PrefixStore.Get(1));
        Assert.Equal("!", PrefixStore.Get(2));
        Assert.Equal("!", PrefixStore.Get(0));

        PrefixStore.Set(1, "..");
        Assert.Equal("..", PrefixStore.Get(1));

        PrefixStore.Reset(1);
        Assert.Equal("!", PrefixStore.Get(1));
    }

    [Fact]
    public void SettingTheDefault_ClearsTheEntry()
    {
        PrefixStore.Set(7, "?");
        Assert.True(PrefixStore.Set(7, PrefixStore.Default));
        Assert.Equal("!", PrefixStore.Get(7));
    }
}
