using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SpriteAssetLibrary spriteAssetLibrary;
    [SerializeField] private Vector3 previewPosition = Vector3.zero;
    [SerializeField] private Vector3 previewScale = Vector3.one;
    [SerializeField] private Vector3 bodyOffset = new(0f, 0f, 0f);
    [SerializeField] private Vector3 bodyScale = new(1f, 1f, 1f);
    [SerializeField] private Vector3 pantsOffset = new(-0.06f, -0.8f, 0f);
    [SerializeField] private Vector3 pantsScale = new(0.51f, 0.51f, 1f);
    [SerializeField] private Vector3 clothingOffset = new(-0.02f, 0.3f, 0f);
    [SerializeField] private Vector3 clothingScale = new(0.6f, 0.6f, 1f);
    [SerializeField] private Vector3 hairOffset = new(0.03f, 2.01f, 0f);
    [SerializeField] private Vector3 hairScale = new(0.5f, 0.5f, 1f);
    [SerializeField] private Vector3 accessoryOffset = new(0f, 0f, 0f);
    [SerializeField] private Vector3 accessoryScale = new(0.5f, 0.5f, 1f);

    private readonly Dictionary<SpriteLayerCategory, SpriteRenderer> layerRenderers = new();
    private Transform previewRoot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Hello World!");

        RunGenerationDemo();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void RunGenerationDemo()
    {
        if (spriteAssetLibrary == null)
        {
            Debug.LogWarning("SpriteAssetLibrary is not assigned yet. Add the component to a GameObject and assign it in GameManager.");
            return;
        }

        foreach (var message in spriteAssetLibrary.ValidateLibrary())
        {
            Debug.LogWarning(message);
        }

        var engine = new CharacterGenerationEngine();
        var result = engine.Generate(CharacterGenerationRequest.CreateDefault(), spriteAssetLibrary);

        foreach (var message in result.Messages)
        {
            Debug.LogWarning(message);
        }

        if (!result.Succeeded || result.Config == null)
        {
            Debug.LogError("Character generation failed.");
            return;
        }

        RenderCharacter(result.Config);

        foreach (var selection in result.Config.Selections)
        {
            Debug.Log($"Selected {selection.Category}: {selection.Option.Id}");
        }
    }

    private void RenderCharacter(GeneratedCharacterConfig config)
    {
        EnsurePreviewRoot();

        foreach (SpriteLayerCategory category in System.Enum.GetValues(typeof(SpriteLayerCategory)))
        {
            var renderer = GetOrCreateLayerRenderer(category);

            if (config.TryGetSelection(category, out var selection))
            {
                renderer.sprite = selection.Option.Sprite;
                renderer.enabled = selection.Option.Sprite != null;
                renderer.transform.localPosition = GetLayerLocalPosition(selection);
                renderer.transform.localScale = GetLayerLocalScale(selection.Category);
                renderer.sortingOrder = GetSortingOrder(selection);
            }
            else
            {
                renderer.sprite = null;
                renderer.enabled = false;
                renderer.transform.localPosition = Vector3.zero;
                renderer.transform.localScale = Vector3.one;
            }
        }
    }

    private void EnsurePreviewRoot()
    {
        if (previewRoot != null)
        {
            previewRoot.position = previewPosition;
            previewRoot.localScale = previewScale;
            return;
        }

        var existingRoot = transform.Find("GeneratedCharacterPreview");
        if (existingRoot != null)
        {
            previewRoot = existingRoot;
            previewRoot.position = previewPosition;
            previewRoot.localScale = previewScale;
            return;
        }

        var previewObject = new GameObject("GeneratedCharacterPreview");
        previewRoot = previewObject.transform;
        previewRoot.SetParent(transform, false);
        previewRoot.position = previewPosition;
        previewRoot.localScale = previewScale;
    }

    private SpriteRenderer GetOrCreateLayerRenderer(SpriteLayerCategory category)
    {
        if (layerRenderers.TryGetValue(category, out var existingRenderer) && existingRenderer != null)
        {
            return existingRenderer;
        }

        EnsurePreviewRoot();

        var childName = $"{category}Layer";
        var child = previewRoot.Find(childName);

        if (child == null)
        {
            var layerObject = new GameObject(childName);
            child = layerObject.transform;
            child.SetParent(previewRoot, false);
            child.localPosition = Vector3.zero;
            child.localScale = Vector3.one;
        }

        var spriteRenderer = child.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = child.gameObject.AddComponent<SpriteRenderer>();
        }

        spriteRenderer.sortingOrder = GetSortingOrder(category);
        layerRenderers[category] = spriteRenderer;
        return spriteRenderer;
    }

    private int GetSortingOrder(SpriteLayerCategory category)
    {
        return category switch
        {
            SpriteLayerCategory.Body => 0,
            SpriteLayerCategory.Pants => 1,
            SpriteLayerCategory.Clothing => 2,
            SpriteLayerCategory.Hair => 3,
            SpriteLayerCategory.Accessory => 4,
            _ => 0
        };
    }

    private int GetSortingOrder(GeneratedCharacterConfig.LayerSelection selection)
    {
        if (selection.Option == null)
        {
            return GetSortingOrder(selection.Category);
        }

        return selection.Option.Id switch
        {
            "pants_shorts_brown" => 1,
            "pants_trousers_tan" => 1,
            "pants_traveler_green" => 1,
            "pants_fitted_dark" => 1,
            "accessory_bag" => 2,
            "accessory_cape" => 1,
            _ => GetSortingOrder(selection.Category)
        };
    }

    private Vector3 GetLayerLocalPosition(GeneratedCharacterConfig.LayerSelection selection)
    {
        if (selection.Option == null)
        {
            return Vector3.zero;
        }

        return GetDefaultLayerLocalPosition(selection.Category) + (selection.Option.Id switch
        {
            "pants_shorts_brown" => new Vector3(0f, 0f, 0f),
            "pants_trousers_tan" => new Vector3(0f, 0f, 0f),
            "pants_traveler_green" => new Vector3(0f, 0f, 0f),
            "pants_fitted_dark" => new Vector3(0f, 0f, 0f),

            "hair_short_brown" => new Vector3(0f, 0f, 0f),
            "hair_ponytail_auburn" => new Vector3(0.03f, 0f, 0f),
            "hair_messy_black" => new Vector3(0f, 0f, 0f),
            "hair_spiky_blonde" => new Vector3(0f, 0f, 0f),

            "accessory_glasses" => new Vector3(0f, 0.78f, 0f),
            "accessory_headband" => new Vector3(0f, 0.98f, 0f),
            "accessory_cape" => new Vector3(0f, 0f, 0f),
            "accessory_bag" => new Vector3(-0.34f, 0f, 0f),

            _ => Vector3.zero
        });
    }

    private Vector3 GetDefaultLayerLocalPosition(SpriteLayerCategory category)
    {
        return category switch
        {
            SpriteLayerCategory.Body => bodyOffset,
            SpriteLayerCategory.Pants => pantsOffset,
            SpriteLayerCategory.Hair => hairOffset,
            SpriteLayerCategory.Clothing => clothingOffset,
            SpriteLayerCategory.Accessory => accessoryOffset,
            _ => Vector3.zero
        };
    }

    private Vector3 GetLayerLocalScale(SpriteLayerCategory category)
    {
        return category switch
        {
            SpriteLayerCategory.Body => bodyScale,
            SpriteLayerCategory.Pants => pantsScale,
            SpriteLayerCategory.Clothing => clothingScale,
            SpriteLayerCategory.Hair => hairScale,
            SpriteLayerCategory.Accessory => accessoryScale,
            _ => Vector3.one
        };
    }
}
