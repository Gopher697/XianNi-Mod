# Ergenverse: Cultivation World

Ergenverse: Cultivation World is a WorldBox cultivation mod inspired by Er Gen's xianxia novels.

It is based on the GameBanana Xianni / Cultivate the Way source page:

<https://gamebanana.com/mods/648751>

This Steam Workshop release is maintained by Gopher. It is not official Er Gen content, not an unmodified upload of the GameBanana release, not an official continuation, and not a replacement for the source GameBanana page.

## Core Gameplay

- Adds a cultivation progression layer to WorldBox civilizations and creatures.
- Includes cultivation realms, realm advancement, and dangerous breakthrough attempts.
- Models realm suppression so crossing major power gaps is difficult.
- Supports cultivator progression alongside Ancient God legacy/star progression and beast/demonic progression.
- Adds cultivation-facing UI, statistics, resources, traits, histories, titles, and related world systems from the Xianni base.

## Steam Release Improvements

- English localization and preferred terminology pass.
- Retained and cleaned Simplified Chinese, Traditional Chinese, and Japanese locale files where present.
- UI and panel improvements, including cultivator distribution rows across confirmed population info panels.
- Removed or disabled upstream social/contact/update panels that were unsuitable for this Steam Workshop release.
- Stability and compatibility fixes for the current deployed version.

## Credits And Source

Source / upstream GameBanana page:

<https://gamebanana.com/mods/648751>

Existing repository credit:

KangKang (QQ Group: 923773102)

Steam Workshop release maintained by:

Gopher

## Release Notes

Version: 0.5.0

This release line includes Gopher's fixes, translation choices, UI improvements, compatibility fixes, and Steam Workshop publication prep on top of the Xianni 0.4.2 base.

## Manual Upload Checklist

Local evidence confirms the runtime package should include:

- `XianniMod.dll`
- `mod.json`
- `default_config.json`
- `icon.png`
- `Locales/en.json`
- `Locales/cz.json`
- `Locales/ch.json`
- `Locales/ja.json`
- `GameResources/`
- `Title/`

Local evidence found NML/NeoModLoader files and upload-related class names, plus Steamworks assemblies, but no definitive local upload documentation or command-line upload script. Confirm the upload path in game through NeoModLoader/NeoModManager or the active Workshop UI before publishing.

Before upload:

1. Confirm `mod.json` shows version `0.5.0` and author `KangKang (QQ Group: 923773102), Gopher`.
2. Build with `dotnet build .\XianniMod.csproj -c Release`.
3. Deploy with `.\tools\deploy-local.ps1 -ModDir "<WorldBox>\Mods\Xianni"`.
4. Launch WorldBox and confirm the mod loads without Xianni exceptions or missing locale keys.
5. Confirm the Workshop description clearly says Ergenverse: Cultivation World is a modified release maintained by Gopher, based on the GameBanana Xianni / Cultivate the Way source page.
