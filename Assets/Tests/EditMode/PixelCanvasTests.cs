using NUnit.Framework;
using UnityEngine;

namespace SpriteGenerator.Tests.EditMode
{
    public sealed class PixelCanvasTests
    {
        [Test]
        public void Constructor_CreatesExplicitlyTransparentCanvas()
        {
            var canvas = new PixelCanvas(5, 3);

            Assert.That(canvas.Width, Is.EqualTo(5));
            Assert.That(canvas.Height, Is.EqualTo(3));
            Assert.That(canvas.Pixels, Has.Length.EqualTo(15));

            var transparent = new Color32(0, 0, 0, 0);
            for (int index = 0; index < canvas.Pixels.Length; index++)
            {
                Assert.That(canvas.Pixels[index], Is.EqualTo(transparent), $"Pixel {index} was not transparent.");
            }
        }

        [Test]
        public void SetPixel_IgnoresEveryOutOfBoundsEdgeWithoutChangingCanvas()
        {
            var canvas = new PixelCanvas(4, 3);
            Color32[] original = (Color32[])canvas.Pixels.Clone();
            var visible = new Color32(255, 80, 20, 255);

            Assert.DoesNotThrow(() => canvas.SetPixel(-1, 0, visible));
            Assert.DoesNotThrow(() => canvas.SetPixel(0, -1, visible));
            Assert.DoesNotThrow(() => canvas.SetPixel(canvas.Width, 0, visible));
            Assert.DoesNotThrow(() => canvas.SetPixel(0, canvas.Height, visible));
            Assert.DoesNotThrow(() => canvas.SetPixel(int.MinValue, int.MaxValue, visible));

            Assert.That(canvas.GetPixel(-1, 0), Is.EqualTo(PixelCanvas.Transparent));
            Assert.That(canvas.GetPixel(0, -1), Is.EqualTo(PixelCanvas.Transparent));
            Assert.That(canvas.GetPixel(canvas.Width, 0), Is.EqualTo(PixelCanvas.Transparent));
            Assert.That(canvas.GetPixel(0, canvas.Height), Is.EqualTo(PixelCanvas.Transparent));

            CollectionAssert.AreEqual(original, canvas.Pixels);

            canvas.SetPixel(3, 2, visible);
            Assert.That(canvas.Pixels[2 * canvas.Width + 3], Is.EqualTo(visible));
        }
    }
}
