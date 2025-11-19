using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelSetup : MonoBehaviour {
    public static LevelSetup Instance { get; private set; }
    
    [SerializeField] private Tilemap tileMap;
    
    private void Start() {
        if(Instance == null) Instance = this;
        else Destroy(this);
    }

    public void GetTile(Vector2Int position) {
        Debug.Log(tileMap.GetTile(new Vector3Int(position.x - 1, position.y - 1, 0)));
    }
    
}
