using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpriteAssetLibrary : MonoBehaviour
{
    [SerializeField] private List<SpriteLayerOption> spriteOptions = new();
    [SerializeField] private List<SpriteLayerCategory> requiredCategories = new()
    {
        SpriteLayerCategory.Body,
        SpriteLayerCategory.Pants,
        SpriteLayerCategory.Hair,
        SpriteLayerCategory.Clothing
    };

    public IReadOnlyList<SpriteLayerOption> SpriteOptions => spriteOptions;
    public IReadOnlyList<SpriteLayerCategory> RequiredCategories => requiredCategories;

    public IReadOnlyList<SpriteLayerOption> GetOptionsFor(SpriteLayerCategory category)
    {
        return spriteOptions
            .Where(option => option != null && option.IsConfigured && option.Category == category)
            .ToList();
    }

    public bool TryGetOptionById(string optionId, out SpriteLayerOption option)
    {
        option = spriteOptions.FirstOrDefault(item =>
            item != null &&
            item.IsConfigured &&
            string.Equals(item.Id, optionId, StringComparison.OrdinalIgnoreCase));

        return option != null;
    }

    public List<string> ValidateLibrary()
    {
        var messages = new List<string>();

        foreach (var category in requiredCategories.Distinct())
        {
            if (GetOptionsFor(category).Count == 0)
            {
                messages.Add($"Missing configured sprite options for required category '{category}'.");
            }
        }

        var duplicateIds = spriteOptions
            .Where(option => option != null && !string.IsNullOrWhiteSpace(option.Id))
            .GroupBy(option => option.Id, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);

        foreach (var duplicateId in duplicateIds)
        {
            messages.Add($"Duplicate sprite option id detected: '{duplicateId}'.");
        }

        foreach (var option in spriteOptions.Where(option => option != null && !option.IsConfigured))
        {
            messages.Add("Found an incompletely configured sprite option. Each option needs an id and sprite reference.");
        }

        return messages;
    }
}
