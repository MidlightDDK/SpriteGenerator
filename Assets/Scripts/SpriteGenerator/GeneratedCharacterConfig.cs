using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class GeneratedCharacterConfig
{
    [Serializable]
    public class LayerSelection
    {
        public LayerSelection(SpriteLayerCategory category, SpriteLayerOption option)
        {
            Category = category;
            Option = option;
        }

        public SpriteLayerCategory Category { get; }
        public SpriteLayerOption Option { get; }
    }

    private readonly List<LayerSelection> selections = new();

    public IReadOnlyList<LayerSelection> Selections => selections;

    public void SetSelection(SpriteLayerCategory category, SpriteLayerOption option)
    {
        var existing = selections.FirstOrDefault(selection => selection.Category == category);
        if (existing != null)
        {
            selections.Remove(existing);
        }

        selections.Add(new LayerSelection(category, option));
    }

    public bool TryGetSelection(SpriteLayerCategory category, out LayerSelection selection)
    {
        selection = selections.FirstOrDefault(item => item.Category == category);
        return selection != null;
    }
}
