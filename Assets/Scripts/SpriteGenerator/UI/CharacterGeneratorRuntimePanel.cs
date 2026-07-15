using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SpriteGenerator
{
    /// <summary>
    /// Asset-free runtime controls for the procedural character generator.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CharacterGeneratorRuntimePanel : MonoBehaviour
    {
        private const float PanelMargin = 10f;
        private const float PreferredPanelWidth = 390f;

        private CharacterGeneratorController controller;
        private Vector2 scrollPosition;

        private string widthText = string.Empty;
        private string heightText = string.Empty;
        private string pixelsPerUnitText = string.Empty;
        private string seedText = string.Empty;
        private bool numericInputsDirty;
        private bool numericInputsValid;
        private string numericValidationMessage = string.Empty;

        private string operationMessage = "Waiting for the character generator to initialize.";
        private bool operationFailed;
        private IReadOnlyList<string> generationWarnings = Array.Empty<string>();
        private string exportPath = string.Empty;

        private GUIStyle titleStyle;
        private GUIStyle sectionStyle;
        private GUIStyle wrappedLabelStyle;
        private GUIStyle statusStyle;
        private GUIStyle errorStyle;
        private GUIStyle warningStyle;
        private GUIStyle pathStyle;

        /// <summary>
        /// Connects this view to the runtime generator. Calling it again safely
        /// rebinds the panel and refreshes all editable numeric fields.
        /// </summary>
        public void Initialize(CharacterGeneratorController controller)
        {
            this.controller = controller;

            if (controller == null)
            {
                operationMessage = "Runtime controls could not initialize because the generator controller is missing.";
                operationFailed = true;
                generationWarnings = Array.Empty<string>();
                return;
            }

            SynchronizeNumericText();

            CharacterPixelData current = controller.CurrentPixelData;
            if (current != null)
            {
                operationMessage = $"Showing the generated {current.Width}x{current.Height} sprite (seed {current.Seed}).";
                operationFailed = false;
                generationWarnings = current.Warnings ?? Array.Empty<string>();
            }
            else
            {
                operationMessage = "Ready. Adjust the settings and select Generate.";
                operationFailed = false;
                generationWarnings = Array.Empty<string>();
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            float availableWidth = Mathf.Max(1f, Screen.width - (PanelMargin * 2f));
            float panelWidth = Mathf.Min(PreferredPanelWidth, availableWidth);
            float panelHeight = Mathf.Max(1f, Screen.height - (PanelMargin * 2f));
            var panelRect = new Rect(PanelMargin, PanelMargin, panelWidth, panelHeight);

            GUI.Box(panelRect, GUIContent.none);

            float innerMargin = Mathf.Min(10f, panelWidth * 0.04f);
            var contentRect = new Rect(
                panelRect.x + innerMargin,
                panelRect.y + innerMargin,
                Mathf.Max(1f, panelRect.width - (innerMargin * 2f)),
                Mathf.Max(1f, panelRect.height - (innerMargin * 2f)));

            GUILayout.BeginArea(contentRect);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

            GUILayout.Label("Runtime Character Sprite Generator", titleStyle);
            GUILayout.Label(
                $"Supported combinations: {CharacterRandomizer.SupportedCombinationCount:N0}",
                wrappedLabelStyle);
            GUILayout.Space(6f);

            if (controller == null)
            {
                GUILayout.Label(operationMessage, errorStyle);
                GUILayout.EndScrollView();
                GUILayout.EndArea();
                return;
            }

            DrawCanvasSettings();
            DrawProportionSettings();
            DrawDesignSettings();
            DrawActions();
            DrawFeedback();

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawCanvasSettings()
        {
            CharacterSpriteSettings settings = controller.Settings;

            GUILayout.Label("Canvas and Generation", sectionStyle);
            DrawNumericField(
                $"Width ({CharacterSpriteSettings.MinimumDimension}-{CharacterSpriteSettings.MaximumDimension})",
                ref widthText);
            DrawNumericField(
                $"Height ({CharacterSpriteSettings.MinimumDimension}-{CharacterSpriteSettings.MaximumDimension})",
                ref heightText);
            DrawNumericField("Pixels per unit (0.01-1024)", ref pixelsPerUnitText);
            DrawNumericField("Seed (whole number)", ref seedText);

            if (numericInputsDirty)
            {
                ValidateAndApplyNumericInputs();
            }

            settings.DrawOutline = GUILayout.Toggle(settings.DrawOutline, "Draw outline");
            settings.DrawGroundShadow = GUILayout.Toggle(settings.DrawGroundShadow, "Draw ground shadow");

            if (!numericInputsValid && !string.IsNullOrEmpty(numericValidationMessage))
            {
                GUILayout.Label(numericValidationMessage, errorStyle);
            }

            GUILayout.Space(6f);
        }

        private void DrawProportionSettings()
        {
            CharacterProportions proportions = controller.Settings.Proportions;

            GUILayout.Label("Character Proportions", sectionStyle);
            proportions.HeadSize = DrawSlider("Head size", proportions.HeadSize, 0.8f, 1.2f);
            proportions.BodyWidth = DrawSlider("Body width", proportions.BodyWidth, 0.75f, 1.25f);
            proportions.LegLength = DrawSlider("Leg length", proportions.LegLength, 0.8f, 1.2f);
            GUILayout.Space(6f);
        }

        private void DrawDesignSettings()
        {
            CharacterDesign design = controller.Settings.Design;
            CharacterLayerLocks locks = controller.Locks;

            GUILayout.Label("Design Layers", sectionStyle);

            bool bodyLocked = locks.Body;
            design.BodyBuild = DrawEnumSelector("Body build", design.BodyBuild, ref bodyLocked);
            locks.Body = bodyLocked;

            bool faceLocked = locks.Face;
            design.FaceStyle = DrawEnumSelector("Face style", design.FaceStyle, ref faceLocked);
            locks.Face = faceLocked;

            bool hairLocked = locks.Hair;
            design.HairStyle = DrawEnumSelector("Hair style", design.HairStyle, ref hairLocked);
            locks.Hair = hairLocked;

            bool outfitLocked = locks.Outfit;
            design.OutfitStyle = DrawEnumSelector("Outfit style", design.OutfitStyle, ref outfitLocked);
            locks.Outfit = outfitLocked;

            bool accessoryLocked = locks.Accessory;
            design.AccessoryStyle = DrawEnumSelector(
                "Accessory style",
                design.AccessoryStyle,
                ref accessoryLocked);
            locks.Accessory = accessoryLocked;

            PalettePreset previousPalette = design.PalettePreset;
            bool paletteLocked = locks.Palette;
            PalettePreset selectedPalette = DrawEnumSelector(
                "Palette preset",
                previousPalette,
                ref paletteLocked);
            locks.Palette = paletteLocked;

            if (selectedPalette != previousPalette)
            {
                CharacterPaletteLibrary.ApplyPreset(design, selectedPalette);
                operationMessage = $"Applied the {selectedPalette} palette. Select Generate to update the sprite.";
                operationFailed = false;
            }

            GUILayout.Space(8f);
        }

        private void DrawActions()
        {
            GUILayout.Label("Actions", sectionStyle);

            if (GUILayout.Button("Generate", GUILayout.MinHeight(30f)))
            {
                if (CanRunGenerationAction())
                {
                    TryGenerate();
                }
            }

            if (GUILayout.Button("Randomize Unlocked", GUILayout.MinHeight(30f)))
            {
                if (CanRunGenerationAction())
                {
                    TryRandomizeUnlocked();
                }
            }

            if (GUILayout.Button("Export PNG", GUILayout.MinHeight(30f)))
            {
                TryExport();
            }

            GUILayout.Space(8f);
        }

        private void DrawFeedback()
        {
            GUILayout.Label("Result", sectionStyle);

            if (!string.IsNullOrEmpty(operationMessage))
            {
                GUILayout.Label(operationMessage, operationFailed ? errorStyle : statusStyle);
            }

            if (generationWarnings != null && generationWarnings.Count > 0)
            {
                GUILayout.Label("Warnings", sectionStyle);
                for (int index = 0; index < generationWarnings.Count; index++)
                {
                    GUILayout.Label($"- {generationWarnings[index]}", warningStyle);
                }
            }

            if (!string.IsNullOrEmpty(exportPath))
            {
                GUILayout.Label("Last export (full path)", sectionStyle);
                GUILayout.TextArea(exportPath, pathStyle);
            }
        }

        private void DrawNumericField(string label, ref string text)
        {
            GUILayout.Label(label, wrappedLabelStyle);
            string editedText = GUILayout.TextField(text ?? string.Empty);
            if (!string.Equals(editedText, text, StringComparison.Ordinal))
            {
                text = editedText;
                numericInputsDirty = true;
            }
        }

        private static float DrawSlider(string label, float value, float minimum, float maximum)
        {
            GUILayout.Label($"{label}: {value:0.00}");
            return GUILayout.HorizontalSlider(value, minimum, maximum);
        }

        private static T DrawEnumSelector<T>(string label, T value, ref bool layerLocked)
            where T : struct, Enum
        {
            GUILayout.Label(label);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("<", GUILayout.Width(34f)))
            {
                value = CycleEnum(value, -1);
            }

            GUILayout.Label(value.ToString(), GUI.skin.box, GUILayout.ExpandWidth(true));

            if (GUILayout.Button(">", GUILayout.Width(34f)))
            {
                value = CycleEnum(value, 1);
            }

            layerLocked = GUILayout.Toggle(
                layerLocked,
                new GUIContent("Lock", $"Keep the {label.ToLowerInvariant()} unchanged when randomizing."),
                GUILayout.Width(58f));

            GUILayout.EndHorizontal();
            return value;
        }

        private static T CycleEnum<T>(T current, int direction)
            where T : struct, Enum
        {
            Array values = Enum.GetValues(typeof(T));
            int currentIndex = Array.IndexOf(values, current);
            if (currentIndex < 0)
            {
                currentIndex = 0;
            }

            int nextIndex = (currentIndex + direction) % values.Length;
            if (nextIndex < 0)
            {
                nextIndex += values.Length;
            }

            return (T)values.GetValue(nextIndex);
        }

        private bool CanRunGenerationAction()
        {
            if (numericInputsDirty)
            {
                ValidateAndApplyNumericInputs();
            }

            if (numericInputsValid)
            {
                return true;
            }

            operationMessage = $"Generation was not started. {numericValidationMessage}";
            operationFailed = true;
            generationWarnings = Array.Empty<string>();
            return false;
        }

        private void TryGenerate()
        {
            try
            {
                RecordGenerationResult(controller.Generate());
            }
            catch (Exception exception)
            {
                RecordUnexpectedFailure("Character generation", exception);
            }
        }

        private void TryRandomizeUnlocked()
        {
            try
            {
                GenerationOperationResult result = controller.RandomizeUnlocked();
                SynchronizeNumericText();
                RecordGenerationResult(result);
            }
            catch (Exception exception)
            {
                RecordUnexpectedFailure("Character randomization", exception);
            }
        }

        private void TryExport()
        {
            try
            {
                PngExportResult result = controller.Export();
                operationMessage = result.Message;
                operationFailed = !result.Succeeded;

                if (result.Succeeded)
                {
                    exportPath = result.Path;
                }
            }
            catch (Exception exception)
            {
                RecordUnexpectedFailure("PNG export", exception);
            }
        }

        private void RecordGenerationResult(GenerationOperationResult result)
        {
            operationMessage = result.Message;
            operationFailed = !result.Succeeded;
            generationWarnings = result.Warnings ?? Array.Empty<string>();
        }

        private void RecordUnexpectedFailure(string operation, Exception exception)
        {
            operationMessage = $"{operation} failed: {exception.Message}";
            operationFailed = true;
            generationWarnings = Array.Empty<string>();
        }

        private void SynchronizeNumericText()
        {
            CharacterSpriteSettings settings = controller.Settings;
            widthText = settings.Width.ToString(CultureInfo.InvariantCulture);
            heightText = settings.Height.ToString(CultureInfo.InvariantCulture);
            pixelsPerUnitText = settings.PixelsPerUnit.ToString("0.###", CultureInfo.InvariantCulture);
            seedText = settings.Seed.ToString(CultureInfo.InvariantCulture);
            numericInputsDirty = true;
            ValidateAndApplyNumericInputs();
        }

        private void ValidateAndApplyNumericInputs()
        {
            CharacterSpriteSettings settings = controller.Settings;
            var errors = new List<string>(4);

            if (TryParseDimension(widthText, out int width))
            {
                settings.Width = width;
            }
            else
            {
                errors.Add(
                    $"Width must be a whole number from {CharacterSpriteSettings.MinimumDimension} " +
                    $"to {CharacterSpriteSettings.MaximumDimension}.");
            }

            if (TryParseDimension(heightText, out int height))
            {
                settings.Height = height;
            }
            else
            {
                errors.Add(
                    $"Height must be a whole number from {CharacterSpriteSettings.MinimumDimension} " +
                    $"to {CharacterSpriteSettings.MaximumDimension}.");
            }

            if (TryParseFiniteFloat(pixelsPerUnitText, out float pixelsPerUnit) &&
                pixelsPerUnit >= 0.01f &&
                pixelsPerUnit <= 1024f)
            {
                settings.PixelsPerUnit = pixelsPerUnit;
            }
            else
            {
                errors.Add("Pixels per unit must be a number from 0.01 to 1024.");
            }

            if (TryParseInt(seedText, out int seed))
            {
                settings.Seed = seed;
            }
            else
            {
                errors.Add("Seed must be a whole number between -2147483648 and 2147483647.");
            }

            numericInputsValid = errors.Count == 0;
            numericValidationMessage = string.Join(" ", errors);
            numericInputsDirty = false;
        }

        private static bool TryParseDimension(string text, out int value)
        {
            return TryParseInt(text, out value) &&
                   value >= CharacterSpriteSettings.MinimumDimension &&
                   value <= CharacterSpriteSettings.MaximumDimension;
        }

        private static bool TryParseInt(string text, out int value)
        {
            return int.TryParse(
                text,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out value);
        }

        private static bool TryParseFiniteFloat(string text, out float value)
        {
            bool parsed = float.TryParse(
                text,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value);

            return parsed && !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                margin = new RectOffset(2, 2, 4, 8)
            };

            sectionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                margin = new RectOffset(0, 0, 7, 3)
            };

            wrappedLabelStyle = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true
            };

            statusStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                wordWrap = true,
                padding = new RectOffset(8, 8, 7, 7)
            };

            errorStyle = new GUIStyle(statusStyle);
            errorStyle.normal.textColor = new Color(1f, 0.47f, 0.42f);

            warningStyle = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true
            };
            warningStyle.normal.textColor = new Color(1f, 0.72f, 0.24f);

            pathStyle = new GUIStyle(GUI.skin.textArea)
            {
                wordWrap = true
            };
        }
    }
}
