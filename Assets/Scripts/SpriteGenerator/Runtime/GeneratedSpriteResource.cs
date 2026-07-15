using System;
using UnityEngine;

namespace SpriteGenerator
{
    public sealed class GeneratedSpriteResource : IDisposable
    {
        private bool disposed;

        private GeneratedSpriteResource(Texture2D texture, Sprite sprite)
        {
            Texture = texture;
            Sprite = sprite;
        }

        public Texture2D Texture { get; private set; }

        public Sprite Sprite { get; private set; }

        public static GeneratedSpriteResource Create(CharacterPixelData pixelData)
        {
            if (pixelData == null)
            {
                throw new ArgumentNullException(nameof(pixelData));
            }

            var texture = new Texture2D(
                pixelData.Width,
                pixelData.Height,
                TextureFormat.RGBA32,
                false,
                false)
            {
                name = $"GeneratedCharacter_{pixelData.Seed}_{pixelData.Width}x{pixelData.Height}",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                anisoLevel = 0,
                hideFlags = HideFlags.DontSave
            };

            texture.SetPixels32(pixelData.CopyPixels());
            texture.Apply(false, false);

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, pixelData.Width, pixelData.Height),
                pixelData.Pivot,
                pixelData.PixelsPerUnit,
                0,
                SpriteMeshType.FullRect);

            sprite.name = texture.name;
            sprite.hideFlags = HideFlags.DontSave;
            return new GeneratedSpriteResource(texture, sprite);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            DestroyRuntimeObject(Sprite);
            DestroyRuntimeObject(Texture);
            Sprite = null;
            Texture = null;
        }

        private static void DestroyRuntimeObject(UnityEngine.Object value)
        {
            if (value == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(value);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(value);
            }
        }
    }
}
