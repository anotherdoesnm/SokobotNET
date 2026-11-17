# SokobotNET

A Sokoban bot for Fluxer: `!play` drops a board into the channel, arrow reactions or
`!w`/`!a`/`!s`/`!d` move the player, and every move edits the same message instead of
posting a new one. Ported from PolyMars' Java bot, rewritten in C# on .NET 9.

## Commands

| | |
|---|---|
| `!play` | start a game |
| `!w` `!a` `!s` `!d` (`!up` `!left` `!down` `!right`) | move |
| ⬆️ ⬅️ ⬇️ ➡️ | move by reacting to the board |
| `!board` | re-post the board |
| `!restart` | same level, new layout — the escape hatch when a box is stuck |
| `!stop` | stop the game |
| `!info`, `!help` | about, command list |

## Configuration

| variable | |
|---|---|
| `FLUXER_TOKEN` | bot token, `id.secret` (required) |
| `FLUXER_INSTANCE` | API root of a self-hosted instance; unset means fluxer.app |

## Run

```sh
dotnet run                      # local
docker build -t sokobot . && docker run --rm -e FLUXER_TOKEN=... sokobot
```

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
