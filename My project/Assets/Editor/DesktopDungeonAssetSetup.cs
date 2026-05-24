using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class DesktopDungeonAssetSetup
{
    private const string CustomArtRoot = "Assets/Art/DesktopDungeon";
    private const string CustomTileRoot = CustomArtRoot + "/Tiles";
    private const string CustomSpriteRoot = CustomArtRoot + "/Sprites";
    private const string KenneyRoguelikeSheet = "Assets/Art/External/Kenney/RoguelikeRPG/Spritesheet/roguelikeSheet_transparent.png";
    private const string GeneratedTilesRoot = "Assets/TilePalettes/DesktopDungeon";
    private const string PalettePath = "Assets/TilePalettes/DesktopDungeonPalette.prefab";

    [InitializeOnLoadMethod]
    private static void AutoSetupWhenImported()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath) != null)
            return;

        EditorApplication.delayCall += Setup;
    }

    [MenuItem("Tools/Desktop Dungeon/Setup Art Assets")]
    public static void Setup()
    {
        EnsureFolder("Assets/Art");
        EnsureFolder(CustomArtRoot);
        EnsureFolder(GeneratedTilesRoot);

        ConfigureSingleSprites(CustomTileRoot, 64);
        ConfigureSingleSprites(CustomSpriteRoot, 64);
        SliceKenneyRoguelikeSheet();

        AssetDatabase.Refresh();
        CreateTilesAndPalette();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Desktop Dungeon art setup complete. Palette: " + PalettePath);
    }

    private static void ConfigureSingleSprites(string folder, float pixelsPerUnit)
    {
        if (!AssetDatabase.IsValidFolder(folder))
            return;

        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }

    private static void SliceKenneyRoguelikeSheet()
    {
        var importer = AssetImporter.GetAtPath(KenneyRoguelikeSheet) as TextureImporter;
        if (importer == null)
            return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 16;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(KenneyRoguelikeSheet);
        if (texture == null)
        {
            importer.SaveAndReimport();
            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(KenneyRoguelikeSheet);
        }

        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
        dataProvider.InitSpriteEditorDataProvider();

        var rects = new List<SpriteRect>();
        var nameIdPairs = new List<SpriteNameFileIdPair>();
        var index = 0;
        const int tileSize = 16;
        const int margin = 1;
        var step = tileSize + margin;

        for (var y = texture.height - tileSize; y >= 0; y -= step)
        {
            for (var x = 0; x + tileSize <= texture.width; x += step)
            {
                var spriteId = GUID.Generate();
                var name = "kenney_roguelike_" + index.ToString("0000");
                rects.Add(new SpriteRect
                {
                    name = name,
                    spriteID = spriteId,
                    rect = new Rect(x, y, tileSize, tileSize),
                    alignment = SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                });
                nameIdPairs.Add(new SpriteNameFileIdPair(name, spriteId));
                index++;
            }
        }

        dataProvider.SetSpriteRects(rects.ToArray());
        var nameFileIdProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        nameFileIdProvider.SetNameFileIdPairs(nameIdPairs);
        dataProvider.Apply();
        importer.SaveAndReimport();
    }

    private static void CreateTilesAndPalette()
    {
        if (AssetDatabase.IsValidFolder(GeneratedTilesRoot))
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Tile", new[] { GeneratedTilesRoot }))
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
        }

        var tiles = new List<TileBase>();
        var tileSpritePaths = new[]
        {
            CustomTileRoot + "/desk_floor.png",
            CustomTileRoot + "/notebook_tile.png",
            CustomTileRoot + "/keyboard_tile.png",
            CustomTileRoot + "/mousepad_tile.png",
            CustomTileRoot + "/coffee_cup_obstacle.png",
            CustomTileRoot + "/pencil_obstacle.png"
        };

        foreach (var path in tileSpritePaths)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
                continue;

            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.name = Path.GetFileNameWithoutExtension(path);
            var assetPath = GeneratedTilesRoot + "/" + tile.name + ".asset";
            AssetDatabase.CreateAsset(tile, assetPath);
            tiles.Add(tile);
        }

        if (AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath) != null)
            AssetDatabase.DeleteAsset(PalettePath);

        GridPaletteUtility.CreateNewPalette(
            "Assets/TilePalettes",
            "DesktopDungeonPalette",
            GridLayout.CellLayout.Rectangle,
            GridPalette.CellSizing.Manual,
            Vector3.one,
            GridLayout.CellSwizzle.XYZ
        );

        var paletteRoot = PrefabUtility.LoadPrefabContents(PalettePath);
        var tilemap = paletteRoot.GetComponentInChildren<Tilemap>();
        if (tilemap != null)
        {
            tilemap.ClearAllTiles();
            for (var i = 0; i < tiles.Count; i++)
            {
                var position = new Vector3Int(i % 6, -(i / 6), 0);
                tilemap.SetTile(position, tiles[i]);
            }
            tilemap.CompressBounds();
        }
        PrefabUtility.SaveAsPrefabAsset(paletteRoot, PalettePath);
        PrefabUtility.UnloadPrefabContents(paletteRoot);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        var parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        var name = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent))
            EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}
