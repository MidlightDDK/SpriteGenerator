using System;
using UnityEngine;

namespace SpriteGenerator
{
    [Serializable]
    public sealed class CharacterSpriteSettings
    {
        public const int DefaultDimension = 32;
        public const int MinimumDimension = 8;
        public const int MaximumDimension = 512;

        [SerializeField, Range(MinimumDimension, MaximumDimension)]
        [Tooltip("Width of the runtime-created texture in pixels.")]
        private int width = DefaultDimension;

        [SerializeField, Range(MinimumDimension, MaximumDimension)]
        [Tooltip("Height of the runtime-created texture in pixels.")]
        private int height = DefaultDimension;

        [SerializeField, Min(0.01f)]
        [Tooltip("How many texture pixels occupy one Unity world unit.")]
        private float pixelsPerUnit = 8f;

        [SerializeField]
        [Tooltip("Normalized pivot used when the runtime Sprite is created.")]
        private Vector2 pivot = new(0.5f, 0.5f);

        [SerializeField]
        [Tooltip("A reproducible seed used by controlled randomization.")]
        private int seed = 314159;

        [SerializeField]
        private bool drawOutline = true;

        [SerializeField]
        private bool drawGroundShadow = true;

        [SerializeField, Range(0.5f, 3f)]
        [Tooltip("Outline width measured on the reference 32-pixel canvas.")]
        private float outlineThickness = 1f;

        [SerializeField]
        private CharacterDesign design = new();

        [SerializeField]
        private CharacterProportions proportions = new();

        public int Width
        {
            get => width;
            set => width = value;
        }

        public int Height
        {
            get => height;
            set => height = value;
        }

        public float PixelsPerUnit
        {
            get => pixelsPerUnit;
            set => pixelsPerUnit = value;
        }

        public Vector2 Pivot
        {
            get => pivot;
            set => pivot = value;
        }

        public int Seed
        {
            get => seed;
            set => seed = value;
        }

        public bool DrawOutline
        {
            get => drawOutline;
            set => drawOutline = value;
        }

        public bool DrawGroundShadow
        {
            get => drawGroundShadow;
            set => drawGroundShadow = value;
        }

        public float OutlineThickness
        {
            get => outlineThickness;
            set => outlineThickness = value;
        }

        public CharacterDesign Design => design ??= new CharacterDesign();

        public CharacterProportions Proportions => proportions ??= new CharacterProportions();

        public CharacterSpriteSettings Clone()
        {
            return new CharacterSpriteSettings
            {
                width = width,
                height = height,
                pixelsPerUnit = pixelsPerUnit,
                pivot = pivot,
                seed = seed,
                drawOutline = drawOutline,
                drawGroundShadow = drawGroundShadow,
                outlineThickness = outlineThickness,
                design = Design.Clone(),
                proportions = Proportions.Clone()
            };
        }
    }

    [Serializable]
    public sealed class CharacterDesign
    {
        [SerializeField] private BodyBuild bodyBuild = BodyBuild.Average;
        [SerializeField] private FaceStyle faceStyle = FaceStyle.Friendly;
        [SerializeField] private HairStyle hairStyle = HairStyle.Short;
        [SerializeField] private OutfitStyle outfitStyle = OutfitStyle.Tunic;
        [SerializeField] private AccessoryStyle accessoryStyle = AccessoryStyle.None;
        [SerializeField] private PalettePreset palettePreset = PalettePreset.Forest;

        [SerializeField] private Color skinColor = new(0.76f, 0.49f, 0.30f, 1f);
        [SerializeField] private Color hairColor = new(0.18f, 0.10f, 0.06f, 1f);
        [SerializeField] private Color outfitPrimaryColor = new(0.18f, 0.42f, 0.25f, 1f);
        [SerializeField] private Color outfitSecondaryColor = new(0.72f, 0.56f, 0.25f, 1f);
        [SerializeField] private Color accessoryColor = new(0.42f, 0.24f, 0.12f, 1f);
        [SerializeField] private Color eyeColor = new(0.08f, 0.07f, 0.06f, 1f);
        [SerializeField] private Color outlineColor = new(0.07f, 0.055f, 0.07f, 1f);

