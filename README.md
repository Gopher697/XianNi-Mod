# Ergenverse: Cultivation World

This repository contains Gopher's modified and maintained Steam Workshop release of Ergenverse: Cultivation World, a WorldBox cultivation mod inspired by Er Gen's xianxia novels and based on the GameBanana Xianni / Cultivate the Way mod page: <https://gamebanana.com/mods/648751>.

This project is not official Er Gen content, not an unmodified upload of the GameBanana release, not an official continuation, and not a replacement for the source GameBanana page.

## Credits

- Source mod page: <https://gamebanana.com/mods/648751>
- Existing repository credit field: KangKang (QQ Group: 923773102)
- Modified Steam Workshop release: Gopher
- Base version: Xianni 0.4.2

## Repository Layout

- `code/` contains the C# mod source.
- `Locales/` contains runtime locale JSON files loaded by the mod.
- `GameResources/`, `Title/`, `mod.json`, `default_config.json`, and `icon.png` are runtime package assets.
- `WORKSHOP_DESCRIPTION.md` contains publication-facing draft text and upload checklist notes.
- `docs/translation/` contains historical translation archaeology notes and artifacts.
- `tools/deploy-local.ps1` copies the built DLL and runtime package files into a local mod folder.
- `build_staging/` contains local build output and intermediates.
- `reports/` contains local smoke-test logs, screenshots, and session closeouts.

`build_staging/` and `reports/` are ignored generated/local artifacts.

## Build Prerequisites

- .NET SDK.
- WorldBox installed locally.
- NeoModLoader/NML installed so the required publicized WorldBox, NeoModLoader, Harmony, and Unity assemblies are available.

By default, the build looks for WorldBox under:

```text
D:\SteamLibrary\steamapps\common\worldbox
```

If WorldBox is installed elsewhere, pass `WorldBoxDir` on the command line:

```powershell
dotnet build .\XianniMod.csproj -c Release /p:WorldBoxDir="C:\Path\To\worldbox"
```

For a persistent local override, create an untracked `Directory.Build.props.user` file:

```xml
<Project>
  <PropertyGroup>
    <WorldBoxDir>C:\Path\To\worldbox</WorldBoxDir>
  </PropertyGroup>
</Project>
```

Do not commit `Directory.Build.props.user`; it is ignored by Git.

## Build

```powershell
dotnet build .\XianniMod.csproj -c Release
```

The build output is:

```text
build_staging\XianniMod.dll
```

## Deploy

Deploy the current release package to an existing Xianni mod directory:

```powershell
.\tools\deploy-local.ps1 -ModDir "<WorldBox>\Mods\Xianni"
```

The deploy helper copies:

- `build_staging\XianniMod.dll` to `XianniMod.dll`
- `mod.json`
- `default_config.json`
- `icon.png`
- all `Locales\*.json`
- the `GameResources\` runtime asset tree
- the `Title\` runtime title tree

It does not copy `reports/`, build intermediates, or local planning notes.

## Local Smoke Workflow

Current UI smoke automation assumes fullscreen WorldBox at `2560x1440` with Windows display scaling at `100%`.

Ignored `reports/` artifacts are used for smoke screenshots, logs, and session closeouts. Generated translation-memory reports and smoke reports are local workflow artifacts, not release package output.

## Known Follow-Ups

- Consider replacing the default `WorldBoxDir` fallback with documented per-developer setup only.
- Rework Aura/cultivation growth before adding broader Er Gen-inspired feature systems.
- Add fixture/setup notes for populated bloodline family/member/talent validation.
- Treasure display is manually validated, but a repeatable automated treasure grant route is still open.
