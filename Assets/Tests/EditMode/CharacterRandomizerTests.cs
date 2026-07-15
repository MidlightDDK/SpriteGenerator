using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace SpriteGenerator.Tests.EditMode
{
    public sealed class CharacterRandomizerTests
    {
        [Test]
        public void SupportedCombinationCount_ExceedsAssignmentMinimum()
        {
            Assert.That(CharacterRandomizer.SupportedCombinationCount, Is.GreaterThanOrEqualTo(100));
        }

        [Test]
        public void RandomizeUnlocked_AllLayersLocked_PreservesEntireDesignAcrossSeedSweep()
        {
            var settings = new CharacterSpriteSettings();
            CharacterRandomizer.RandomizeUnlocked(settings, new CharacterLayerLocks(), 12345);
            CharacterDesign expected = settings.Design.Clone();
            var locks = new CharacterLayerLocks
            {
                Body = true,
                Face = true,
                Hair = true,
                Outfit = true,
                Accessory = true,
                Palette = true
            };

            for (int seed = -32; seed <= 32; seed++)
            {
                CharacterRandomizer.RandomizeUnlocked(settings, locks, seed);
                AssertDesignEquals(expected, settings.Design, $"seed {seed}");
            }
        }

        [Test]
        public void RandomizeUnlocked_AllLayersUnlocked_ChangesEveryLayerAcrossSeedSweep()
        {
            var settings = new CharacterSpriteSettings();
            var locks = new CharacterLayerLocks();
            var bodies = new HashSet<BodyBuild>();
            var faces = new HashSet<FaceStyle>();
            var hairStyles = new HashSet<HairStyle>();
            var outfits = new HashSet<OutfitStyle>();
            var accessories = new HashSet<AccessoryStyle>();
            var palettes = new HashSet<PalettePreset>();

            for (int seed = -32; seed <= 32; seed++)
            {
                CharacterRandomizer.RandomizeUnlocked(settings, locks, seed);
                bodies.Add(settings.Design.BodyBuild);
                faces.Add(settings.Design.FaceStyle);
                hairStyles.Add(settings.Design.HairStyle);
                outfits.Add(settings.Design.OutfitStyle);
                accessories.Add(settings.Design.AccessoryStyle);
                palettes.Add(settings.Design.PalettePreset);
            }

            Assert.That(bodies.Count, Is.GreaterThan(1), "Body layer did not vary.");
            Assert.That(faces.Count, Is.GreaterThan(1), "Face layer did not vary.");
            Assert.That(hairStyles.Count, Is.GreaterThan(1), "Hair layer did not vary.");
            Assert.That(outfits.Count, Is.GreaterThan(1), "Outfit layer did not vary.");
            Assert.That(accessories.Count, Is.GreaterThan(1), "Accessory layer did not vary.");
            Assert.That(palettes.Count, Is.GreaterThan(1), "Palette layer did not vary.");
        }

        [Test]
        public void RandomizeUnlocked_SameSeedAndStartingDesign_IsDeterministic()
        {
            var first = new CharacterSpriteSettings();
            var second = new CharacterSpriteSettings();

            CharacterRandomizer.RandomizeUnlocked(first, new CharacterLayerLocks(), -2147480000);
            CharacterRandomizer.RandomizeUnlocked(second, new CharacterLayerLocks(), -2147480000);

            Assert.That(second.Seed, Is.EqualTo(first.Seed));
            AssertDesignEquals(first.Design, second.Design, "matching seed");
        }

        [Test]
        public void PaletteIndexing_IntMinValue_WrapsWithoutOverflow()
        {
            var extreme = new CharacterDesign();
            var wrapped = new CharacterDesign();

            Assert.DoesNotThrow(() => CharacterPaletteLibrary.ApplySkinTone(extreme, int.MinValue));
            Assert.DoesNotThrow(() => CharacterPaletteLibrary.ApplyEyeColor(extreme, int.MinValue));
            CharacterPaletteLibrary.ApplySkinTone(wrapped, 4);
            CharacterPaletteLibrary.ApplyEyeColor(wrapped, 0);

            Assert.That(extreme.SkinColor, Is.EqualTo(wrapped.SkinColor));
            Assert.That(extreme.EyeColor, Is.EqualTo(wrapped.EyeColor));
        }

        private static void AssertDesignEquals(CharacterDesign expected, CharacterDesign actual, string context)
        {
            Assert.That(actual.BodyBuild, Is.EqualTo(expected.BodyBuild), $"Body changed for {context}.");
            Assert.That(actual.FaceStyle, Is.EqualTo(expected.FaceStyle), $"Face changed for {context}.");
            Assert.That(actual.HairStyle, Is.EqualTo(expected.HairStyle), $"Hair changed for {context}.");
            Assert.That(actual.OutfitStyle, Is.EqualTo(expected.OutfitStyle), $"Outfit changed for {context}.");
            Assert.That(actual.AccessoryStyle, Is.EqualTo(expected.AccessoryStyle), $"Accessory changed for {context}.");
            Assert.That(actual.PalettePreset, Is.EqualTo(expected.PalettePreset), $"Palette changed for {context}.");
            AssertColorEquals(expected.SkinColor, actual.SkinColor, "Skin", context);
            AssertColorEquals(expected.HairColor, actual.HairColor, "Hair", context);
            AssertColorEquals(expected.OutfitPrimaryColor, actual.OutfitPrimaryColor, "Primary outfit", context);
            AssertColorEquals(expected.OutfitSecondaryColor, actual.OutfitSecondaryColor, "Secondary outfit", context);
            AssertColorEquals(expected.AccessoryColor, actual.AccessoryColor, "Accessory", context);
            AssertColorEquals(expected.EyeColor, actual.EyeColor, "Eye", context);
            AssertColorEquals(expected.OutlineColor, actual.OutlineColor, "Outline", context);
        }

        private static void AssertColorEquals(Color expected, Color actual, string label, string context)
        {
            Assert.That(actual, Is.EqualTo(expected), $"{label} color changed for {context}.");
        }
    }
}
