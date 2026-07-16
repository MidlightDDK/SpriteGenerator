# Runtime RPG Sprite Generator

A local-first Unity capstone project that creates pixel-art RPG characters entirely at runtime. The generator starts with a transparent texture, draws a layered character into its pixel buffer, creates a Unity `Sprite`, and previews it automatically when Play starts.

No source character PNGs, cloud services, or AI image APIs are required.

## Quick start

1. Open `Assets/Scenes/SampleScene.unity` in Unity `6000.3.12f1`.
2. Press Play.
3. A generated character and the runtime control panel appear immediately.

Use **Randomize Unlocked** to create a new controlled variation, lock any layers that should remain unchanged, and use **Export PNG** to save the current sprite. Exports default to the current working folder. The path field accepts pasted paths, while **Browse...** opens the native folder chooser on Windows. The full saved path is shown after export, and **Quit** closes a build or stops Play Mode in the Editor.

## Exposed generation parameters

The `GameManager` component exposes the full configuration in the Inspector, and the most useful controls are also available in Play mode:

- texture width and height (32×32 by default; validated range 8–512);
- pixels per unit and normalized sprite pivot;
- deterministic random seed;
- outline, shadow, and outline thickness;
- head size, body width, and leg length;
- body build, face, hair, outfit, accessory, and palette;
- direct skin, hair, outfit, accessory, eye, and outline colors;
- independent body, face, hair, outfit, accessory, and palette locks;
- preview position, maximum world-space preview size, runtime-panel visibility, and export settings.

The current design space contains at least 51,840 combinations before proportion and direct-color changes are counted.

## Runtime pipeline

```text
Settings + locks
      |
      v
Validation -> Controlled randomization
      |
      v
Transparent pixel canvas (8–512 px per axis)
      |
      v
Shadow -> back accessories/hair -> body -> outfit -> face -> front hair/accessories
      |
      v
Texture2D (Point/Clamp) -> Sprite -> scene preview -> optional PNG export
```

All drawing uses a logical 32×32 coordinate system that is scaled with point sampling and centered without changing its aspect ratio. Bounds-safe raster operations and transparent letterboxing keep tiny, odd, and non-square canvases valid.

## Architecture

Runtime code is under `Assets/Scripts/SpriteGenerator`:

- `Core`: serializable configuration, palettes, enums, locks, and seeded randomization;
- `Validation`: safe size, pivot, proportion, enum, and color normalization;
- `Drawing`: the transparent pixel canvas, drawing context, composition order, and independent character-layer drawers;
- `Generation`: pixel-data generation and immutable result metadata;
- `Runtime`: native `Texture2D`/`Sprite` ownership, preview presentation, and application controller;
- `UI`: the zero-asset Play-mode control panel;
- `Export`: validated local PNG encoding and file naming.

`Assets/Scripts/GameManager.cs` is intentionally small. It connects the saved sample scene to the runtime modules so generation requires only pressing Play.

## Verification

Tests are split into EditMode and PlayMode suites under `Assets/Tests`.

- EditMode checks transparency, dimensions, bounds safety, deterministic output, variation, locking, validation, texture settings, resource replacement, and PNG export.
- PlayMode loads the saved sample scene and verifies that a visible generated sprite exists inside the camera without any user action.

The Unity editor must not already own the project when running tests from the command line:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.3.12f1\Editor\Unity.exe' `
  -batchmode -projectPath $PWD -runTests -testPlatform EditMode `
  -testResults .\TestResults-EditMode.xml
```

Change `EditMode` to `PlayMode` for the scene smoke test.
