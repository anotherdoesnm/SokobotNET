# SokobotNET

A Sokoban bot for Fluxer: `!play` drops a board into the channel, arrow reactions or
`!w`/`!a`/`!s`/`!d` move the player, and every move edits the same message instead of
posting a new one. Ported from PolyMars' Java bot, rewritten in C# on .NET 9.

## Adding it to a community

A hosted copy runs on fluxer.com — invite it with:

```
https://fluxer.com/oauth2/authorize?client_id=1572238378789773312&scope=bot&permissions=8192
```

`8192` is **Manage Messages**, and it is asked for exactly one reason: the bot removes
the player's arrow reaction after each move, and removing somebody else's reaction
needs that permission. Sending, embedding and adding reactions come from the default
`@everyone` permissions, so nothing else is requested.

To run your own copy, create a bot account on your instance, set `FLUXER_TOKEN`, and
invite it with your own id:

```
https://<your instance>/oauth2/authorize?client_id=<bot id>&scope=bot&permissions=8192
```

## Commands

| | |
|---|---|
| `!play` | start a game |
| `!w` `!a` `!s` `!d` (`!up` `!left` `!down` `!right`) | move |
| ⬆️ ⬅️ ⬇️ ➡️ | move by reacting to the board |
| `!board` | re-post the board |
| `!restart` | same level, new layout — the escape hatch when a box is stuck |
| `!prefix <chars>` | change the command prefix for this community, no argument resets it |
| `!stop` | put the game aside — the level is saved |
| `!new` | forget the saved game and start over from level 1 |
| `!top` | per-guild leaderboard: furthest level first, lowest average moves breaks ties |
| `!stats` | your best level, cleared count and average moves, with your place |
| `!info` | about the bot and the real command list |
| `!help` | does not |

## Configuration

| variable | |
|---|---|
| `FLUXER_TOKEN` | bot token, `id.secret` (required) |
| `FLUXER_INSTANCE` | API root of a self-hosted instance; unset means fluxer.app |
| `SOKOBOT_DB` | SQLite file for per-guild prefixes and saved games, default `sokobot.db` (the image uses `/data/sokobot.db`) |

## Run

```sh
dotnet run                      # local
docker build -t sokobot .
docker run --rm -e FLUXER_TOKEN=... -v sokobot-data:/data sokobot
```

`/data` holds one SQLite file: per-community prefixes and the players' saved boards. Mount a volume or they reset with
the container.

## Tests

```sh
dotnet test SokobotNET.Tests/SokobotNET.Tests.csproj
```

The suite covers the game rules (walls, pushing, box counts), the generator
(every level is solvable — a BFS solver proves it for the small ones — and no box
starts on its target), the arrow mapping and the level caps. CI runs it on every
push and pull request.

## How levels are made

Random placement gives dead ends, so levels are generated backwards: start with
every box on a target, then walk the player and drag boxes away. A pull is the
inverse of a push, so undoing the generation solves the level. Interior walls
appear from level 4 and are placed before any dragging, which keeps that guarantee.
