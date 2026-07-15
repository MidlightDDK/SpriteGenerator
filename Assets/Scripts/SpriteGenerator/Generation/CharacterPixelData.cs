using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteGenerator
{
    public sealed class CharacterPixelData
    {
        private readonly Color32[] pixels;

        internal CharacterPixelData(
            int width,
            int height,
            float pixelsPerUnit,
            Vector2 pivot,
            int seed,
            Color32[] pixels,
            IReadOnlyList<string> warnings,
            TimeSpan generationDuration)
        {
            Width = width;
            Height = height;
            PixelsPerUnit = pixelsPerUnit;
            Pivot = pivot;
            Seed = seed;
            this.pixels = pixels ?? throw new ArgumentNullException(nameof(pixels));
            Warnings = warnings ?? Array.Empty<string>();
            GenerationDuration = generationDuration;

            int visible = 0;
            for (int index = 0; index < pixels.Length; index++)
            {
                if (pixels[index].a > 0)
                {
                    visible++;
                }
            }

            VisiblePixelCount = visible;
        }

        public int Width { get; }

        public int Height { get; }

        public float PixelsPerUnit { get; }

        public Vector2 Pivot { get; }

        public int Seed { get; }

        public int VisiblePixelCount { get; }

        public TimeSpan GenerationDuration { get; }

        public IReadOnlyList<string> Warnings { get; }

        public Color32 GetPixel(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException($"Pixel ({x}, {y}) is outside the {Width}x{Height} canvas.");
            }

            return pixels[y * Width + x];
        }

        public Color32[] CopyPixels()
        {
            return (Color32[])pixels.Clone();
        }
    }
}
