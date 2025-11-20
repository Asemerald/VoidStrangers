using System;
using System.Collections.Generic;
using _00_Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelSetup : MonoBehaviour {
    public static LevelSetup Instance { get; private set; }
    
    [Header("Tiles")]
    [SerializeField] private Tilemap tileMap;
    [SerializeField] private List<TilesData> tilesData;
    
    [Header("Gem")]
    [SerializeField] private Transform gem;

    private bool gemPickedUp = false;

    private Dictionary<TileBase, TilesData> dataFromTiles =  new Dictionary<TileBase, TilesData>();
    
    private void Start() {
        if(Instance == null) Instance = this;
        else Destroy(this);

        foreach (var tileData in tilesData) {
            foreach (var tile in tileData.tiles) {
                dataFromTiles.Add(tile, tileData);
            }
        }
    }

    private void GetTile(Vector2Int position) {
        Debug.Log(tileMap.GetTile(new Vector3Int(position.x, position.y, 0)));
    }

    public TileBase PickUpTile(Vector3Int position) {
        var tile = tileMap.GetTile(position);

        if (!dataFromTiles[tileMap.GetTile(position)].walkable) return null;
        
        foreach (var data in dataFromTiles) {
            if (dataFromTiles[data.Key].tileType is not TilesData.TileType.Void) continue;
            tileMap.SetTile(position, dataFromTiles[data.Key].tiles[0]);
            break;
        }
        
        PlayerData.Instance.SetPickedUpTile(tile);

        if (gem.position != position || gemPickedUp) return tile;
        PlayerData.Instance.AddGemAmount(1);
        gemPickedUp = true;

        return tile;
    }

    public bool CanPlaceTile(Vector3Int position, ref TileBase tile) {
        if (dataFromTiles[tileMap.GetTile(position)].tileType is not TilesData.TileType.Void) return false;
        
        tileMap.SetTile(position, tile);
        tile = null;
        return true;
    }
    
    public bool CanMove(Vector3 position) {
        var floorPos = Vector3Int.FloorToInt(position);
        var ceilPos = Vector3Int.CeilToInt(position);
        

        if (IsVoid(floorPos) || IsVoid(ceilPos)) {
            //Do something
        }

        if (IsStairs(floorPos) || IsStairs(ceilPos)) {
            //Do something
        }
        
        if (position.normalized != Vector3.zero) {
            if(position.normalized == Vector3.down || position.normalized == Vector3.up || position.normalized == Vector3.right || position.normalized == Vector3.left)
                return dataFromTiles[tileMap.GetTile(ceilPos)].walkable && dataFromTiles[tileMap.GetTile(floorPos)].walkable;
        }
              
        return false;
    }

    public void Interact(Vector3 position)
    {
        var fx = Mathf.FloorToInt(position.x);
        var fy = Mathf.FloorToInt(position.y);
        var fz = Mathf.FloorToInt(position.z);
        
        var cx = Mathf.CeilToInt(position.x);
        var cy = Mathf.CeilToInt(position.y);
        var cz = Mathf.CeilToInt(position.z);

        if (IsPaper(new Vector3Int(fx, fy, fz)))
        {
            //Do something
        }
    }

    private bool IsVoid(Vector3Int position) {
        return dataFromTiles[tileMap.GetTile(position)].tileType is TilesData.TileType.Void;
    }

    private bool IsStairs(Vector3Int position) {
        return dataFromTiles[tileMap.GetTile(position)].tileType is TilesData.TileType.Stair;
    }

    private bool IsPaper(Vector3Int position)
    {
        return dataFromTiles[tileMap.GetTile(position)].tileType is TilesData.TileType.Paper;
    }
}
