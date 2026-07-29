# Dungeon Slime

A 2D game built with [MonoGame](https://monogame.net/) (DesktopGL), targeting Windows, Linux, and macOS.

## Project layout

- `DungeonSlime/` — the game project (entry point, game loop, content references).
- `MonoGameLibrary/` — shared reusable game library (graphics, input abstractions, etc.).
- `Content/` — source assets (`Content/Assets/`) and the custom content pipeline builder project.
- `texturepacker-exporter/` — a [TexturePacker](https://www.codeandweb.com/texturepacker) custom exporter matching this project's texture atlas XML schema.

## Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/) (pinned via `.mise.toml`)

## Running

```
dotnet run --project DungeonSlime
```

## Building for release

Builds for Windows x64, Linux x64, and macOS (Apple Silicon) run automatically in CI on every branch/PR, and are attached to [GitHub Releases](https://github.com/educrod/DungeonSlime/releases) when one is published.

To publish a self-contained build locally:

```
dotnet publish DungeonSlime/DungeonSlime.csproj -c Release -r <RID> --self-contained true -o publish/
```

Where `<RID>` is one of `win-x64`, `linux-x64`, or `osx-arm64`.

## Contributing

Changes to `main` go through pull requests — direct pushes are disabled, and the build must pass on all three platforms before a PR can be merged.
