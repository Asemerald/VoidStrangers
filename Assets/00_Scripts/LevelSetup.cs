using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _00_Scripts;
using _00_Scripts.Save;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelSetup : Interactable {
    public static LevelSetup Instance { get; private set; }

    public int CurrentLevelIndex => currentLevel;

    [Header("Levels")] 
    [SerializeField] private Transform tileGrid;
    [SerializeField] private LevelData[] levels;
    
    [Header("Tiles")]
    [SerializeField] private List<TilesData> tilesData;
    [SerializeField] private Transform gem;
    
    public Tilemap tileMap { get; private set; }
    private Transform currentLoadedGem;
    
    private bool gemPickedUp = false;
    private int currentLevel = 0;
    private List<GameObject> levelObjects = new List<GameObject>();
    private List<Transform> rocks = new List<Transform>();

    public Dictionary<TileBase, TilesData> dataFromTiles =  new Dictionary<TileBase, TilesData>();
    
    private void Start() {
        if(Instance == null) Instance = this;
        else Destroy(this);

        foreach (var tileData in tilesData) {
            foreach (var tile in tileData.tiles) {
                dataFromTiles.Add(tile, tileData);
            }
        }
        
        // Load last level using the SaveData, if he completed the game, load level 0
        var lastLevel = SaveManager.CurrentSaveData.LastLevelCompleted;
        if (lastLevel >= levels.Length)
            lastLevel = 0;
        LoadLevel(lastLevel);
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
        PlayerData.Instance.ResetPickedUpTile();

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
                break;
                
            }
        }
        #endregion
    }

    public int ReloadLevel()
    {
        LoadLevel(currentLevel);
        return currentLevel;
    }
    
    private void GetTile(Vector2Int position) {
        Debug.Log(tileMap.GetTile(new Vector3Int(position.x, position.y, 0)));
    }

    private void PickUpTile(Vector3Int position)
    {
        var tile = tileMap.GetTile(position);
        
        foreach (var data in dataFromTiles) {
            if (dataFromTiles[data.Key].tileType is not TilesData.TileType.Void)
                continue;
            if (!IsVoid(position + Vector3Int.up))
                tileMap.SetTile(position, dataFromTiles[data.Key].tiles[2]);
            else
                tileMap.SetTile(position, dataFromTiles[data.Key].tiles[0]);
            if (IsVoid(position + Vector3Int.down))
                tileMap.SetTile(position + Vector3Int.down, dataFromTiles[data.Key].tiles[0]);
            break;
        }
        
        PlayerData.Instance.SetPickedUpTile(tile);

        if (!currentLoadedGem || currentLoadedGem.position != position || gemPickedUp) return;
        PlayerData.Instance.AddGemAmount(1);
        gemPickedUp = true;
        currentLoadedGem.gameObject.SetActive(false);
    }

    private void PlaceTile(Vector3Int position) {
        var tile = PlayerData.Instance.pickedUpTile;
        tileMap.SetTile(position, tile);
        
        foreach (var data in dataFromTiles) {
            if (dataFromTiles[data.Key].tileType is not TilesData.TileType.Void)
                continue;
            if (IsVoid(position + Vector3Int.down))
                tileMap.SetTile(position + Vector3Int.down, dataFromTiles[data.Key].tiles[2]);
            break;
        }
        
        PlayerData.Instance.ResetPickedUpTile();
    }

    public bool CanEnemyMove(Vector3 position, Vector3 dir) {
        var intPos = Vector3Int.zero;
        intPos.x = dir.x > 0 ? Mathf.CeilToInt(position.x) : Mathf.FloorToInt(position.x);
        intPos.y = dir.y > 0 ? Mathf.CeilToInt(position.y) : Mathf.FloorToInt(position.y);
        intPos.z = 0;

        if (!IsFloor(intPos)) {
            intPos.x = Mathf.RoundToInt(position.x);
            intPos.y = Mathf.CeilToInt(position.y);
        
            intPos.x = Mathf.CeilToInt(position.x);
            intPos.y = Mathf.RoundToInt(position.y);
        }
        
        if (IsVoid(intPos)) {
            return false;
        }

        if (IsStairs(intPos)) {
            return false;
        }

        if (!tileMap.GetTile(intPos))
            return false;
        
        return IsFloor(intPos);
    }
    
    public bool CanMove(Vector3 position, Vector3 dir, ref Vector3 deltaPosition) {
        var intPos = Vector3Int.zero;
        intPos.x = dir.x > 0 ? Mathf.CeilToInt(position.x) : Mathf.FloorToInt(position.x);
        intPos.y = dir.y > 0 ? Mathf.CeilToInt(position.y) : Mathf.FloorToInt(position.y);
        intPos.z = 0;
        
        var intDir = new Vector3Int(Mathf.CeilToInt(dir.x), Mathf.CeilToInt(dir.y), Mathf.CeilToInt(dir.z));

        if (!IsFloor(intPos))
        {
            intPos.x = Mathf.RoundToInt(position.x);
            intPos.y = Mathf.CeilToInt(position.y);
            if (!IsFloor(intPos) || !IsFloor(intPos + Vector3Int.down))
                deltaPosition.y = 0f;
        
            intPos.x = Mathf.CeilToInt(position.x);
            intPos.y = Mathf.RoundToInt(position.y);
            if (!IsFloor(intPos) || !IsFloor(intPos + Vector3Int.left))
                deltaPosition.x = 0f;
        }

        if (IsEnemy(intPos) ||IsEnemy(intPos - intDir)) {
            Debug.LogWarning("Ennemy Tile");
            ReloadLevel();
            return false;
        }
        
        if (IsVoid(intPos)) {
            if (PlayerData.Instance.hasScepter)
                PlayerController.Instance.SetState(PlayerController.State.Edging);
            return false;
        }

        if (IsStairs(intPos)) {
            LoadLevel(levels[currentLevel].loadLevel);
            // Save the level completion
            SaveManager.CurrentSaveData.LastLevelCompleted = currentLevel;
            
            SaveManager.SaveGame();
            
            // Play Next Level Music
            MusicManager.Instance.PlayGameplayMusic(currentLevel + 1);
            return false;
        }

        if (!tileMap.GetTile(intPos))
            return false;

        var tileIsRock = dataFromTiles[tileMap.GetTile(intPos)].tileType == TilesData.TileType.RockStairs
                         || dataFromTiles[tileMap.GetTile(intPos)].tileType == TilesData.TileType.Rock;

        if (!tileIsRock)
            return IsFloor(intPos);
        
        var tileIsRockStairs = dataFromTiles[tileMap.GetTile(intPos)].tileType == TilesData.TileType.RockStairs;
        return !CheckForMoveable(intPos, intDir, tileIsRockStairs) && IsFloor(intPos);
    }

    private bool IsEnemy(Vector3Int pos) {
        return tileMap.GetTile(pos) && !IsVoid(pos) && dataFromTiles[tileMap.GetTile(pos)].walkable && dataFromTiles[tileMap.GetTile(pos)].tileType is TilesData.TileType.Enemy;
    }

    private bool CheckForMoveable(Vector3Int pos, Vector3Int dir, bool stairs = false) {
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
    
    private void MoveRock(Vector3Int pos, Vector3Int dir, bool stairs) {
        if (stairs) {
            foreach (var data in dataFromTiles) {
                if (dataFromTiles[data.Key].tileType is not TilesData.TileType.Stair) continue;
                tileMap.SetTile(pos, dataFromTiles[data.Key].tiles[0]);
                break;
            }
        }
        else {
            if (dataFromTiles[tileMap.GetTile(pos + dir)].tileType is TilesData.TileType.Stair) {
                foreach (var data in dataFromTiles) {
                    if (dataFromTiles[data.Key].walkable &&
                        dataFromTiles[data.Key].tileType is not TilesData.TileType.Stair) {
                        tileMap.SetTile(pos, dataFromTiles[data.Key].tiles[0]);
                        break;
                    }
                }
                
                foreach (var dat in dataFromTiles) {
                    if (dataFromTiles[dat.Key].tileType is not TilesData.TileType.RockStairs) continue;
                    tileMap.SetTile(pos + dir, dataFromTiles[dat.Key].tiles[0]);
                    break;
                }

                pos.z = -1;
        
                foreach (var rock in rocks) {
                    if (rock.position != pos) continue;
                    rock.position = pos + dir;
                    break;
                }
                
                return;
            }
            
            tileMap.SetTile(pos, tileMap.GetTile(pos + dir));
        }
        
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

    private void DestroyRock(Vector3Int pos, Vector3Int dir, bool stairs) {
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
    
    public override int Interact(PlayerController player, Vector2 position)
    {
        var fx = Mathf.FloorToInt(player.transform.position.x + position.x);
        var fy = Mathf.FloorToInt(player.transform.position.y + position.y);
        var floorBlock = new Vector3Int(fx, fy, 0);
        
        if (IsPaper(floorBlock)) {
            //Do something
            return 0;
        }
        
        if (IsFloor(floorBlock) && !IsVoid(floorBlock) && !PlayerData.Instance.pickedUpTile) {
            PickUpTile(floorBlock);
            return 1;
        }
        
        if (IsVoid(floorBlock) && PlayerData.Instance.pickedUpTile) {
            PlaceTile(floorBlock);
            return 2;
        }

        if (IsStairs(floorBlock)) {
            //Do something
            return 0;
        }

        return 0;
    }
    
    private bool IsFloor(Vector3Int position) {
        return tileMap.GetTile(position) && !IsVoid(position) && dataFromTiles[tileMap.GetTile(position)].walkable;
    }

    private bool IsVoid(Vector3Int position) {
        if (!tileMap.GetTile(position))
            return true;
        return dataFromTiles[tileMap.GetTile(position)].tileType is TilesData.TileType.Void;
    }

    private bool IsStairs(Vector3Int position) {
        if (!tileMap.GetTile(position))
            return false;
        return dataFromTiles[tileMap.GetTile(position)].tileType is TilesData.TileType.Stair;
    }

    private bool IsPaper(Vector3Int position)
    {
        if (!tileMap.GetTile(position))
            return false;
        return dataFromTiles[tileMap.GetTile(position)].tileType is TilesData.TileType.Paper;
    }
}
