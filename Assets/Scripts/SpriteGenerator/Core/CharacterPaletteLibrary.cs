using UnityEngine;

namespace SpriteGenerator
{
    public static class CharacterPaletteLibrary
    {
        private static readonly Color[] SkinTones =
        {
            Rgb(246, 214, 180),
            Rgb(224, 174, 126),
            Rgb(194, 125, 78),
            Rgb(146, 85, 54),
            Rgb(91, 55, 43),
            Rgb(61, 40, 36)
        };

        private static readonly Color[] EyeColors =
        {
            Rgb(38, 30, 28),
            Rgb(35, 73, 87),
            Rgb(49, 87, 55),
            Rgb(88, 57, 35)
        };

        public static int PresetCount => System.Enum.GetValues(typeof(PalettePreset)).Length;

        public static int SkinToneCount => SkinTones.Length;

        public static void ApplyPreset(CharacterDesign design, PalettePreset preset)
        {
            if (design == null)
            {
                return;
            }

            design.PalettePreset = preset;

            switch (preset)
            {
                case PalettePreset.Forest:
                    SetPalette(design, Rgb(45, 105, 62), Rgb(201, 157, 67), Rgb(102, 63, 37), Rgb(60, 35, 22));
                    break;
                case PalettePreset.Royal:
                    SetPalette(design, Rgb(50, 63, 155), Rgb(232, 190, 72), Rgb(114, 42, 131), Rgb(65, 37, 24));
                    break;
                case PalettePreset.Ember:
                    SetPalette(design, Rgb(167, 52, 43), Rgb(245, 147, 48), Rgb(91, 45, 32), Rgb(62, 28, 22));
                    break;
                case PalettePreset.Ocean:
                    SetPalette(design, Rgb(35, 116, 150), Rgb(80, 199, 188), Rgb(44, 76, 104), Rgb(30, 48, 63));
                    break;
                case PalettePreset.Violet:
                    SetPalette(design, Rgb(112, 62, 151), Rgb(205, 116, 184), Rgb(84, 58, 112), Rgb(49, 32, 58));
                    break;
                case PalettePreset.Monochrome:
                    SetPalette(design, Rgb(86, 91, 101), Rgb(180, 185, 193), Rgb(52, 55, 62), Rgb(35, 36, 41));
                    break;
                case PalettePreset.Desert:
                    SetPalette(design, Rgb(169, 111, 50), Rgb(224, 190, 111), Rgb(117, 75, 43), Rgb(84, 50, 29));
                    break;
                case PalettePreset.Rose:
                    SetPalette(design, Rgb(176, 67, 91), Rgb(239, 151, 151), Rgb(100, 56, 73), Rgb(73, 37, 48));
                    break;
                default:
                    SetPalette(design, Rgb(45, 105, 62), Rgb(201, 157, 67), Rgb(102, 63, 37), Rgb(60, 35, 22));
                    break;
            }
        }

        public static void ApplySkinTone(CharacterDesign design, int index)
        {
            if (design == null)
            {
                return;
            }

            design.SkinColor = SkinTones[WrapIndex(index, SkinTones.Length)];
        }

        public static void ApplyEyeColor(CharacterDesign design, int index)
        {
            if (design == null)
            {
                return;
            }

            design.EyeColor = EyeColors[WrapIndex(index, EyeColors.Length)];
        }

        private static int WrapIndex(int index, int length)
        {
            int remainder = index % length;
            return remainder < 0 ? remainder + length : remainder;
        }

        private static void SetPalette(CharacterDesign design, Color primary, Color secondary, Color accessory, Color hair)
        {
            design.OutfitPrimaryColor = primary;
            design.OutfitSecondaryColor = secondary;
            design.AccessoryColor = accessory;
            design.HairColor = hair;
        }

        private static Color Rgb(byte red, byte green, byte blue)
        {
            return new Color32(red, green, blue, byte.MaxValue);
        }
    }
}
