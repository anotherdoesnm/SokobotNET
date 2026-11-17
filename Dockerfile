# Build a self-contained binary, then run it on a bare runtime image: the bot needs
# nothing from the SDK at runtime, and the result is ~100 MB with no package manager.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish SokobotNET.csproj -c Release -r linux-x64 --self-contained \
    -p:PublishSingleFile=true -o /out --nologo

FROM mcr.microsoft.com/dotnet/runtime-deps:9.0
RUN useradd --create-home --uid 1000 bot
USER bot
WORKDIR /app
COPY --from=build /out/SokobotNET /app/SokobotNET
ENTRYPOINT ["/app/SokobotNET"]
