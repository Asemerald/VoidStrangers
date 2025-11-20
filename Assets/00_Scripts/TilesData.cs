using UnityEngine;
using UnityEngine.Tilemaps;

namespace _00_Scripts {
    [CreateAssetMenu(menuName = "ScriptableObjects/TileData")]
    public class TilesData : ScriptableObject {
        public TileBase[] tiles;
        
        public bool walkable;

        public enum TileType {
            None,
            Stair,
            Void,
            Paper,
            Chest,
            Rock,
            DoubleChest,
            RockStairs
        }
        
        public TileType tileType;
    }
}