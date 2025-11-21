using System;
using _00_Scripts;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class EnemyController : MonoBehaviour
{
    public enum MoveDir {
        Horizontal,
        Vertical
    }

    public enum CurrentDir {
        Right,
        Left,
        Up,
        Down,
    }
    
    public MoveDir moveDir;
    public CurrentDir currentDir;

    private void OnEnable() {
        TurnManager.Instance.OnTurn += OnTurnUpdate;
    }
    
    private void OnDisable() {
        TurnManager.Instance.OnTurn -= OnTurnUpdate;
    }

    private void OnTurnUpdate() {
        switch (moveDir) {
            case MoveDir.Horizontal:
                MoveEnemyHorizontal();
                break;
            case MoveDir.Vertical:
                MoveEnemyVertical();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void MoveEnemyHorizontal() {
        var intPos = Vector3Int.RoundToInt(transform.position);
        if (currentDir is CurrentDir.Right) {
            if (LevelSetup.Instance.CanEnemyMove(intPos + Vector3Int.right, Vector3.right)) {
                UpdateTile(intPos, intPos + Vector3Int.right);
                transform.position += Vector3.right;
            }
            else {
                currentDir = CurrentDir.Left;
            }
        }
        else if (currentDir is CurrentDir.Left) {
            if (LevelSetup.Instance.CanEnemyMove(intPos + Vector3Int.left, Vector3.left)) {
                UpdateTile(intPos, intPos + Vector3Int.left);
                transform.position += Vector3.left;
            }
            else {
                currentDir = CurrentDir.Right;
            }
        }
        else {
            Debug.LogError("Unsupported MoveDir");
        }
    }

    private void MoveEnemyVertical() {
        var intPos = Vector3Int.RoundToInt(transform.position);
        if (currentDir is CurrentDir.Up) {
            if (LevelSetup.Instance.CanEnemyMove(intPos + Vector3Int.up, Vector3.up)) {
                UpdateTile(intPos, intPos + Vector3Int.up);
                transform.position += Vector3.up;
            }
            else {
                //Changer de dir
                currentDir = CurrentDir.Down;
            }
        }
        else if (currentDir is CurrentDir.Down) {
            if (LevelSetup.Instance.CanEnemyMove(intPos + Vector3Int.down, Vector3.down)) {
                UpdateTile(intPos, intPos + Vector3Int.down);
                transform.position += Vector3.down;
            }
            else {
                //Changer de dir
                currentDir = CurrentDir.Up;
            }
        }
        else {
            Debug.LogError("Unsupported MoveDir");
        }
    }
    
    void UpdateTile(Vector3Int previousPos, Vector3Int newPos) {
        foreach (var data in LevelSetup.Instance.dataFromTiles) {
            if (LevelSetup.Instance.dataFromTiles[data.Key].walkable &&
                LevelSetup.Instance.dataFromTiles[data.Key].tileType is TilesData.TileType.None) {
                LevelSetup.Instance.tileMap.SetTile(previousPos, LevelSetup.Instance.dataFromTiles[data.Key].tiles[0]);
                break;
            }
        }
        
        foreach (var data in LevelSetup.Instance.dataFromTiles) {
            if (LevelSetup.Instance.dataFromTiles[data.Key].tileType is TilesData.TileType.Enemy) {
                LevelSetup.Instance.tileMap.SetTile(newPos, LevelSetup.Instance.dataFromTiles[data.Key].tiles[0]);
                break;
            }
        }
    }
}
