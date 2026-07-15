using System;
using System.Diagnostics;

namespace SpriteGenerator
{
    public sealed class CharacterPixelGenerator
    {
        public CharacterPixelData Generate(CharacterSpriteSettings sourceSettings)
        {
            SpriteGenerationValidationResult validation = SpriteGenerationValidator.Validate(sourceSettings);
            CharacterSpriteSettings settings = validation.Settings;
            var stopwatch = Stopwatch.StartNew();

            var canvas = new PixelCanvas(settings.Width, settings.Height);
            CharacterLayerComposer.Draw(canvas, settings);

            stopwatch.Stop();
            return new CharacterPixelData(
                settings.Width,
                settings.Height,
                settings.PixelsPerUnit,
                settings.Pivot,
                settings.Seed,
                (UnityEngine.Color32[])canvas.Pixels.Clone(),
                validation.Warnings,
                stopwatch.Elapsed);
        }
    }
}
