using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace _00_Scripts {
    [CreateAssetMenu(menuName = "LevelData")]
    public class LevelData : ScriptableObject {
        [Header("Level Data")]
        public Tilemap tileMap;
        public int loadLevel = 0;
        
        [Header("PlayerPosition")]
        public Vector3 playerPosition;
        
        [Header("Object Data")]
        public Objects[] objetcs;
        public bool hasGem = false;
        public Vector3 gemPosition;
        
    }

    [Serializable]
    public struct Objects {
        public GameObject objectToSpawn;
        public Vector3 position;
    }
}