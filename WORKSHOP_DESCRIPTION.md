# Er Gen: Cultivation World

Er Gen: Cultivation World is a modified Steam Workshop release based on the GameBanana Xianni / Cultivate the Way mod page:

<https://gamebanana.com/mods/648751>

## Credits

Source mod page: <https://gamebanana.com/mods/648751>

Existing repository credit field: KangKang (QQ Group: 923773102).

This Steam Workshop release is a modified version maintained by Gopher. It is planned to expand toward broader Er Gen novel-inspired cultivation content, but it is not official Er Gen content, not an unmodified upload of the GameBanana release, not an official continuation, and not a replacement for the source GameBanana page.

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
5. Confirm the Workshop description clearly says Er Gen: Cultivation World is a modified release maintained by Gopher, based on the GameBanana Xianni / Cultivate the Way source page.
