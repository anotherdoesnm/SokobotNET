# Build a self-contained application directory, then run it on a bare runtime image: the
# bot needs nothing from the SDK at runtime, and shipping the directory (rather than a
# single-file bundle) keeps SQLite's native library where the loader expects it.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish SokobotNET.csproj -c Release -r linux-x64 --self-contained \
    -o /out --nologo

FROM mcr.microsoft.com/dotnet/runtime-deps:9.0
# /data holds the SQLite file with per-guild prefixes; mount a volume there.
RUN useradd --create-home --uid 1000 bot && mkdir -p /data && chown bot:bot /data
ENV SOKOBOT_DB=/data/sokobot.db
VOLUME ["/data"]
USER bot
WORKDIR /app
COPY --from=build /out /app
ENTRYPOINT ["/app/SokobotNET"]
