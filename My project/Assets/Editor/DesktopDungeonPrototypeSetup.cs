using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public static class DesktopDungeonPrototypeSetup
{
    private const string PrefabRoot = "Assets/Prefabs/DesktopDungeon";
    private const string DustPrefabPath = PrefabRoot + "/Enemy_Dust.prefab";

    [MenuItem("Tools/Desktop Dungeon/Setup Prototype Scene")]
    public static void SetupScene()
    {
        const string scenePath = "Assets/Scenes/SampleScene.unity";
        if (File.Exists(scenePath))
            EditorSceneManager.OpenScene(scenePath);

        EnsureFolder("Assets/Prefabs");
        EnsureFolder(PrefabRoot);

        EnsureTag("Enemy");
        CleanupOldPrototypeObjects();

        var enemyPrefab = CreateEnemyPrefab();

        ConfigureCamera();
        ConfigureFlatFloor();
        ConfigureArenaBounds();
        var player = ConfigurePlayer();
        PlaceTestEnemy(enemyPrefab);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Desktop Dungeon flat touch test setup complete.");
    }

    private static void CleanupOldPrototypeObjects()
    {
        var namesToRemove = new[]
        {
            "GameManager",
            "PrototypeCanvas",
            "EnemySpawnPoints",
            "ArenaBounds",
            "Enemy Dust",
            "Enemy Clip",
            "Enemy USB"
        };

        var objectsToRemove = new List<GameObject>();
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go == null)
                continue;

            if (!go.scene.IsValid())
                continue;

            foreach (var name in namesToRemove)
            {
                if (go.name == name)
                {
                    objectsToRemove.Add(go);
                    break;
                }
            }
        }

        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go == null)
                continue;

            if (!go.scene.IsValid() || !go.CompareTag("Enemy"))
                continue;

            objectsToRemove.Add(go);
        }

        foreach (var go in objectsToRemove)
        {
            if (go != null)
                Object.DestroyImmediate(go);
        }
    }

    private static EnemyController CreateEnemyPrefab()
    {
        if (AssetDatabase.LoadAssetAtPath<EnemyController>(DustPrefabPath) != null)
            AssetDatabase.DeleteAsset(DustPrefabPath);

        var go = new GameObject("Enemy Dust");
        go.tag = "Enemy";
        go.transform.localScale = Vector3.one * 1.15f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("Assets/Art/DesktopDungeon/Sprites/enemy_dust.png");
        sr.sortingOrder = 4;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var collider = go.AddComponent<CircleCollider2D>();
        collider.radius = 0.32f;

        var health = go.AddComponent<Health>();
        health.maxHealth = 1;

        var enemy = go.AddComponent<EnemyController>();
        enemy.moveSpeed = 2.2f;
        enemy.touchDamage = 999;
        enemy.attackCooldown = 0.05f;

        go.AddComponent<TouchKillPlayer>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, DustPrefabPath).GetComponent<EnemyController>();
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static void ConfigureCamera()
    {
        var camera = Camera.main;
        if (camera == null)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
        }

        camera.orthographic = true;
        camera.orthographicSize = 9.5f;
        camera.transform.position = new Vector3(0f, 0f, -10f);
        camera.backgroundColor = new Color(0.12f, 0.13f, 0.15f);
    }

    private static void ConfigureFlatFloor()
    {
        var grid = Object.FindFirstObjectByType<Grid>();
        if (grid == null)
        {
            var gridObject = new GameObject("Grid");
            grid = gridObject.AddComponent<Grid>();
        }

        var background = FindOrCreateTilemap(grid.transform, "Background", 0);
        var objects = FindOrCreateTilemap(grid.transform, "Object", 2);
        background.ClearAllTiles();
        objects.ClearAllTiles();

        var objectCollider = objects.GetComponent<TilemapCollider2D>();
        if (objectCollider != null)
            Object.DestroyImmediate(objectCollider);

        var floor = LoadTile("Assets/TilePalettes/DesktopDungeon/desk_floor.asset");
        for (var x = -30; x <= 30; x++)
        {
            for (var y = -18; y <= 18; y++)
                background.SetTile(new Vector3Int(x, y, 0), floor);
        }
    }

    private static Tilemap FindOrCreateTilemap(Transform parent, string name, int sortingOrder)
    {
        var existing = parent.Find(name);
        var go = existing != null ? existing.gameObject : new GameObject(name);
        go.transform.SetParent(parent);

        var tilemap = go.GetComponent<Tilemap>();
        if (tilemap == null)
            tilemap = go.AddComponent<Tilemap>();

        var renderer = go.GetComponent<TilemapRenderer>();
        if (renderer == null)
            renderer = go.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = sortingOrder;

        return tilemap;
    }

    private static void ConfigureArenaBounds()
    {
        var root = new GameObject("ArenaBounds");
        CreateWall(root.transform, "Top", new Vector2(0f, 18.75f), new Vector2(62f, 0.5f));
        CreateWall(root.transform, "Bottom", new Vector2(0f, -18.75f), new Vector2(62f, 0.5f));
        CreateWall(root.transform, "Left", new Vector2(-30.75f, 0f), new Vector2(0.5f, 38f));
        CreateWall(root.transform, "Right", new Vector2(30.75f, 0f), new Vector2(0.5f, 38f));
    }

    private static void CreateWall(Transform parent, string name, Vector2 position, Vector2 size)
    {
        var wall = new GameObject(name);
        wall.transform.SetParent(parent);
        wall.transform.position = position;
        var collider = wall.AddComponent<BoxCollider2D>();
        collider.size = size;
    }

    private static GameObject ConfigurePlayer()
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            player = GameObject.Find("Player");
            if (player == null)
                player = new GameObject("Player");
            player.tag = "Player";
        }

        player.name = "Player";
        player.transform.position = Vector3.zero;
        player.transform.localScale = Vector3.one * 2.2f;

        var sr = player.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("Assets/Images/Characters/tile_0024.png");
        sr.sortingOrder = 6;

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var collider = player.GetComponent<CircleCollider2D>();
        if (collider == null)
            collider = player.AddComponent<CircleCollider2D>();
        collider.radius = 0.28f;

        var controller = player.GetComponent<PlayerController>();
        if (controller == null)
            controller = player.AddComponent<PlayerController>();
        controller.moveSpeed = 5.0f;
        controller.frameTime = 0.14f;
        controller.cameraFollowSpeed = 12f;
        AssignPlayerAnimationSprites(controller);

        var health = player.GetComponent<Health>();
        if (health == null)
            health = player.AddComponent<Health>();
        health.maxHealth = 1;
        health.destroyOnDeath = true;

        var shooter = player.GetComponent<PlayerShooter>();
        if (shooter != null)
            Object.DestroyImmediate(shooter);

        var input = player.GetComponent<PlayerInput>();
        if (input != null)
            input.notificationBehavior = PlayerNotifications.SendMessages;

        return player;
    }

    private static void PlaceTestEnemy(EnemyController enemyPrefab)
    {
        var enemyObject = PrefabUtility.InstantiatePrefab(enemyPrefab.gameObject) as GameObject;
        enemyObject.transform.position = new Vector3(6f, 0f, 0f);
        enemyObject.name = "Enemy Dust";
    }

    private static void AssignPlayerAnimationSprites(PlayerController controller)
    {
        controller.spriteDown = LoadSprites(
            "Assets/Images/Characters/tile_0024.png",
            "Assets/Images/Characters/tile_0023.png",
            "Assets/Images/Characters/tile_0024.png",
            "Assets/Images/Characters/tile_0025.png"
        );
        controller.spriteLeft = LoadSprites(
            "Assets/Images/Characters/tile_0026.png",
            "Assets/Images/Characters/tile_0053.png",
            "Assets/Images/Characters/tile_0026.png",
            "Assets/Images/Characters/tile_0080.png"
        );
        controller.spriteRight = LoadSprites(
            "Assets/Images/Characters/tile_0050.png",
            "Assets/Images/Characters/tile_0077.png",
            "Assets/Images/Characters/tile_0050.png",
            "Assets/Images/Characters/tile_0104.png"
        );
        controller.spriteUp = LoadSprites(
            "Assets/Images/Characters/tile_0052.png",
            "Assets/Images/Characters/tile_0051.png",
            "Assets/Images/Characters/tile_0052.png",
            "Assets/Images/Characters/tile_0079.png"
        );
    }

    private static Sprite[] LoadSprites(params string[] paths)
    {
        var sprites = new Sprite[paths.Length];
        for (var i = 0; i < paths.Length; i++)
            sprites[i] = LoadSprite(paths[i]);
        return sprites;
    }

    private static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static TileBase LoadTile(string path)
    {
        return AssetDatabase.LoadAssetAtPath<TileBase>(path);
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
