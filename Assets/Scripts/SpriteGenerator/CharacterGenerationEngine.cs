using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterGenerationEngine
{
    private readonly System.Random random;

    public CharacterGenerationEngine(int? seed = null)
    {
        random = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
    }

    public GenerationResult Generate(CharacterGenerationRequest request, SpriteAssetLibrary library)
    {
        var messages = new List<string>();

        if (library == null)
        {
            messages.Add("Sprite asset library reference is missing.");
            return GenerationResult.Failure(messages);
        }

        request ??= CharacterGenerationRequest.CreateDefault();

        var config = new GeneratedCharacterConfig();

        foreach (SpriteLayerCategory category in Enum.GetValues(typeof(SpriteLayerCategory)))
        {
            var options = library.GetOptionsFor(category);

            if (options.Count == 0)
            {
                if (library.RequiredCategories.Contains(category))
                {
                    messages.Add($"No valid sprite options found for required category '{category}'.");
                }

                continue;
            }

            var selected = ResolveSelectionForCategory(category, request, library, options, messages);
            if (selected != null)
            {
                config.SetSelection(category, selected);
            }
        }

        return messages.Count > 0
            ? GenerationResult.Partial(config, messages)
            : GenerationResult.Success(config);
    }

    private SpriteLayerOption ResolveSelectionForCategory(
        SpriteLayerCategory category,
        CharacterGenerationRequest request,
        SpriteAssetLibrary library,
        IReadOnlyList<SpriteLayerOption> options,
        List<string> messages)
    {
        if (request.TryGetLockState(category, out var lockState) && lockState.Locked)
        {
            if (library.TryGetOptionById(lockState.LockedOptionId, out var lockedOption) && lockedOption.Category == category)
            {
                return lockedOption;
            }

            messages.Add($"Locked option '{lockState.LockedOptionId}' for category '{category}' was not found. Falling back to random selection.");
        }

        var randomizedOptions = new List<SpriteLayerOption>();
        foreach (var option in options)
        {
            if (option.EnabledForRandomization)
            {
                randomizedOptions.Add(option);
            }
        }

        if (randomizedOptions.Count == 0)
        {
            messages.Add($"No randomizable sprite options are enabled for category '{category}'.");
            return null;
        }

        return randomizedOptions[random.Next(0, randomizedOptions.Count)];
    }

    public readonly struct GenerationResult
    {
        private GenerationResult(GeneratedCharacterConfig config, IReadOnlyList<string> messages, bool succeeded)
        {
            Config = config;
            Messages = messages;
            Succeeded = succeeded;
        }

        public GeneratedCharacterConfig Config { get; }
        public IReadOnlyList<string> Messages { get; }
        public bool Succeeded { get; }

        public static GenerationResult Success(GeneratedCharacterConfig config)
        {
            return new GenerationResult(config, Array.Empty<string>(), true);
        }

        public static GenerationResult Partial(GeneratedCharacterConfig config, IReadOnlyList<string> messages)
        {
            return new GenerationResult(config, messages, true);
        }

        public static GenerationResult Failure(IReadOnlyList<string> messages)
        {
            return new GenerationResult(null, messages, false);
        }
    }
}
