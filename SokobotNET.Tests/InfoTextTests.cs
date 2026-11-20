using Xunit;

public class InfoTextTests
{
    [Fact]
    public void TheListUsesTheGuildPrefix()
    {
        var text = InfoText.Commands("?");
        Assert.Contains("?play", text);
        Assert.Contains("?restart", text);
        Assert.Contains("?prefix", text);
        Assert.Contains("community's leaderboard", text);
        Assert.DoesNotContain("!play", text);
    }

    [Fact]
    public void TheDefaultPrefixStillReadsNormally()
    {
        var text = InfoText.Commands("!");
        Assert.Contains("!play", text);
        Assert.Contains("!info", text);
    }
}
