using Fluxer.Net;
using Fluxer.Net.Commands;

public class PrefixModule : ModuleBase
{
    [Command("prefix")]
    [Alias("setprefix")]
    public async Task Prefix(string? value = null)
    {
        ulong guildId = Context.Guild?.Id ?? 0;

        if (string.IsNullOrEmpty(value))
        {
            PrefixStore.Set(guildId, PrefixStore.Default);
            await ReplyAsync(
                $"Prefix reset to `{PrefixStore.Default}`. Example: `{PrefixStore.Default}play`."
            );
            return;
        }

        if (!PrefixStore.IsValid(value))
        {
            await ReplyAsync("Prefix must be 1-3 characters with no spaces. Example: `!prefix ?`");
            return;
        }

        PrefixStore.Set(guildId, value);
        await ReplyAsync(
            $"Prefix is now `{value}`. Example: `{value}play`. "
            + $"`{value}prefix` with no argument puts it back to `{PrefixStore.Default}`."
        );
    }
}
