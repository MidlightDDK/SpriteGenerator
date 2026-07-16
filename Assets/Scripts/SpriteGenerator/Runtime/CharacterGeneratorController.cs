using System;
using System.Collections.Generic;

namespace SpriteGenerator
{
    public readonly struct GenerationOperationResult
    {
        private GenerationOperationResult(
            bool succeeded,
            string message,
            CharacterPixelData pixelData,
            IReadOnlyList<string> warnings)
        {
            Succeeded = succeeded;
            Message = message;
            PixelData = pixelData;
            Warnings = warnings ?? Array.Empty<string>();
        }

        public bool Succeeded { get; }

        public string Message { get; }

        public CharacterPixelData PixelData { get; }

        public IReadOnlyList<string> Warnings { get; }

        internal static GenerationOperationResult Success(CharacterPixelData pixelData)
        {
            string message = $"Generated {pixelData.Width}x{pixelData.Height} sprite from seed {pixelData.Seed} " +
                             $"in {pixelData.GenerationDuration.TotalMilliseconds:0.###} ms.";
            return new GenerationOperationResult(true, message, pixelData, pixelData.Warnings);
        }

        internal static GenerationOperationResult Failure(string message)
        {
            return new GenerationOperationResult(false, message, null, Array.Empty<string>());
        }
    }

    public sealed class CharacterGeneratorController : IDisposable
    {
        private readonly CharacterPixelGenerator generator = new();
        private readonly SpritePreviewPresenter presenter;
        private readonly RuntimePresentationSettings presentationSettings;

        public CharacterGeneratorController(
            CharacterSpriteSettings settings,
            CharacterLayerLocks locks,
            RuntimePresentationSettings presentationSettings,
            UnityEngine.Transform owner)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Locks = locks ?? throw new ArgumentNullException(nameof(locks));
            this.presentationSettings = presentationSettings ?? throw new ArgumentNullException(nameof(presentationSettings));
            presenter = new SpritePreviewPresenter(
                owner,
                presentationSettings.PreviewLocalPosition,
                presentationSettings.SortingOrder,
                presentationSettings.PreviewMaximumWorldSize);
        }

        public CharacterSpriteSettings Settings { get; }

        public CharacterLayerLocks Locks { get; }

        public CharacterPixelData CurrentPixelData { get; private set; }

        public SpritePreviewPresenter Presenter => presenter;

        public string ExportDirectory
        {
            get => presentationSettings.ExportDirectory;
            set => presentationSettings.ExportDirectory = value;
        }

        public string ResolvedExportDirectory => presentationSettings.ResolveExportDirectory();

        public GenerationOperationResult Generate()
        {
            try
            {
                CharacterPixelData pixelData = generator.Generate(Settings);
                presenter.SetLocalPosition(presentationSettings.PreviewLocalPosition);
                presenter.Present(pixelData);
                CurrentPixelData = pixelData;
                return GenerationOperationResult.Success(pixelData);
            }
            catch (Exception exception)
            {
                return GenerationOperationResult.Failure($"Character generation failed: {exception.Message}");
            }
        }

        public GenerationOperationResult RandomizeUnlocked()
        {
            int nextSeed = CharacterRandomizer.GetNextSeed(Settings.Seed);
            CharacterRandomizer.RandomizeUnlocked(Settings, Locks, nextSeed);
            return Generate();
        }

        public PngExportResult Export(string requestedFileName = null)
        {
            int generatedSeed = CurrentPixelData?.Seed ?? Settings.Seed;
            string fileName = string.IsNullOrWhiteSpace(requestedFileName)
                ? $"{presentationSettings.ExportFilePrefix}_{generatedSeed}"
                : requestedFileName;

            return SpritePngExporter.Export(
                presenter.CurrentTexture,
                presentationSettings.ResolveExportDirectory(),
                fileName);
        }

        public void UseDefaultExportDirectory()
        {
            presentationSettings.ExportDirectory = string.Empty;
        }

        public void Dispose()
        {
            presenter.Dispose();
            CurrentPixelData = null;
        }
    }
}
