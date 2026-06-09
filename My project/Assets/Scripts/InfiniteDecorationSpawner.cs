using System.Collections.Generic;
using UnityEngine;

public class InfiniteDecorationSpawner : MonoBehaviour
{
    public Transform player;
    public Sprite[] decorationSprites;
    public Sprite[] largeDecorationSprites;
    public Sprite shadowSprite;
    public int chunkSize = 18;
    public int chunkRadius = 3;
    public int maxDecorationsPerChunk = 3;
    public float largeDecorationChance = 0.25f;

    private readonly Dictionary<Vector2Int, GameObject> activeChunks = new();
    private Vector2Int lastPlayerChunk = new(99999, 99999);

    private void Start()
    {
        Refresh(true);
    }

    private void Update()
    {
        Refresh(false);
    }

    private void Refresh(bool force)
    {
        if (player == null || IsEmpty(decorationSprites) && IsEmpty(largeDecorationSprites))
            return;

        var currentChunk = WorldToChunk(player.position);
        if (!force && currentChunk == lastPlayerChunk)
            return;

        lastPlayerChunk = currentChunk;

        var needed = new HashSet<Vector2Int>();
        for (var x = currentChunk.x - chunkRadius; x <= currentChunk.x + chunkRadius; x++)
        {
            for (var y = currentChunk.y - chunkRadius; y <= currentChunk.y + chunkRadius; y++)
            {
                var chunk = new Vector2Int(x, y);
                needed.Add(chunk);
                if (!activeChunks.ContainsKey(chunk))
                    activeChunks.Add(chunk, CreateChunk(chunk));
            }
        }

        var removeList = new List<Vector2Int>();
        foreach (var pair in activeChunks)
        {
            if (!needed.Contains(pair.Key))
                removeList.Add(pair.Key);
        }

        foreach (var chunk in removeList)
        {
            Destroy(activeChunks[chunk]);
            activeChunks.Remove(chunk);
        }
    }

    private GameObject CreateChunk(Vector2Int chunk)
    {
        var root = new GameObject("Decor Chunk " + chunk.x + "," + chunk.y);
        root.transform.SetParent(transform, false);

        var count = Mathf.Abs(HashInt(chunk.x, chunk.y, 4)) % (maxDecorationsPerChunk + 1);
        for (var i = 0; i < count; i++)
        {
            var roll = Hash01(chunk.x, chunk.y, i * 11 + 9);
            if (roll < 0.35f)
                continue;

            var useLarge = !IsEmpty(largeDecorationSprites) && Hash01(chunk.x, chunk.y, i * 23 + 12) < largeDecorationChance;
            var sprites = useLarge ? largeDecorationSprites : decorationSprites;
            if (IsEmpty(sprites))
                sprites = largeDecorationSprites;

            var sprite = sprites[Mathf.Abs(HashInt(chunk.x, chunk.y, i * 17 + 2)) % sprites.Length];
            if (sprite == null)
                continue;

            var localX = Hash01(chunk.x, chunk.y, i * 31 + 6) * chunkSize;
            var localY = Hash01(chunk.x, chunk.y, i * 41 + 8) * chunkSize;
            var world = new Vector3(chunk.x * chunkSize + localX, chunk.y * chunkSize + localY, 0f);

            if (Vector2.Distance(world, player.position) < 5f)
                continue;

            var go = new GameObject("Map Decor");
            go.transform.SetParent(root.transform, false);
            go.transform.position = world;
            go.transform.localScale = Vector3.one * RandomScale(chunk.x, chunk.y, i, useLarge);

            AddShadow(go.transform, useLarge);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = useLarge ? 4 : 3;
        }

        return root;
    }

    private void AddShadow(Transform parent, bool isLarge)
    {
        if (shadowSprite == null)
            return;

        var shadow = new GameObject("Shadow");
        shadow.transform.SetParent(parent, false);
        shadow.transform.localPosition = new Vector3(0f, -0.16f, 0f);
        shadow.transform.localScale = isLarge ? new Vector3(1.8f, 0.55f, 1f) : new Vector3(1.15f, 0.38f, 1f);

        var renderer = shadow.AddComponent<SpriteRenderer>();
        renderer.sprite = shadowSprite;
        renderer.color = new Color(0f, 0f, 0f, isLarge ? 0.42f : 0.28f);
        renderer.sortingOrder = 1;
    }

    private Vector2Int WorldToChunk(Vector3 position)
    {
        return new Vector2Int(Mathf.FloorToInt(position.x / chunkSize), Mathf.FloorToInt(position.y / chunkSize));
    }

    private static float RandomScale(int x, int y, int salt, bool isLarge)
    {
        if (isLarge)
            return Mathf.Lerp(1.25f, 1.8f, Hash01(x, y, salt + 100));

        return Mathf.Lerp(0.85f, 1.35f, Hash01(x, y, salt + 100));
    }

    private static bool IsEmpty(Sprite[] sprites)
    {
        return sprites == null || sprites.Length == 0;
    }

    private static float Hash01(int x, int y, int salt)
    {
        return (HashInt(x, y, salt) & 0x7fffffff) / (float)int.MaxValue;
    }

    private static int HashInt(int x, int y, int salt)
    {
        unchecked
        {
            var hash = x * 73856093 ^ y * 19349663 ^ salt * 83492791;
            hash ^= hash << 13;
            hash ^= hash >> 17;
            hash ^= hash << 5;
            return hash;
        }
    }
}
