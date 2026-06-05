using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public static class DesktopSurvivorsSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/DesktopSurvivorsPrototype.unity";
    private const string PrefabRoot = "Assets/Prefabs/DesktopSurvivors";
    private const string ProjectilePrefabPath = PrefabRoot + "/PistolBullet.prefab";
    private const string EnemyPrefabPath = PrefabRoot + "/SmallZombie.prefab";
    private const string BossPrefabPath = PrefabRoot + "/BruteZombie.prefab";
    private const string GemPrefabPath = PrefabRoot + "/SupplyGem.prefab";
    private const string HealPickupPrefabPath = PrefabRoot + "/HealPickup.prefab";
    private const string BombPickupPrefabPath = PrefabRoot + "/BombPickup.prefab";
    private const string ExperiencePickupPrefabPath = PrefabRoot + "/ExperiencePickup.prefab";
    private const string PostRoot = "Assets/Art/External/PostApocalypse";
    private const string GeneratedRoot = "Assets/Art/Generated/PostApocalypse";
    private const string TileRoot = "Assets/TilePalettes/PostApocalypse";
    private const string PalettePrefabPath = "Assets/TilePalettes/PostApocalypsePalette.prefab";
    private const string MapDecorPrefabRoot = "Assets/Prefabs/MapDecor";

    [MenuItem("Tools/Desktop Survivors/Build Prototype Scene")]
    public static void Build()
    {
        EnsureFolder("Assets/Scenes");
        EnsureFolder("Assets/Prefabs");
        EnsureFolder(PrefabRoot);
        EnsureFolder(MapDecorPrefabRoot);
        EnsureFolder(TileRoot);
        EnsureTag("Enemy");
        AssetDatabase.Refresh();
        PrepareSprites();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "WastelandSurvivorsPrototype";

        var gemPrefab = CreateGemPrefab();
        var projectilePrefab = CreateProjectilePrefab();
        var healPickupPrefab = CreateSupplyPickupPrefab(HealPickupPrefabPath, "HealPickup", CreateMedkitSpriteAsset(), SupplyPickupType.Heal, Color.white);
        var bombPickupPrefab = CreateSupplyPickupPrefab(BombPickupPrefabPath, "BombPickup", PostRoot + "/Objects/Barrel_rust_red_1.png", SupplyPickupType.Bomb, new Color(1f, 0.32f, 0.12f));
        var experiencePickupPrefab = CreateSupplyPickupPrefab(ExperiencePickupPrefabPath, "ExperiencePickup", PostRoot + "/Objects/Pickable/Bullet-box_1_Green.png", SupplyPickupType.Experience, Color.white);
        var enemyPrefab = CreateEnemyPrefab(gemPrefab);
        var bossPrefab = CreateBossPrefab(gemPrefab, healPickupPrefab, bombPickupPrefab, experiencePickupPrefab);
        CreatePostApocalypseTilePalette();
        CreateMapDecorPrefabs();

        ConfigureCamera();
        var infiniteMap = ConfigureFloor();
        var player = CreatePlayer(projectilePrefab);
        infiniteMap.player = player.transform;
        ConfigureDecorations(player.transform);
        CreateSpawner(enemyPrefab, player.transform);
        CreateBossSpawner(bossPrefab, player.transform);
        CreateSupplySpawner(healPickupPrefab, bombPickupPrefab, experiencePickupPrefab, player.transform);
        CreateGameManager(player);
        CreateCanvas();
        CreateEventSystem();

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Wasteland Survivors scene rebuilt: " + ScenePath);
    }

    private static void PrepareSprites()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { GeneratedRoot }))
            ConfigureTexture(AssetDatabase.GUIDToAssetPath(guid), 16);

        foreach (var path in GetUsedExternalSprites())
            ConfigureTexture(path, 16);
    }

    private static string[] GetUsedExternalSprites()
    {
        return new[]
        {
            PostRoot + "/Objects/Pickable/Ammo-crate_Blue.png",
            PostRoot + "/Objects/Pickable/Ammo-crate_Red.png",
            PostRoot + "/Objects/Pickable/Ammo-crate_Green.png",
            PostRoot + "/Objects/Pickable/Bandage.png",
            PostRoot + "/Objects/Pickable/Bullet-box_1_Green.png",
            PostRoot + "/Objects/Pickable/Bullet-box_1_Red.png",
            PostRoot + "/Objects/Pickable/Pistol.png",
            PostRoot + "/Objects/Pickable/Shotgun.png",
            PostRoot + "/Objects/Pickable/Gun.png",
            PostRoot + "/Objects/Barrel_rust_blue_1.png",
            PostRoot + "/Objects/Barrel_rust_blue_2.png",
            PostRoot + "/Objects/Barrel_rust_red_1.png",
            PostRoot + "/Character/Guns/Bullets/Pistol-bullet_Bullet.png",
            PostRoot + "/Character/Guns/Bullets/Gun-bullet_Whole.png",
            PostRoot + "/Character/Guns/Fire/Fire_Down-Sheet3.png",
            PostRoot + "/Objects/Vehicles/Overgrown/Car_1_Overgrown/Bleak-Yellow/Car_1_Overgrown_Bleak-Yellow_Red.png",
            PostRoot + "/Objects/Vehicles/Normal/Car_6_Scrap/Car_6_Red_Scrap.png",
            PostRoot + "/Objects/Vehicles/Normal/Car_8_Bus/Car_8_Red_Bus.png",
            PostRoot + "/Objects/Buildings/Door_3_Boarded-up_Beige.png",
            PostRoot + "/Objects/Buildings/Destroyed-wall_not-corner.png",
            PostRoot + "/Objects/Container/Container_3_Gray_Horizontal.png"
            ,PostRoot + "/Objects/Garbage-Bin_1.png"
            ,PostRoot + "/Objects/Garbage-Bin_2.png"
            ,PostRoot + "/Objects/Trash-bag_1.png"
            ,PostRoot + "/Objects/Trash-can_1.png"
            ,PostRoot + "/Objects/Stop-sign_Down_2_Overgrown_Bleak-Yellow.png"
            ,PostRoot + "/Objects/Street-Light_4_Side_Overgrown_Bleak-Yellow.png"
            ,PostRoot + "/Objects/Vending-machine_Red_Overgrown_Bleak-Yellow.png"
            ,PostRoot + "/Objects/Metal-Plates.png"
            ,PostRoot + "/Objects/Gray-brick_Debris.png"
            ,PostRoot + "/Objects/Traffic-cone.png"
        };
    }

    private static void ConfigureTexture(string path, int pixelsPerUnit)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            return;

        var changed = false;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            changed = true;
        }

        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            changed = true;
        }

        if (!Mathf.Approximately(importer.spritePixelsPerUnit, pixelsPerUnit))
        {
            importer.spritePixelsPerUnit = pixelsPerUnit;
            changed = true;
        }

        if (importer.filterMode != FilterMode.Point)
        {
            importer.filterMode = FilterMode.Point;
            changed = true;
        }

        if (importer.textureCompression != TextureImporterCompression.Uncompressed)
        {
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            changed = true;
        }

        if (changed)
            importer.SaveAndReimport();
    }

    private static ExperienceGem CreateGemPrefab()
    {
        DeleteAssetIfExists(GemPrefabPath);

        var go = new GameObject("SupplyGem");
        go.transform.localScale = Vector3.one * 0.75f;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(PostRoot + "/Objects/Pickable/Bullet-box_1_Green.png");
        sr.color = Color.white;
        sr.sortingOrder = 3;

        var collider = go.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.34f;

        go.AddComponent<ExperienceGem>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, GemPrefabPath).GetComponent<ExperienceGem>();
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static Sprite CreateRadiationShardSpriteAsset()
    {
        var texturePath = GeneratedRoot + "/radiation_xp_shard.png";
        if (!File.Exists(texturePath))
        {
            EnsureFolder(GeneratedRoot);
            var texture = new Texture2D(24, 24, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            for (var y = 0; y < 24; y++)
            {
                for (var x = 0; x < 24; x++)
                {
                    var color = new Color(0f, 0f, 0f, 0f);

                    var core = Mathf.Abs(x - 12) + Mathf.Abs(y - 12) <= 7;
                    var tallTop = x >= 9 && x <= 14 && y >= 4 && y <= 12 && Mathf.Abs(x - 12) <= y - 3;
                    var tallBottom = x >= 8 && x <= 15 && y >= 12 && y <= 20 && Mathf.Abs(x - 12) <= 21 - y;
                    var leftChip = x >= 5 && x <= 10 && y >= 11 && y <= 17 && x + y >= 18 && x + y <= 24;
                    var rightChip = x >= 14 && x <= 19 && y >= 8 && y <= 15 && x - y >= 3 && x - y <= 8;
                    var inside = core || tallTop || tallBottom || leftChip || rightChip;
                    var outline = inside && (x <= 5 || x >= 19 || y <= 4 || y >= 20
                        || Mathf.Abs(x - 12) + Mathf.Abs(y - 12) >= 7);
                    var shine = inside && ((x == 11 && y >= 7 && y <= 14) || (x == 9 && y >= 12 && y <= 15));

                    if (inside)
                        color = new Color(0.26f, 0.95f, 0.32f, 1f);
                    if (outline)
                        color = new Color(0.05f, 0.22f, 0.08f, 1f);
                    if (shine)
                        color = new Color(0.82f, 1f, 0.62f, 1f);

                    texture.SetPixel(x, y, color);
                }
            }

            File.WriteAllBytes(texturePath, texture.EncodeToPNG());
            AssetDatabase.ImportAsset(texturePath);
        }

        ConfigureTexture(texturePath, 16);
        return LoadSprite(texturePath);
    }

    private static Projectile CreateProjectilePrefab()
    {
        DeleteAssetIfExists(ProjectilePrefabPath);

        var go = new GameObject("PistolBullet");
        go.transform.localScale = new Vector3(1.35f, 0.82f, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(PostRoot + "/Character/Guns/Bullets/Pistol-bullet_Bullet.png");
        sr.color = new Color(1f, 0.88f, 0.44f);
        sr.sortingOrder = 7;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var collider = go.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.22f;

        var projectile = go.AddComponent<Projectile>();
        projectile.damage = 1;
        projectile.speed = 13.5f;
        projectile.lifetime = 1.05f;

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, ProjectilePrefabPath).GetComponent<Projectile>();
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static EnemyController CreateEnemyPrefab(ExperienceGem gemPrefab)
    {
        DeleteAssetIfExists(EnemyPrefabPath);

        var go = new GameObject("SmallZombie");
        go.tag = "Enemy";
        go.transform.localScale = Vector3.one * 1.35f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(GeneratedRoot + "/zombie_small_down_00.png");
        sr.sortingOrder = 5;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var collider = go.AddComponent<CircleCollider2D>();
        collider.radius = 0.36f;

        var health = go.AddComponent<Health>();
        health.maxHealth = 3;

        var flash = go.AddComponent<DamageFlash>();
        flash.flashColor = new Color(1f, 0.1f, 0.06f);

        var enemy = go.AddComponent<EnemyController>();
        enemy.moveSpeed = 2.1f;
        enemy.touchDamage = 1;
        enemy.attackCooldown = 0.55f;

        var animator = go.AddComponent<EnemyDirectionalAnimator>();
        animator.spriteDown = LoadGeneratedSprites("zombie_small_down", 6);
        animator.spriteLeft = LoadGeneratedSprites("zombie_small_left", 6);
        animator.spriteRight = LoadGeneratedSprites("zombie_small_right", 6);
        animator.spriteUp = LoadGeneratedSprites("zombie_small_up", 6);
        animator.frameTime = 0.11f;

        var dropper = go.AddComponent<ExperienceDropper>();
        dropper.gemPrefab = gemPrefab;

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, EnemyPrefabPath).GetComponent<EnemyController>();
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static EnemyController CreateBossPrefab(ExperienceGem gemPrefab, SupplyPickup healPickupPrefab, SupplyPickup bombPickupPrefab, SupplyPickup experiencePickupPrefab)
    {
        DeleteAssetIfExists(BossPrefabPath);

        var go = new GameObject("BruteZombie");
        go.tag = "Enemy";
        go.transform.localScale = Vector3.one * 2.15f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(GeneratedRoot + "/zombie_small_down_00.png");
        sr.color = new Color(1f, 0.78f, 0.78f, 1f);
        sr.sortingOrder = 6;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var collider = go.AddComponent<CircleCollider2D>();
        collider.radius = 0.42f;

        var health = go.AddComponent<Health>();
        health.maxHealth = 28;
        health.showDamageNumber = false;

        var flash = go.AddComponent<DamageFlash>();
        flash.flashColor = new Color(1f, 0.05f, 0.02f);
        flash.flashTime = 0.16f;

        var healthBar = go.AddComponent<WorldHealthBar>();
        healthBar.width = 1.35f;
        healthBar.height = 0.11f;
        healthBar.offset = new Vector3(0f, -0.58f, 0f);

        var enemy = go.AddComponent<EnemyController>();
        enemy.moveSpeed = 1.35f;
        enemy.touchDamage = 2;
        enemy.attackCooldown = 0.72f;
        enemy.charger = true;
        enemy.chargeDistance = 3.2f;
        enemy.chargeMultiplier = 1.6f;

        var animator = go.AddComponent<EnemyDirectionalAnimator>();
        animator.spriteDown = LoadGeneratedSprites("zombie_small_down", 6);
        animator.spriteLeft = LoadGeneratedSprites("zombie_small_left", 6);
        animator.spriteRight = LoadGeneratedSprites("zombie_small_right", 6);
        animator.spriteUp = LoadGeneratedSprites("zombie_small_up", 6);
        animator.frameTime = 0.12f;

        var dropper = go.AddComponent<ExperienceDropper>();
        dropper.gemPrefab = gemPrefab;

        var bossDropper = go.AddComponent<BossDropper>();
        bossDropper.healPrefab = healPickupPrefab;
        bossDropper.bombPrefab = bombPickupPrefab;
        bossDropper.experiencePrefab = experiencePickupPrefab;
        bossDropper.experienceDropCount = 3;

        go.AddComponent<BossEnemy>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, BossPrefabPath).GetComponent<EnemyController>();
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static SupplyPickup CreateSupplyPickupPrefab(string prefabPath, string objectName, string spritePath, SupplyPickupType type, Color color)
    {
        return CreateSupplyPickupPrefab(prefabPath, objectName, LoadSprite(spritePath), type, color);
    }

    private static SupplyPickup CreateSupplyPickupPrefab(string prefabPath, string objectName, Sprite sprite, SupplyPickupType type, Color color)
    {
        DeleteAssetIfExists(prefabPath);

        var go = new GameObject(objectName);
        go.transform.localScale = Vector3.one * 0.95f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = 4;

        var collider = go.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.42f;

        var pickup = go.AddComponent<SupplyPickup>();
        pickup.type = type;
        pickup.healAmount = 2;
        pickup.experienceAmount = 5;
        pickup.rotateSpeed = type == SupplyPickupType.Bomb ? 130f : 95f;
        pickup.bobHeight = 0.13f;

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath).GetComponent<SupplyPickup>();
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static Sprite CreateMedkitSpriteAsset()
    {
        var texturePath = GeneratedRoot + "/medkit_pickup.png";
        if (!File.Exists(texturePath))
        {
            EnsureFolder(GeneratedRoot);
            var texture = new Texture2D(24, 24, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            for (var y = 0; y < 24; y++)
            {
                for (var x = 0; x < 24; x++)
                {
                    var color = new Color(0f, 0f, 0f, 0f);
                    var inside = x >= 3 && x <= 20 && y >= 5 && y <= 19;
                    var border = inside && (x <= 4 || x >= 19 || y <= 6 || y >= 18);
                    var handle = x >= 8 && x <= 15 && y >= 2 && y <= 5;
                    var handleBorder = handle && (x == 8 || x == 15 || y == 2);
                    var cross = (x >= 10 && x <= 13 && y >= 8 && y <= 16) || (y >= 11 && y <= 14 && x >= 7 && x <= 16);

                    if (inside)
                        color = new Color(0.92f, 0.92f, 0.86f, 1f);
                    if (border || handleBorder)
                        color = new Color(0.18f, 0.2f, 0.18f, 1f);
                    if (handle && !handleBorder)
                        color = new Color(0.72f, 0.74f, 0.68f, 1f);
                    if (cross)
                        color = new Color(0.85f, 0.06f, 0.05f, 1f);

                    texture.SetPixel(x, y, color);
                }
            }

            File.WriteAllBytes(texturePath, texture.EncodeToPNG());
            AssetDatabase.ImportAsset(texturePath);
        }

        ConfigureTexture(texturePath, 16);
        return LoadSprite(texturePath);
    }

    private static void ConfigureCamera()
    {
        var go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        var camera = go.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 10.5f;
        camera.backgroundColor = new Color(0.045f, 0.05f, 0.045f);
        go.transform.position = new Vector3(0f, 0f, -10f);
        go.AddComponent<CameraShake>();
    }

    private static InfiniteWastelandMap ConfigureFloor()
    {
        var gridObject = new GameObject("Grid");
        gridObject.AddComponent<Grid>();

        var background = CreateTilemapObject(gridObject.transform, "Infinite Ruined City Ground", 0);
        var detail = CreateTilemapObject(gridObject.transform, "Cracks And Debris Detail", 1);

        var dirtA = CreateTile("waste_bg_00_00");
        var dirtB = CreateTile("waste_bg_01_00");
        var cracked = CreateTile("waste_bg_10_00");
        var roadA = CreateTile("waste_bg_00_07");
        var roadB = CreateTile("waste_bg_01_07");
        var roadC = CreateTile("waste_bg_02_07");

        var dirtTiles = new TileBase[] { dirtA, dirtB };
        var crackTiles = new TileBase[] { cracked };
        var roadTiles = new TileBase[] { roadA, roadB, roadC };

        for (var x = -60; x <= 60; x++)
        {
            for (var y = -46; y <= 46; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                var road = IsPreviewRoad(cell);
                var tileIndex = Mathf.Abs((x * 17 + y * 31) % 3);
                background.SetTile(cell, road ? roadTiles[tileIndex] : dirtTiles[Mathf.Abs((x + y) % dirtTiles.Length)]);

                if (!road && Mathf.Abs((x * 11 + y * 19) % 9) == 0)
                    detail.SetTile(cell, cracked);
            }
        }

        var map = gridObject.AddComponent<InfiniteWastelandMap>();
        map.groundTilemap = background;
        map.detailTilemap = detail;
        map.dirtTiles = dirtTiles;
        map.crackTiles = crackTiles;
        map.roadTiles = roadTiles;
        map.renderRadius = 54;
        map.updateStep = 8;
        map.roadSpacing = 30;
        map.roadHalfWidth = 3;
        return map;
    }

    private static Tilemap CreateTilemapObject(Transform parent, string name, int sortingOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        var tilemap = go.AddComponent<Tilemap>();
        var renderer = go.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = sortingOrder;
        return tilemap;
    }

    private static bool IsPreviewRoad(Vector3Int cell)
    {
        const int spacing = 30;
        const int halfWidth = 3;
        var x = PositiveMod(cell.x, spacing);
        var y = PositiveMod(cell.y, spacing);
        return x <= halfWidth || x >= spacing - halfWidth
            || y <= halfWidth || y >= spacing - halfWidth;
    }

    private static int PositiveMod(int value, int mod)
    {
        var result = value % mod;
        return result < 0 ? result + mod : result;
    }

    private static Tile CreateTile(string generatedName)
    {
        var tilePath = TileRoot + "/" + generatedName + ".asset";
        var existing = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
        if (existing != null)
            return existing;

        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = LoadSprite(GeneratedRoot + "/" + generatedName + ".png");
        tile.colliderType = Tile.ColliderType.None;
        AssetDatabase.CreateAsset(tile, tilePath);
        return tile;
    }

    private static void CreatePostApocalypseTilePalette()
    {
        DeleteAssetIfExists(PalettePrefabPath);

        var grid = new GameObject("PostApocalypsePalette");
        grid.AddComponent<Grid>();
        var tilemap = CreateTilemapObject(grid.transform, "Palette Tiles", 0);

        var tiles = new[]
        {
            CreateTile("waste_bg_00_00"),
            CreateTile("waste_bg_01_00"),
            CreateTile("waste_bg_10_00"),
            CreateTile("waste_bg_00_07"),
            CreateTile("waste_bg_01_07"),
            CreateTile("waste_bg_02_07"),
            CreateTile("building_gray_00_00"),
            CreateTile("building_gray_00_01"),
            CreateTile("building_gray_01_00"),
            CreateTile("building_gray_01_01")
        };

        for (var i = 0; i < tiles.Length; i++)
            tilemap.SetTile(new Vector3Int(i % 5, -(i / 5), 0), tiles[i]);

        PrefabUtility.SaveAsPrefabAsset(grid, PalettePrefabPath);
        Object.DestroyImmediate(grid);
    }

    private static void CreateMapDecorPrefabs()
    {
        CreateDecorationPrefab("Decor_GarbageBin", PostRoot + "/Objects/Garbage-Bin_1.png", Vector3.one);
        CreateDecorationPrefab("Decor_TrashBag", PostRoot + "/Objects/Trash-bag_1.png", Vector3.one);
        CreateDecorationPrefab("Decor_StopSign", PostRoot + "/Objects/Stop-sign_Down_2_Overgrown_Bleak-Yellow.png", Vector3.one);
        CreateDecorationPrefab("Decor_StreetLight", PostRoot + "/Objects/Street-Light_4_Side_Overgrown_Bleak-Yellow.png", Vector3.one);
        CreateDecorationPrefab("Decor_VendingMachine", PostRoot + "/Objects/Vending-machine_Red_Overgrown_Bleak-Yellow.png", Vector3.one);
        CreateDecorationPrefab("Decor_MetalPlates", PostRoot + "/Objects/Metal-Plates.png", Vector3.one);
        CreateDecorationPrefab("Decor_TrafficCone", PostRoot + "/Objects/Traffic-cone.png", Vector3.one);
        CreateDecorationPrefab("Decor_RustBarrel", PostRoot + "/Objects/Barrel_rust_red_1.png", Vector3.one);
    }

    private static void CreateDecorationPrefab(string name, string spritePath, Vector3 scale)
    {
        var prefabPath = MapDecorPrefabRoot + "/" + name + ".prefab";
        DeleteAssetIfExists(prefabPath);

        var go = new GameObject(name);
        go.transform.localScale = scale;
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = LoadSprite(spritePath);
        renderer.sortingOrder = 2;

        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);
    }

    private static void ConfigureDecorations(Transform player)
    {
        var root = new GameObject("Wasteland Decorations");
        CreateDecoration(root.transform, "Overgrown Car", PostRoot + "/Objects/Vehicles/Overgrown/Car_1_Overgrown/Bleak-Yellow/Car_1_Overgrown_Bleak-Yellow_Red.png", new Vector3(-20f, 14f, 0f), Vector3.one * 1.25f);
        CreateDecoration(root.transform, "Scrap Car", PostRoot + "/Objects/Vehicles/Normal/Car_6_Scrap/Car_6_Red_Scrap.png", new Vector3(24f, -12f, 0f), Vector3.one * 1.15f);
        CreateDecoration(root.transform, "Broken Bus", PostRoot + "/Objects/Vehicles/Normal/Car_8_Bus/Car_8_Red_Bus.png", new Vector3(42f, 18f, 0f), Vector3.one * 1.2f);
        CreateDecoration(root.transform, "Ammo Crate", PostRoot + "/Objects/Pickable/Ammo-crate_Red.png", new Vector3(-8f, -16f, 0f), Vector3.one * 1.1f);
        CreateDecoration(root.transform, "Rust Barrel A", PostRoot + "/Objects/Barrel_rust_red_1.png", new Vector3(13f, 10f, 0f), Vector3.one);
        CreateDecoration(root.transform, "Rust Barrel B", PostRoot + "/Objects/Barrel_rust_blue_2.png", new Vector3(-33f, -8f, 0f), Vector3.one);
        CreateDecoration(root.transform, "Boarded Door", PostRoot + "/Objects/Buildings/Door_3_Boarded-up_Beige.png", new Vector3(-48f, 26f, 0f), Vector3.one * 1.5f);
        CreateDecoration(root.transform, "Destroyed Wall", PostRoot + "/Objects/Buildings/Destroyed-wall_not-corner.png", new Vector3(55f, -28f, 0f), Vector3.one * 1.4f);
        CreateDecoration(root.transform, "Container", PostRoot + "/Objects/Container/Container_3_Gray_Horizontal.png", new Vector3(-58f, -34f, 0f), Vector3.one * 1.2f);
        CreateDecoration(root.transform, "Supply Pistol", PostRoot + "/Objects/Pickable/Pistol.png", new Vector3(6f, -7f, 0f), Vector3.one * 1.3f);

        var spawner = root.AddComponent<InfiniteDecorationSpawner>();
        spawner.player = player;
        spawner.chunkSize = 18;
        spawner.chunkRadius = 4;
        spawner.maxDecorationsPerChunk = 3;
        spawner.decorationSprites = new[]
        {
            LoadSprite(PostRoot + "/Objects/Garbage-Bin_1.png"),
            LoadSprite(PostRoot + "/Objects/Garbage-Bin_2.png"),
            LoadSprite(PostRoot + "/Objects/Trash-bag_1.png"),
            LoadSprite(PostRoot + "/Objects/Trash-can_1.png"),
            LoadSprite(PostRoot + "/Objects/Stop-sign_Down_2_Overgrown_Bleak-Yellow.png"),
            LoadSprite(PostRoot + "/Objects/Street-Light_4_Side_Overgrown_Bleak-Yellow.png"),
            LoadSprite(PostRoot + "/Objects/Vending-machine_Red_Overgrown_Bleak-Yellow.png"),
            LoadSprite(PostRoot + "/Objects/Metal-Plates.png"),
            LoadSprite(PostRoot + "/Objects/Gray-brick_Debris.png"),
            LoadSprite(PostRoot + "/Objects/Traffic-cone.png")
        };
    }

    private static void CreateDecoration(Transform parent, string name, string spritePath, Vector3 position, Vector3 scale)
    {
        var sprite = LoadSprite(spritePath);
        if (sprite == null)
            return;

        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = position;
        go.transform.localScale = scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;
    }

    private static GameObject CreatePlayer(Projectile projectilePrefab)
    {
        var go = new GameObject("Player");
        go.tag = "Player";
        go.transform.localScale = Vector3.one * 1.45f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(GeneratedRoot + "/survivor_down_00.png");
        sr.sortingOrder = 6;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var collider = go.AddComponent<CircleCollider2D>();
        collider.radius = 0.34f;

        var controller = go.AddComponent<PlayerController>();
        controller.moveSpeed = 4.8f;
        controller.frameTime = 0.1f;
        controller.cameraFollowSpeed = 12f;
        AssignPlayerSprites(controller);

        var health = go.AddComponent<Health>();
        health.maxHealth = 5;
        health.destroyOnDeath = false;
        health.showDamageNumber = false;

        var flash = go.AddComponent<DamageFlash>();
        flash.flashColor = new Color(1f, 0.1f, 0.06f);
        flash.flashTime = 0.12f;

        var healthBar = go.AddComponent<WorldHealthBar>();
        healthBar.width = 0.95f;
        healthBar.height = 0.1f;
        healthBar.offset = new Vector3(0f, -0.68f, 0f);

        var weapon = go.AddComponent<AutoAimWeapon>();
        weapon.projectilePrefab = projectilePrefab;
        weapon.fireCooldown = 0.42f;
        weapon.projectileSpeed = 13.5f;
        weapon.range = 10f;
        CreateHeldWeapon(go.transform, weapon);

        var orbitWeapon = go.AddComponent<BookOrbitWeapon>();
        orbitWeapon.bookSprite = LoadSprite(PostRoot + "/Objects/Barrel_rust_blue_1.png");
        orbitWeapon.radius = 1.35f;
        orbitWeapon.rotationSpeed = 210f;

        go.AddComponent<CoffeeSpillWeapon>();
        go.AddComponent<GrenadeWeapon>();

        var input = go.AddComponent<PlayerInput>();
        input.actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        input.defaultActionMap = "Player";
        input.notificationBehavior = PlayerNotifications.SendMessages;

        return go;
    }

    private static void CreateHeldWeapon(Transform player, AutoAimWeapon weapon)
    {
        var weaponObject = new GameObject("Held Weapon");
        weaponObject.transform.SetParent(player, false);
        weaponObject.transform.localScale = Vector3.one * 0.62f;

        var weaponRenderer = weaponObject.AddComponent<SpriteRenderer>();
        weaponRenderer.sprite = LoadSprite(PostRoot + "/Objects/Pickable/Pistol.png");
        weaponRenderer.sortingOrder = 9;

        var muzzleObject = new GameObject("Muzzle Point");
        muzzleObject.transform.SetParent(player, false);
        muzzleObject.transform.localPosition = Vector3.right * 0.85f;

        weapon.muzzlePoint = muzzleObject.transform;

        var visual = player.gameObject.AddComponent<HeldWeaponVisual>();
        visual.weapon = weapon;
        visual.weaponRenderer = weaponRenderer;
        visual.pistolSprite = LoadSprite(PostRoot + "/Objects/Pickable/Pistol.png");
        visual.shotgunSprite = LoadSprite(PostRoot + "/Objects/Pickable/Shotgun.png");
        visual.muzzlePoint = muzzleObject.transform;
        visual.holdDistance = 0.42f;
    }

    private static void CreateSpawner(EnemyController enemyPrefab, Transform player)
    {
        var go = new GameObject("Zombie Spawner");
        var spawner = go.AddComponent<SurvivorsEnemySpawner>();
        spawner.enemyPrefab = enemyPrefab;
        spawner.player = player;
        spawner.spawnInterval = 1.05f;
        spawner.minSpawnInterval = 0.28f;
        spawner.spawnDistance = 14f;
        spawner.maxEnemies = 75;
        spawner.maxEnemiesLimit = 160;
        spawner.difficultyStepTime = 30f;
    }

    private static void CreateBossSpawner(EnemyController bossPrefab, Transform player)
    {
        var go = new GameObject("Brute Zombie Spawner");
        var spawner = go.AddComponent<BossSpawner>();
        spawner.bossPrefab = bossPrefab;
        spawner.player = player;
        spawner.firstSpawnTime = 65f;
        spawner.spawnInterval = 95f;
        spawner.minSpawnInterval = 55f;
        spawner.spawnDistance = 16f;
    }

    private static void CreateSupplySpawner(SupplyPickup healPickupPrefab, SupplyPickup bombPickupPrefab, SupplyPickup experiencePickupPrefab, Transform player)
    {
        var go = new GameObject("Supply Spawner");
        var spawner = go.AddComponent<SupplySpawner>();
        spawner.healPrefab = healPickupPrefab;
        spawner.bombPrefab = bombPickupPrefab;
        spawner.experiencePrefab = experiencePickupPrefab;
        spawner.player = player;
        spawner.spawnInterval = 17f;
        spawner.spawnDistance = 8f;
        spawner.bombStartTime = 90f;
        spawner.bombMinEnemyCount = 18;
        spawner.maxPickups = 5;
        spawner.maxHealPickups = 2;
    }

    private static void CreateGameManager(GameObject player)
    {
        var go = new GameObject("Survivors Game Manager");
        var manager = go.AddComponent<SurvivorsGameManager>();
        manager.player = player.transform;
        manager.playerController = player.GetComponent<PlayerController>();
        manager.autoAimWeapon = player.GetComponent<AutoAimWeapon>();
        manager.bookOrbitWeapon = player.GetComponent<BookOrbitWeapon>();
        manager.coffeeSpillWeapon = player.GetComponent<CoffeeSpillWeapon>();
        manager.grenadeWeapon = player.GetComponent<GrenadeWeapon>();
        manager.playerHealth = player.GetComponent<Health>();
    }

    private static void CreateCanvas()
    {
        var canvasObject = new GameObject("Survivors Canvas");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        var manager = Object.FindFirstObjectByType<SurvivorsGameManager>();
        var hudBar = CreatePanel(canvasObject.transform, "HUD Bar", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(0f, 82f), new Color(0.035f, 0.04f, 0.035f, 0.9f));
        CreatePanel(hudBar.transform, "Rust Accent", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(94f, -8f), new Vector2(144f, 6f), new Color(0.75f, 0.18f, 0.08f, 1f));
        manager.statusText = CreateText(hudBar.transform, "Status", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -12f), new Vector2(920f, 30f), 20, TextAnchor.UpperLeft, "START 버튼을 눌러 폐허 도시로 진입");
        manager.levelText = CreateText(hudBar.transform, "Level", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -48f), new Vector2(480f, 26f), 20, TextAnchor.UpperLeft, "LV 1  XP 0 / 5");
        manager.timerText = CreateText(hudBar.transform, "Timer", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-90f, -18f), new Vector2(180f, 42f), 34, TextAnchor.UpperCenter, "00:00");

        manager.rewardPanel = CreateRewardPanel(canvasObject.transform);
        CreateTitlePanel(canvasObject.transform, manager);
        CreateGameOverPanel(canvasObject.transform, manager);
    }

    private static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.color = color;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return go;
    }

    private static SurvivorsRewardPanel CreateRewardPanel(Transform parent)
    {
        var root = new GameObject("Reward Panel");
        root.transform.SetParent(parent, false);
        var image = root.AddComponent<Image>();
        image.color = new Color(0.055f, 0.052f, 0.045f, 0.97f);
        var rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(980f, 390f);
        rect.anchoredPosition = Vector2.zero;
        CreatePanel(root.transform, "Top Rust Line", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -4f), new Vector2(0f, 8f), new Color(0.72f, 0.20f, 0.08f, 1f));

        var panel = parent.gameObject.AddComponent<SurvivorsRewardPanel>();
        panel.root = root;
        panel.buttons = new Button[3];
        panel.labels = new Text[3];
        panel.icons = new Image[3];
        panel.pencilIcon = LoadSprite(PostRoot + "/Objects/Pickable/Pistol.png");
        panel.bookIcon = LoadSprite(PostRoot + "/Objects/Barrel_rust_blue_1.png");
        panel.coffeeIcon = LoadSprite(PostRoot + "/Character/Guns/Fire/Fire_Down-Sheet3.png");
        panel.eraserIcon = LoadSprite(PostRoot + "/Objects/Pickable/Ammo-crate_Green.png");
        panel.rubberBandIcon = LoadSprite(PostRoot + "/Character/Guns/Bullets/Gun-bullet_Whole.png");
        panel.grenadeIcon = LoadSprite(PostRoot + "/Objects/Pickable/Ammo-crate_Red.png");
        panel.shotgunIcon = LoadSprite(PostRoot + "/Objects/Pickable/Shotgun.png");

        var title = CreateText(root.transform, "Reward Title", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(520f, 44f), 32, TextAnchor.UpperCenter, "보급품을 선택하세요");
        title.color = new Color(1f, 0.82f, 0.45f);
        var sub = CreateText(root.transform, "Reward Sub", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(620f, 24f), 17, TextAnchor.UpperCenter, "폐허 속에서 살아남기 위한 강화 하나를 고르세요");
        sub.color = new Color(0.76f, 0.72f, 0.62f);

        for (var i = 0; i < 3; i++)
        {
            var buttonObject = new GameObject("Reward Button " + (i + 1));
            buttonObject.transform.SetParent(root.transform, false);
            var buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.25f, 0.24f, 0.20f, 1f);
            var button = buttonObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = new Color(0.25f, 0.24f, 0.20f, 1f);
            colors.highlightedColor = new Color(0.47f, 0.35f, 0.22f, 1f);
            colors.pressedColor = new Color(0.68f, 0.27f, 0.12f, 1f);
            button.colors = colors;

            var buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0f, 0.5f);
            buttonRect.anchorMax = new Vector2(0f, 0.5f);
            buttonRect.sizeDelta = new Vector2(280f, 230f);
            buttonRect.anchoredPosition = new Vector2(180f + i * 310f, -42f);
            CreatePanel(buttonObject.transform, "Button Rust Line", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -3f), new Vector2(0f, 6f), new Color(0.75f, 0.2f, 0.08f, 1f));

            var iconObject = new GameObject("Icon");
            iconObject.transform.SetParent(buttonObject.transform, false);
            var icon = iconObject.AddComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            var iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 1f);
            iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.pivot = new Vector2(0.5f, 1f);
            iconRect.anchoredPosition = new Vector2(0f, -24f);
            iconRect.sizeDelta = new Vector2(92f, 92f);

            var label = CreateText(buttonObject.transform, "Label", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(16f, 18f), new Vector2(-32f, 100f), 20, TextAnchor.UpperCenter, "");
            label.color = new Color(0.95f, 0.88f, 0.68f);

            panel.buttons[i] = button;
            panel.labels[i] = label;
            panel.icons[i] = icon;
        }

        root.SetActive(false);
        return panel;
    }

    private static void CreateTitlePanel(Transform parent, SurvivorsGameManager manager)
    {
        var root = CreatePanel(parent, "Title Panel", new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, new Color(0.035f, 0.038f, 0.032f, 0.96f));
        manager.titlePanel = root;
        CreatePanel(root.transform, "Blood Line", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 126f), new Vector2(620f, 7f), new Color(0.62f, 0.12f, 0.07f, 1f));

        var title = CreateText(root.transform, "Game Title", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(860f, 72f), 54, TextAnchor.MiddleCenter, "WASTELAND SURVIVORS");
        title.color = new Color(1f, 0.88f, 0.62f);
        var sub = CreateText(root.transform, "Game Subtitle", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 82f), new Vector2(860f, 36f), 22, TextAnchor.MiddleCenter, "폐허 도시에서 자동 전투로 좀비 웨이브를 버텨라");
        sub.color = new Color(0.78f, 0.75f, 0.66f);
        var rule = CreateText(root.transform, "Game Rule", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -8f), new Vector2(720f, 84f), 20, TextAnchor.MiddleCenter, "WASD 이동\n가장 가까운 적을 자동 사격\n경험치를 모아 보급품 강화 선택");
        rule.color = new Color(0.9f, 0.86f, 0.72f);

        var buttonObject = CreateMenuButton(root.transform, "Start Button", "START", new Vector2(0f, -135f), new Vector2(300f, 70f));
        manager.startButton = buttonObject.GetComponent<Button>();
    }

    private static void CreateGameOverPanel(Transform parent, SurvivorsGameManager manager)
    {
        var root = CreatePanel(parent, "Game Over Panel", new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, new Color(0.06f, 0.02f, 0.018f, 0.94f));
        manager.gameOverPanel = root;
        manager.gameOverTitleText = CreateText(root.transform, "Game Over Title", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 95f), new Vector2(560f, 72f), 58, TextAnchor.MiddleCenter, "YOU DIED");
        manager.gameOverTitleText.color = new Color(1f, 0.18f, 0.12f);
        manager.gameOverInfoText = CreateText(root.transform, "Game Over Info", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 14f), new Vector2(720f, 70f), 24, TextAnchor.MiddleCenter, "");
        manager.gameOverInfoText.color = new Color(0.92f, 0.84f, 0.68f);

        var buttonObject = CreateMenuButton(root.transform, "Restart Button", "RESTART", new Vector2(0f, -110f), new Vector2(300f, 70f));
        manager.restartButton = buttonObject.GetComponent<Button>();
        root.SetActive(false);
    }

    private static GameObject CreateMenuButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.color = new Color(0.55f, 0.18f, 0.08f, 1f);
        var button = go.AddComponent<Button>();
        var colors = button.colors;
        colors.normalColor = new Color(0.55f, 0.18f, 0.08f, 1f);
        colors.highlightedColor = new Color(0.78f, 0.32f, 0.14f, 1f);
        colors.pressedColor = new Color(0.36f, 0.08f, 0.04f, 1f);
        button.colors = colors;

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        var text = CreateText(go.transform, "Label", new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, 28, TextAnchor.MiddleCenter, label);
        text.color = new Color(1f, 0.9f, 0.62f);
        return go;
    }

    private static Text CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment, string initialText)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        text.text = initialText;
        text.raycastTarget = false;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return text;
    }

    private static void CreateEventSystem()
    {
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<InputSystemUIInputModule>();
    }

    private static void AssignPlayerSprites(PlayerController controller)
    {
        controller.spriteDown = LoadGeneratedSprites("survivor_down", 6);
        controller.spriteLeft = LoadGeneratedSprites("survivor_left", 6);
        controller.spriteRight = LoadGeneratedSprites("survivor_right", 6);
        controller.spriteUp = LoadGeneratedSprites("survivor_up", 6);
    }

    private static Sprite[] LoadGeneratedSprites(string prefix, int count)
    {
        var sprites = new Sprite[count];
        for (var i = 0; i < count; i++)
            sprites[i] = LoadSprite(GeneratedRoot + "/" + prefix + "_" + i.ToString("00") + ".png");
        return sprites;
    }

    private static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void DeleteAssetIfExists(string path)
    {
        if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            AssetDatabase.DeleteAsset(path);
    }

    private static void EnsureTag(string tag)
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tags = tagManager.FindProperty("tags");
        for (var i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                return;
        }

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
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
