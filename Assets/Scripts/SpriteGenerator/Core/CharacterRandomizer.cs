using System;

namespace SpriteGenerator
{
    public static class CharacterRandomizer
    {
        public static long SupportedCombinationCount =>
            Enum.GetValues(typeof(BodyBuild)).Length *
            Enum.GetValues(typeof(FaceStyle)).Length *
            Enum.GetValues(typeof(HairStyle)).Length *
            Enum.GetValues(typeof(OutfitStyle)).Length *
            Enum.GetValues(typeof(AccessoryStyle)).Length *
            CharacterPaletteLibrary.PresetCount *
            CharacterPaletteLibrary.SkinToneCount;

        public static void RandomizeUnlocked(CharacterSpriteSettings settings, CharacterLayerLocks locks, int seed)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            locks ??= new CharacterLayerLocks();
            settings.Seed = seed;
            CharacterDesign design = settings.Design;

            if (!locks.Body)
            {
                design.BodyBuild = Pick<BodyBuild>(seed, 101);
            }

            if (!locks.Face)
            {
                design.FaceStyle = Pick<FaceStyle>(seed, 211);
            }

            if (!locks.Hair)
            {
                design.HairStyle = Pick<HairStyle>(seed, 307);
            }

            if (!locks.Outfit)
            {
                design.OutfitStyle = Pick<OutfitStyle>(seed, 401);
            }

            if (!locks.Accessory)
            {
                design.AccessoryStyle = Pick<AccessoryStyle>(seed, 503);
            }

            if (!locks.Palette)
            {
                var paletteRandom = CreateRandom(seed, 601);
                var preset = (PalettePreset)paletteRandom.Next(0, CharacterPaletteLibrary.PresetCount);
                CharacterPaletteLibrary.ApplyPreset(design, preset);
                CharacterPaletteLibrary.ApplySkinTone(design, paletteRandom.Next(0, CharacterPaletteLibrary.SkinToneCount));
                CharacterPaletteLibrary.ApplyEyeColor(design, paletteRandom.Next(0, 4));
            }
        }

        public static int GetNextSeed(int currentSeed)
        {
            return unchecked(currentSeed * 1664525 + 1013904223);
        }

        private static T Pick<T>(int seed, int salt) where T : struct, Enum
        {
            Array values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(CreateRandom(seed, salt).Next(0, values.Length));
        }

        private static Random CreateRandom(int seed, int salt)
        {
            int mixed = unchecked(seed ^ (salt * -1640531527));
            mixed = unchecked((mixed ^ (mixed >> 16)) * -2048144789);
            mixed = unchecked((mixed ^ (mixed >> 13)) * -1028477387);
            return new Random(mixed ^ (mixed >> 16));
        }
    }
}
