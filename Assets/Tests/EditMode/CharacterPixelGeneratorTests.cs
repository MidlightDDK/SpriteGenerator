using NUnit.Framework;
using UnityEngine;

namespace SpriteGenerator.Tests.EditMode
{
    public sealed class CharacterPixelGeneratorTests
    {
        private readonly CharacterPixelGenerator generator = new CharacterPixelGenerator();

        [Test]
        public void Generate_DefaultSettings_Creates32By32CharacterWithTransparentCorners()
        {
            CharacterPixelData result = generator.Generate(new CharacterSpriteSettings());
            Color32[] pixels = result.CopyPixels();

            Assert.That(result.Width, Is.EqualTo(32));
            Assert.That(result.Height, Is.EqualTo(32));
            Assert.That(pixels, Has.Length.EqualTo(32 * 32));
            Assert.That(result.VisiblePixelCount, Is.GreaterThan(0), "The character must contain visible pixels.");
            Assert.That(result.VisiblePixelCount, Is.LessThan(pixels.Length), "The runtime canvas must retain transparency.");

            Assert.That(result.GetPixel(0, 0).a, Is.Zero, "Bottom-left corner should remain transparent.");
            Assert.That(result.GetPixel(31, 0).a, Is.Zero, "Bottom-right corner should remain transparent.");
            Assert.That(result.GetPixel(0, 31).a, Is.Zero, "Top-left corner should remain transparent.");
            Assert.That(result.GetPixel(31, 31).a, Is.Zero, "Top-right corner should remain transparent.");
        }

        [Test]
        public void Generate_DefaultSettings_CompletesWithinTwoSecondAcceptanceBudget()
        {
            CharacterPixelData result = generator.Generate(new CharacterSpriteSettings());

            Assert.That(result.GenerationDuration, Is.LessThan(System.TimeSpan.FromSeconds(2)),
                $"Pixel generation took {result.GenerationDuration.TotalMilliseconds:0.###} ms.");
        }

        [Test]
        public void Generate_IdenticalSettings_IsPixelForPixelDeterministic()
        {
            var settings = new CharacterSpriteSettings();
            CharacterRandomizer.RandomizeUnlocked(settings, new CharacterLayerLocks(), 8675309);
            settings.Width = 31;
            settings.Height = 47;

            CharacterPixelData first = generator.Generate(settings);
            CharacterPixelData second = generator.Generate(settings.Clone());

            Assert.That(second.Width, Is.EqualTo(first.Width));
            Assert.That(second.Height, Is.EqualTo(first.Height));
            Assert.That(second.Seed, Is.EqualTo(first.Seed));
            Assert.That(second.PixelsPerUnit, Is.EqualTo(first.PixelsPerUnit));
            Assert.That(second.Pivot, Is.EqualTo(first.Pivot));
            CollectionAssert.AreEqual(first.CopyPixels(), second.CopyPixels());
        }

        [Test]
        public void Generate_ControlledRandomizationAcrossSeeds_ProducesVisualVariation()
        {
            Color32[] baseline = GenerateRandomized(1000).CopyPixels();
            bool foundDifferentPixels = false;

            for (int seed = 1001; seed <= 1032; seed++)
            {
                if (!PixelsEqual(baseline, GenerateRandomized(seed).CopyPixels()))
                {
                    foundDifferentPixels = true;
                    break;
                }
            }

            Assert.That(foundDifferentPixels, Is.True,
                "Controlled randomization across 32 distinct seeds did not change the generated character.");
        }

        [Test]
        public void Generate_Headband_DoesNotOverwriteEyePixels()
        {
            var settings = new CharacterSpriteSettings
            {
                OutlineThickness = 3f
            };
            settings.Design.HairStyle = HairStyle.None;
            settings.Design.FaceStyle = FaceStyle.Friendly;
            settings.Design.AccessoryStyle = AccessoryStyle.None;
            CharacterPixelData withoutHeadband = generator.Generate(settings);

            settings.Design.AccessoryStyle = AccessoryStyle.Headband;
            CharacterPixelData withHeadband = generator.Generate(settings);

            const int eyeY = 25;
            foreach (int eyeX in new[] { 13, 19 })
            {
                Assert.That(withHeadband.GetPixel(eyeX, eyeY),
                    Is.EqualTo(withoutHeadband.GetPixel(eyeX, eyeY)));
                Assert.That(withHeadband.GetPixel(eyeX, eyeY - 1),
                    Is.EqualTo(withoutHeadband.GetPixel(eyeX, eyeY - 1)));
            }
        }

