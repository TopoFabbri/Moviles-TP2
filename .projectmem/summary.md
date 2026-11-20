# projectmem - Moviles-TP2

_Last updated: 2026-11-20_

## Project purpose
Replace this placeholder with a concise description of what this project does, who it serves, and the main technologies or runtime assumptions.

## Recent issues
- [DONE] #legacy_f907 Legacy issue: Attempted fix IAP (fixed)
- [DONE] #legacy_c2fa Legacy issue: Fixed in-app purchase product ID to match Google Play Console (fixed)
- [DONE] #legacy_b846 Legacy issue: Fixed store buttons text overflow (fixed)
- [DONE] #legacy_b1b4 Legacy issue: Fixed cloud save sync and updated build (fixed)
- [DONE] #legacy_1edf Legacy issue: Fix LevelPlay ad load retries, AfterSceneLoad init, and rewarded callback ordering (fixed)

## Decisions
- No decisions logged yet.

## Notes
- Refactored gyro controls to use AttitudeSensor with GravitySensor fallback
- Updated version to 1.1.0 and calibrated accelerometer controls
- Added purchasing package
- Added no ads IAP
- Updated version code for internal release
- Updated minor bundle version to 1.2.0
- Added diagnostic log service and tests
- Added native Android share sheet report export
- Wired diagnostic report buttons in UI and scenes
- Instrumented cloud save, ads, and IAP failures

## Key files
- `.gitignore`
- `Moviles-TP2-Unity/Assets/Adaptive Performance/Settings/Basic Provider Settings.asset`
- `Moviles-TP2-Unity/Assets/Adaptive Performance/Settings/Basic Provider Settings.asset.meta`
- `Moviles-TP2-Unity/Assets/Art/UI/Font/digital_7/digital-7 (mono italic) SDF.asset`
- `Moviles-TP2-Unity/Assets/Art/UI/Font/digital_7/digital-7 (mono) SDF.asset`
- `Moviles-TP2-Unity/Assets/Plugins/Roslyn.meta`
- `Moviles-TP2-Unity/Assets/Plugins/Roslyn/Microsoft.CodeAnalysis.CSharp.dll`
- `Moviles-TP2-Unity/Assets/Plugins/Roslyn/Microsoft.CodeAnalysis.CSharp.dll.meta`
- `Moviles-TP2-Unity/Assets/Plugins/Roslyn/Microsoft.CodeAnalysis.dll`
- `Moviles-TP2-Unity/Assets/Plugins/Roslyn/Microsoft.CodeAnalysis.dll.meta`
- `Moviles-TP2-Unity/Assets/Plugins/Roslyn/System.Collections.Immutable.dll`
- `Moviles-TP2-Unity/Assets/Plugins/Roslyn/System.Collections.Immutable.dll.meta`
- `Moviles-TP2-Unity/Assets/Plugins/Roslyn/System.Reflection.Metadata.dll`
- `Moviles-TP2-Unity/Assets/Plugins/Roslyn/System.Reflection.Metadata.dll.meta`
- `Moviles-TP2-Unity/Assets/Plugins/Roslyn/System.Runtime.CompilerServices.Unsafe.dll`
- `Moviles-TP2-Unity/Assets/Plugins/Roslyn/System.Runtime.CompilerServices.Unsafe.dll.meta`
- `Moviles-TP2-Unity/Assets/Scenes/UI/MainMenu.unity`
- `Moviles-TP2-Unity/Assets/Settings/Mobile_RPAsset.asset`
- `Moviles-TP2-Unity/Assets/Settings/UniversalRenderPipelineGlobalSettings.asset`
- `Moviles-TP2-Unity/Packages/manifest.json`

## Open questions
- None logged yet.
