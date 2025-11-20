using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _00_Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelSetup : Interactable {
    public static LevelSetup Instance { get; private set; }

    [Header("Levels")] 
    [SerializeField] private Transform tileGrid;
    [SerializeField] private LevelData[] levels;
    
    [Header("Tiles")]
    [SerializeField] private List<TilesData> tilesData;
    [SerializeField] private Transform gem;
    
    private Tilemap tileMap;
    private Transform currentLoadedGem;
    
    private bool gemPickedUp = false;
    private int currentLevel = 0;
    private List<GameObject> levelObjects = new List<GameObject>();

    private Dictionary<TileBase, TilesData> dataFromTiles =  new Dictionary<TileBase, TilesData>();
    
    private void Start() {
        if(Instance == null) Instance = this;
        else Destroy(this);

        foreach (var tileData in tilesData) {
            foreach (var tile in tileData.tiles) {
                dataFromTiles.Add(tile, tileData);
            }
        }
        
        LoadLevel(0);
    }

    private void LoadLevel(int level) {
        if (tileMap != null) {
            Destroy(tileMap.gameObject);
            tileMap = null;
        }
        
        foreach (var obj in levelObjects) {
            Destroy(obj.gameObject);
        }
        
        levelObjects.Clear();
        gemPickedUp = false;
        if(currentLoadedGem != null)
            Destroy(currentLoadedGem.gameObject);


        currentLevel = level;

        var player = FindAnyObjectByType<PlayerController>();
        player.transform.position = levels[currentLevel].playerPosition;
        
        tileMap = Instantiate(levels[level].tileMap, tileGrid);
        if (levels[level].hasGem) {
            currentLoadedGem = Instantiate(gem, levels[level].gemPosition, Quaternion.identity);
        }

        foreach (var obj in levels[level].objetcs) {
            var o = Instantiate(obj.objectToSpawn, obj.position, Quaternion.identity);
            levelObjects.Add(o);
        }
    }
    
    private void GetTile(Vector2Int position) {
        Debug.Log(tileMap.GetTile(new Vector3Int(position.x, position.y, 0)));
    }

    void PickUpTile(Vector3Int position)
    {
        var tile = tileMap.GetTile(position);
        
        foreach (var data in dataFromTiles) {
            if (dataFromTiles[data.Key].tileType is not TilesData.TileType.Void) continue;
            tileMap.SetTile(position, dataFromTiles[data.Key].tiles[0]);
            break;
        }
        
        PlayerData.Instance.SetPickedUpTile(tile);

        if (!currentLoadedGem || currentLoadedGem.position != position || gemPickedUp) return;
        PlayerData.Instance.AddGemAmount(1);
        gemPickedUp = true;
        currentLoadedGem.gameObject.SetActive(false);
    }

    void PlaceTile(Vector3Int position) {
        var tile = PlayerData.Instance.pickedUpTile;
        tileMap.SetTile(position, tile);
        PlayerData.Instance.SetPickedUpTile(null);
    }
    
    public bool CanMove(PlayerController player, Vector3 position) {
        var floorPos = Vector3Int.FloorToInt(position);
        var ceilPos = Vector3Int.CeilToInt(position);
        
        if (IsVoid(floorPos) || IsVoid(ceilPos)) {
            player.SetState(PlayerController.State.Edging);
            return false;
        }

        if (IsStairs(floorPos) || IsStairs(ceilPos)) {
            LoadLevel(levels[currentLevel].loadLevel);
            return false;
        }
        
        return dataFromTiles[tileMap.GetTile(ceilPos)].walkable && dataFromTiles[tileMap.GetTile(floorPos)].walkable;
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