        [TestCase(8, 8)]
        [TestCase(31, 47)]
        [TestCase(64, 48)]
        public void Generate_ValidVariableDimensions_UsesRequestedCanvasSize(int width, int height)
        {
            var settings = new CharacterSpriteSettings
            {
                Width = width,
                Height = height
            };

            CharacterPixelData result = generator.Generate(settings);

            Assert.That(result.Width, Is.EqualTo(width));
            Assert.That(result.Height, Is.EqualTo(height));
            Assert.That(result.CopyPixels(), Has.Length.EqualTo(width * height));
            Assert.That(result.VisiblePixelCount, Is.GreaterThan(0));
        }

        [TestCase(48, 32, 8, 0)]
        [TestCase(32, 48, 0, 8)]
        public void Generate_RectangularCanvas_PreservesCharacterAspectAndCentersIt(
            int width,
            int height,
            int offsetX,
            int offsetY)
        {
            CharacterPixelData square = generator.Generate(new CharacterSpriteSettings());
            CharacterPixelData rectangular = generator.Generate(new CharacterSpriteSettings
            {
                Width = width,
                Height = height
            });

            for (int y = 0; y < rectangular.Height; y++)
            {
                for (int x = 0; x < rectangular.Width; x++)
                {
                    bool insideCharacterArea = x >= offsetX && x < offsetX + square.Width &&
                                               y >= offsetY && y < offsetY + square.Height;
                    Color32 actual = rectangular.GetPixel(x, y);

                    if (insideCharacterArea)
                    {
                        Assert.That(actual, Is.EqualTo(square.GetPixel(x - offsetX, y - offsetY)));
                    }
                    else
                    {
                        Assert.That(actual.a, Is.Zero, $"Letterbox pixel ({x}, {y}) must remain transparent.");
                    }
                }
            }
        }

        [TestCase(0, -42, CharacterSpriteSettings.MinimumDimension, CharacterSpriteSettings.MinimumDimension)]
        [TestCase(-1, 0, CharacterSpriteSettings.MinimumDimension, CharacterSpriteSettings.MinimumDimension)]
        [TestCase(513, int.MaxValue, CharacterSpriteSettings.MaximumDimension, CharacterSpriteSettings.MaximumDimension)]
        public void Validate_InvalidDimensions_ClampsWithoutMutatingSource(
            int width,
            int height,
            int expectedWidth,
            int expectedHeight)
        {
            var source = new CharacterSpriteSettings
            {
                Width = width,
                Height = height
            };

            SpriteGenerationValidationResult validation = SpriteGenerationValidator.Validate(source);

            Assert.That(validation.Settings.Width, Is.EqualTo(expectedWidth));
            Assert.That(validation.Settings.Height, Is.EqualTo(expectedHeight));
            Assert.That(validation.Warnings, Is.Not.Empty);
            Assert.That(source.Width, Is.EqualTo(width), "Validation should not mutate serialized source settings.");
            Assert.That(source.Height, Is.EqualTo(height), "Validation should not mutate serialized source settings.");
        }

        [Test]
        public void CopyPixels_ReturnsDefensiveCopy()
        {
            CharacterPixelData result = generator.Generate(new CharacterSpriteSettings());
            Color32 expected = result.GetPixel(0, 0);
            Color32[] copy = result.CopyPixels();

            copy[0] = new Color32(1, 2, 3, 4);

            Assert.That(result.GetPixel(0, 0), Is.EqualTo(expected));
        }

        [Test]
        public void GetPixel_RejectsCoordinatesOutsideGeneratedCanvas()
        {
            CharacterPixelData result = generator.Generate(new CharacterSpriteSettings());

            Assert.Throws<System.ArgumentOutOfRangeException>(() => result.GetPixel(-1, 0));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => result.GetPixel(0, -1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => result.GetPixel(result.Width, 0));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => result.GetPixel(0, result.Height));
        }

        private CharacterPixelData GenerateRandomized(int seed)
        {
            var settings = new CharacterSpriteSettings();
            CharacterRandomizer.RandomizeUnlocked(settings, new CharacterLayerLocks(), seed);
            return generator.Generate(settings);
        }

        private static bool PixelsEqual(Color32[] first, Color32[] second)
        {
            if (first.Length != second.Length)
            {
                return false;
            }

            for (int index = 0; index < first.Length; index++)
            {
                if (!first[index].Equals(second[index]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
