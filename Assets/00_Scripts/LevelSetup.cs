using System;
using System.Collections.Generic;
using _00_Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelSetup : MonoBehaviour {
    public static LevelSetup Instance { get; private set; }
    
    [SerializeField] private Tilemap tileMap;
    [SerializeField] private List<TilesData> tilesData;
    
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
