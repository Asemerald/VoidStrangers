using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _00_Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelSetup : Interactable {
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

    public TileBase PickUpTile(Vector3Int position)
    {
        var tile = tileMap.GetTile(position);
        
        foreach (var data in dataFromTiles) {
            if (dataFromTiles[data.Key].tileType is not TilesData.TileType.Void)
                continue;
            if (!IsVoid(position + Vector3Int.up))
                tileMap.SetTile(position, dataFromTiles[data.Key].tiles[2]);
            else
                tileMap.SetTile(position, dataFromTiles[data.Key].tiles[0]);
            break;
        }
        
        PlayerData.Instance.SetPickedUpTile(tile);

        if (!gem || gem.position != position || gemPickedUp) return tile;
        PlayerData.Instance.AddGemAmount(1);
        gemPickedUp = true;

        return tile;
    }

    public void PlaceTile(Vector3Int position) {
        var tile = PlayerData.Instance.pickedUpTile;
        tileMap.SetTile(position, tile);
        PlayerData.Instance.SetPickedUpTile(null);
    }
    
    public bool CanMove(Vector3 position) {
        var fx = Mathf.FloorToInt(position.x);
        var fy = Mathf.FloorToInt(position.y);
        var fz = Mathf.FloorToInt(position.z);
        
        var cx = Mathf.CeilToInt(position.x);
        var cy = Mathf.CeilToInt(position.y);
        var cz = Mathf.CeilToInt(position.z);

        if (IsVoid(new Vector3Int(fx,fy,fz))) {
            //Do something
        }

        if (IsStairs(new Vector3Int(fx,fy,fz))) {
            //Do something
        }

        if (dataFromTiles[tileMap.GetTile(new Vector3Int(fx, fy, fz))].walkable) {
            if(!dataFromTiles[tileMap.GetTile(new Vector3Int(cx, cy, cz))].walkable) return false;
            
            return true;
        }
        
        if (dataFromTiles[tileMap.GetTile(new Vector3Int(cx, cy, cz))].walkable) {
            if(!dataFromTiles[tileMap.GetTile(new Vector3Int(fx, fy, fz))].walkable) return false;
            
            return true;
        }
        
        return false;
    }

    public override void Interact(PlayerController player, Vector2 position)
    {
        var fx = Mathf.FloorToInt(player.transform.position.x + position.x);
        var fy = Mathf.FloorToInt(player.transform.position.y + position.y);
        var fz = Mathf.FloorToInt(0);
        
        var cx = Mathf.CeilToInt(player.transform.position.x + position.x);
        var cy = Mathf.CeilToInt(player.transform.position.y + position.y);
        var cz = Mathf.CeilToInt(0);

        var floorBlock = new Vector3Int(fx, fy, fz);
        
        if (IsPaper(floorBlock))
        {
            //Do something
            return;
        }
        
        if (IsFloor(floorBlock) && !PlayerData.Instance.pickedUpTile)
        {
            PickUpTile(floorBlock);
            return;
        }
        
        if (IsVoid(floorBlock))
        {
            PlaceTile(floorBlock);
            return;
        }

        if (IsStairs(floorBlock))
        {
            //Do something
            return;
        }
    }
    
    private bool IsFloor(Vector3Int position) {
        return dataFromTiles[tileMap.GetTile(position)].walkable;
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
