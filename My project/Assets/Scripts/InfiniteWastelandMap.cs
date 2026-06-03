using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InfiniteWastelandMap : MonoBehaviour
{
    public Tilemap groundTilemap;
    public Tilemap detailTilemap;
    public TileBase[] dirtTiles;
    public TileBase[] crackTiles;
    public TileBase[] roadTiles;
    public Transform player;
    public int renderRadius = 48;
    public int updateStep = 8;
    public int roadSpacing = 30;
    public int roadHalfWidth = 3;

    private readonly HashSet<Vector3Int> activeCells = new();
    private Vector3Int lastCenterCell = new(99999, 99999, 0);

    private void Start()
    {
        RefreshMap(true);
    }

    private void Update()
    {
        RefreshMap(false);
    }

    private void RefreshMap(bool force)
    {
        if (groundTilemap == null || player == null)
            return;

        var center = groundTilemap.WorldToCell(player.position);
        center.z = 0;

        if (!force && Mathf.Abs(center.x - lastCenterCell.x) < updateStep && Mathf.Abs(center.y - lastCenterCell.y) < updateStep)
            return;

        lastCenterCell = center;

        var nextCells = new HashSet<Vector3Int>();
        for (var x = center.x - renderRadius; x <= center.x + renderRadius; x++)
        {
            for (var y = center.y - renderRadius; y <= center.y + renderRadius; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                nextCells.Add(cell);

                if (!activeCells.Contains(cell))
                    PaintCell(cell);
            }
        }

        foreach (var cell in activeCells)
        {
            if (nextCells.Contains(cell))
                continue;

            groundTilemap.SetTile(cell, null);
            if (detailTilemap != null)
                detailTilemap.SetTile(cell, null);
        }

        activeCells.Clear();
        foreach (var cell in nextCells)
            activeCells.Add(cell);
    }

    private void PaintCell(Vector3Int cell)
    {
        var tile = IsRoad(cell) ? PickTile(roadTiles, cell.x, cell.y, 9) : PickGroundTile(cell);
        groundTilemap.SetTile(cell, tile);

        if (detailTilemap == null)
            return;

        var detailRoll = Hash01(cell.x, cell.y, 33);
        if (!IsRoad(cell) && detailRoll > 0.88f && crackTiles != null && crackTiles.Length > 0)
            detailTilemap.SetTile(cell, PickTile(crackTiles, cell.x, cell.y, 17));
        else
            detailTilemap.SetTile(cell, null);
    }

    private TileBase PickGroundTile(Vector3Int cell)
    {
        var roll = Hash01(cell.x, cell.y, 5);
        if (crackTiles != null && crackTiles.Length > 0 && roll > 0.82f)
            return PickTile(crackTiles, cell.x, cell.y, 21);

        return PickTile(dirtTiles, cell.x, cell.y, 3);
    }

    private bool IsRoad(Vector3Int cell)
    {
        var x = PositiveMod(cell.x, roadSpacing);
        var y = PositiveMod(cell.y, roadSpacing);
        return x <= roadHalfWidth || x >= roadSpacing - roadHalfWidth
            || y <= roadHalfWidth || y >= roadSpacing - roadHalfWidth;
    }

    private static TileBase PickTile(TileBase[] tiles, int x, int y, int salt)
    {
        if (tiles == null || tiles.Length == 0)
            return null;

        var index = Mathf.Abs(HashInt(x, y, salt)) % tiles.Length;
        return tiles[index];
    }

    private static int PositiveMod(int value, int mod)
    {
        var result = value % mod;
        return result < 0 ? result + mod : result;
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
