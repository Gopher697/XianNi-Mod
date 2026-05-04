# WorldBox Xianni Mod

This repository contains a C# WorldBox mod for the Xianni cultivation overhaul. It is built against WorldBox, NeoModLoader/NML, Harmony, and Unity assemblies.

## Repository Layout

- `code/` contains the C# mod source.
- `Locales/` contains runtime locale JSON files loaded by the mod.
- `GameResources/`, `Title/`, `mod.json`, `default_config.json`, and `icon.png` are runtime package assets.
- `tools/deploy-local.ps1` copies the built DLL and locale files into a local mod folder.
- `build_staging/` contains local build output and intermediates.
- `reports/` contains local smoke-test logs, screenshots, and session closeouts.

`build_staging/` and `reports/` are ignored generated/local artifacts.

## Build Prerequisites

- .NET SDK.
- WorldBox installed locally.
- NeoModLoader/NML installed so the required publicized WorldBox, NeoModLoader, Harmony, and Unity assemblies are available.

Current limitation: `XianniMod.csproj` uses hardcoded reference paths under:

```text
D:\SteamLibrary\steamapps\common\worldbox
```

If WorldBox is installed elsewhere, update the reference paths in `XianniMod.csproj` before building. A future cleanup should replace these hardcoded paths with a configurable `WorldBoxDir`.

## Build

```powershell
dotnet build .\XianniMod.csproj -c Release
```

The build output is:

```text
build_staging\XianniMod.dll
```

## Deploy

Deploy the current DLL and runtime locale files to an existing Xianni mod directory:

```powershell
.\tools\deploy-local.ps1 -ModDir "<WorldBox>\Mods\xianni"
```

The deploy helper:

- copies `build_staging\XianniMod.dll` to `XianniMod.dll`;
- copies all repository `Locales\*.json` files to the target `Locales\` folder;
- does not copy reports or build intermediates.

For a full clean install, ensure the target mod folder also has the runtime package assets: `GameResources/`, `Title/`, `mod.json`, `default_config.json`, and `icon.png`.

## Local Smoke Workflow

Current UI smoke automation assumes fullscreen WorldBox at `2560x1440` with Windows display scaling at `100%`.

Ignored `reports/` artifacts are used for smoke screenshots, logs, and session closeouts. Translation-memory and smoke reports are local workflow artifacts, not release package output.

## Known Follow-Ups

- Replace hardcoded WorldBox reference paths with a configurable `WorldBoxDir`.
- Decide whether root translation patch/report artifacts should remain tracked.
- Add fixture/setup notes for populated bloodline family/member/talent validation.
- Treasure display is manually validated, but a repeatable automated treasure grant route is still open.
