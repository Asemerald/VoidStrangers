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
    private List<Transform> rocks = new List<Transform>();

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
        #region Reset
        if (tileMap != null) {
            Destroy(tileMap.gameObject);
            tileMap = null;
        }
        
        foreach (var obj in levelObjects) {
            Destroy(obj.gameObject);
        }
        
        rocks.Clear();
        levelObjects.Clear();
        gemPickedUp = false;
        if(currentLoadedGem != null)
            Destroy(currentLoadedGem.gameObject);
        
        #endregion
        
        currentLevel = level;

        var player = FindAnyObjectByType<PlayerController>();
        player.transform.position = levels[currentLevel].playerPosition;
        
        tileMap = Instantiate(levels[level].tileMap, tileGrid);
        if (levels[level].hasGem) { //Spawn Gem
            currentLoadedGem = Instantiate(gem, levels[level].gemPosition, Quaternion.identity);
        }

        #region SpawnObjects
        foreach (var obj in levels[level].objetcs) { //Spawn Objects
            var o = Instantiate(obj.objectToSpawn, obj.position, Quaternion.identity);
            levelObjects.Add(o);
            if(obj.tyle is TilesData.TileType.Rock or TilesData.TileType.RockStairs)
                rocks.Add(o.transform);
            
            if (obj.tyle is TilesData.TileType.None) continue; //Check pour changer la tile pour correspondre a l'objet
            foreach (var data in dataFromTiles) {
                if (obj.tyle is TilesData.TileType.DoubleChest) {
                    if (dataFromTiles[data.Key].tileType is TilesData.TileType.Chest) {
                        var doubleIntPos = Vector3Int.RoundToInt(obj.position);
                        doubleIntPos.z = 0;
                        tileMap.SetTile(doubleIntPos, dataFromTiles[data.Key].tiles[0]);
                        doubleIntPos.x += 1;
                        tileMap.SetTile(doubleIntPos, dataFromTiles[data.Key].tiles[0]);
                        break;
                    }
                }
                
                if (dataFromTiles[data.Key].tileType != obj.tyle) continue;
                
                var intPos = Vector3Int.RoundToInt(obj.position);
                intPos.z = 0;
                tileMap.SetTile(intPos, dataFromTiles[data.Key].tiles[0]);
                Debug.Log("Tyle type is " + obj.tyle);
                break;
                
            }
        }
        #endregion
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
    
    public bool CanMove(Vector3 position, Vector3 dir) {
        var intPos = Vector3Int.zero;
        
        switch (dir) {
            //Straight
            case { y: > 0, x: 0 }:
                intPos.x = Mathf.RoundToInt(position.x);
                intPos.y = Mathf.CeilToInt(position.y);
                break;
            case { y: < 0, x: 0 }:
                intPos.x = Mathf.RoundToInt(position.x);
                intPos.y = Mathf.FloorToInt(position.y);
                break;
            case { x: > 0, y: 0 }:
                intPos.x = Mathf.CeilToInt(position.x);
                intPos.y = Mathf.RoundToInt(position.y);
                break;
            case { x: < 0, y: 0 }:
                intPos.x = Mathf.FloorToInt(position.x);
                intPos.y = Mathf.RoundToInt(position.y);
                break;
            //Diagonal
            case { y: > 0, x: > 0 }:
                intPos.x = Mathf.CeilToInt(position.x);
                intPos.y = Mathf.CeilToInt(position.y);
                break;
            case { y: < 0, x: < 0 }:
                intPos.x = Mathf.FloorToInt(position.x);
                intPos.y = Mathf.FloorToInt(position.y);
                break;
            case { x: > 0, y: < 0 }:
                intPos.x = Mathf.CeilToInt(position.x);
                intPos.y = Mathf.FloorToInt(position.y);
                break;
            case { x: < 0, y: > 0 }:
                intPos.x = Mathf.FloorToInt(position.x);
                intPos.y = Mathf.CeilToInt(position.y);
                break;
        }
        intPos.z = 0;
        
        if (IsVoid(intPos)) {
            //Do something
        }

        if (IsStairs(intPos)) {
            LoadLevel(levels[currentLevel].loadLevel);
            return false;
        }

        switch (dataFromTiles[tileMap.GetTile(intPos)].tileType) {
            case TilesData.TileType.Rock:
                switch (dir) {
                    case { y: > 0, x: 0 }: //Up
                        if (CheckForMoveable(intPos, Vector3Int.up))
                            return false;
                        break;
                    case { y: < 0, x: 0 }: //Down
                        if (CheckForMoveable(intPos, Vector3Int.down))
                            return false;
                        break;
                    case { x: > 0, y: 0 }: //Right
                        if (CheckForMoveable(intPos, Vector3Int.right))
                            return false;
                        break;
                    case { x: < 0, y: 0 }: //Left
                        if (CheckForMoveable(intPos, Vector3Int.left))
                            return false;
                        break;
                }
                break;
            case TilesData.TileType.RockStairs:
                switch (dir) {
                    case { y: > 0, x: 0 }: //Up
                        if (CheckForMoveable(intPos, Vector3Int.up, true))
                            return false;
                        break;
                    case { y: < 0, x: 0 }: //Down
                        if (CheckForMoveable(intPos, Vector3Int.down, true))
                            return false;
                        break;
                    case { x: > 0, y: 0 }: //Right
                        if (CheckForMoveable(intPos, Vector3Int.right, true))
                            return false;
                        break;
                    case { x: < 0, y: 0 }: //Left
                        if (CheckForMoveable(intPos, Vector3Int.left, true))
                            return false;
                        break;
                }
                break;
        }
        
        return dataFromTiles[tileMap.GetTile(intPos)].walkable;
    }

    bool CheckForMoveable(Vector3Int pos, Vector3Int dir, bool stairs = false) {
        if (dataFromTiles[tileMap.GetTile(pos + dir)].tileType is TilesData.TileType.Void) {
            DestroyRock(pos, Vector3Int.up, stairs);
            return true;
        }
                    
        if (dataFromTiles[tileMap.GetTile(pos + dir)].walkable) {
            MoveRock(pos, dir, stairs);
            return true;
        }
        
        return false;
    }
    
    void MoveRock(Vector3Int pos, Vector3Int dir, bool stairs) {
        if (stairs) {
            foreach (var data in dataFromTiles) {
                if (dataFromTiles[data.Key].tileType is not TilesData.TileType.Stair) continue;
                tileMap.SetTile(pos, dataFromTiles[data.Key].tiles[0]);
                break;
            }
        }
        else
            tileMap.SetTile(pos, tileMap.GetTile(pos + dir));
        foreach (var data in dataFromTiles) {
            if (dataFromTiles[data.Key].tileType is not TilesData.TileType.Rock) continue;
            tileMap.SetTile(pos + dir, dataFromTiles[data.Key].tiles[0]);
            break;
        }

        pos.z = -1;
        
        foreach (var rock in rocks) {
            if (rock.position != pos) continue;
            rock.position = pos + dir;
            break;
        }
        
        Debug.Log("Moved");
        Debug.Log(stairs);
    }

    void DestroyRock(Vector3Int pos, Vector3Int dir, bool stairs) {
        if (stairs) {
            foreach (var data in dataFromTiles) {
                if (dataFromTiles[data.Key].tileType is not TilesData.TileType.Stair) continue;
                tileMap.SetTile(pos, dataFromTiles[data.Key].tiles[0]);
                break;
            }
        }
        else {
            foreach (var data in dataFromTiles) {
                if (!dataFromTiles[data.Key].walkable) continue;
                tileMap.SetTile(pos, dataFromTiles[data.Key].tiles[0]);
                break;
            }
        }

        pos.z = -1;
        
        foreach (var rock in rocks) {
            if (rock.position != pos) continue;
            Destroy(rock.gameObject);
            rocks.Remove(rock);
            break;
        }
        Debug.Log("Destroyed");
        Debug.Log(stairs);
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