        public BodyBuild BodyBuild
        {
            get => bodyBuild;
            set => bodyBuild = value;
        }

        public FaceStyle FaceStyle
        {
            get => faceStyle;
            set => faceStyle = value;
        }

        public HairStyle HairStyle
        {
            get => hairStyle;
            set => hairStyle = value;
        }

        public OutfitStyle OutfitStyle
        {
            get => outfitStyle;
            set => outfitStyle = value;
        }

        public AccessoryStyle AccessoryStyle
        {
            get => accessoryStyle;
            set => accessoryStyle = value;
        }

        public PalettePreset PalettePreset
        {
            get => palettePreset;
            set => palettePreset = value;
        }

        public Color SkinColor
        {
            get => skinColor;
            set => skinColor = value;
        }

        public Color HairColor
        {
            get => hairColor;
            set => hairColor = value;
        }

        public Color OutfitPrimaryColor
        {
            get => outfitPrimaryColor;
            set => outfitPrimaryColor = value;
        }

        public Color OutfitSecondaryColor
        {
            get => outfitSecondaryColor;
            set => outfitSecondaryColor = value;
        }

        public Color AccessoryColor
        {
            get => accessoryColor;
            set => accessoryColor = value;
        }

        public Color EyeColor
        {
            get => eyeColor;
            set => eyeColor = value;
        }

        public Color OutlineColor
        {
            get => outlineColor;
            set => outlineColor = value;
        }

        public CharacterDesign Clone()
        {
            return new CharacterDesign
            {
                bodyBuild = bodyBuild,
                faceStyle = faceStyle,
                hairStyle = hairStyle,
                outfitStyle = outfitStyle,
                accessoryStyle = accessoryStyle,
                palettePreset = palettePreset,
                skinColor = skinColor,
                hairColor = hairColor,
                outfitPrimaryColor = outfitPrimaryColor,
                outfitSecondaryColor = outfitSecondaryColor,
                accessoryColor = accessoryColor,
                eyeColor = eyeColor,
                outlineColor = outlineColor
            };
        }
    }

    [Serializable]
    public sealed class CharacterProportions
    {
        [SerializeField, Range(0.8f, 1.2f)] private float headSize = 1f;
        [SerializeField, Range(0.75f, 1.25f)] private float bodyWidth = 1f;
        [SerializeField, Range(0.8f, 1.2f)] private float legLength = 1f;

        public float HeadSize
        {
            get => headSize;
            set => headSize = value;
        }

        public float BodyWidth
        {
            get => bodyWidth;
            set => bodyWidth = value;
        }

        public float LegLength
        {
            get => legLength;
            set => legLength = value;
        }

        public CharacterProportions Clone()
        {
            return new CharacterProportions
            {
                headSize = headSize,
                bodyWidth = bodyWidth,
                legLength = legLength
            };
        }
    }

    [Serializable]
    public sealed class CharacterLayerLocks
    {
        [SerializeField] private bool body;
        [SerializeField] private bool face;
        [SerializeField] private bool hair;
        [SerializeField] private bool outfit;
        [SerializeField] private bool accessory;
        [SerializeField] private bool palette;

        public bool Body
        {
            get => body;
            set => body = value;
        }

        public bool Face
        {
            get => face;
            set => face = value;
        }

        public bool Hair
        {
            get => hair;
            set => hair = value;
        }

        public bool Outfit
        {
            get => outfit;
            set => outfit = value;
        }

        public bool Accessory
        {
            get => accessory;
            set => accessory = value;
        }

        public bool Palette
        {
            get => palette;
            set => palette = value;
        }
    }
}
