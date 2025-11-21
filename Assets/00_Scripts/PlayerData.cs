using System;
using _00_Scripts.Save;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace _00_Scripts {
    public class PlayerData : MonoBehaviour {
        public static PlayerData Instance { get; private set; }

        public int healthPoints = 7;
        public int gemAmount { get; private set; }
        public int bugAmount { get; private set; }
        
        public bool hasScepter { get; private set; }
        
        public TileBase pickedUpTile { get; private set; }
        
        public int roomNumber { get; private set; }

        private void Awake() {
            if (Instance == null)
                Instance = this;
            else Destroy(this);
        }

        private void Start()
        {
            PlayerData.Instance.healthPoints = SaveManager.CurrentSaveData.HealthPoints;
            PlayerData.Instance.hasScepter = SaveManager.CurrentSaveData.HasScepter;
            PlayerData.Instance.bugAmount = SaveManager.CurrentSaveData.BugAmount;
        }

        //Health Methods
        public void SetHealthPoints(int healthPoints) {
            this.healthPoints = healthPoints;
        }
        
        public void AddHealthPoints(int healthPoints) {
            this.healthPoints += healthPoints;
        }

        public void SubtractHealthPoints(int healthPoints) {
            this.healthPoints -= healthPoints;
        }

        //Gem Methods
        public void SetGemAmount(int gemAmount) {
            this.gemAmount = gemAmount;
        }

        public void AddGemAmount(int gemAmount) {
            this.gemAmount += gemAmount;
        }

        public void SubtractGemAmount(int gemAmount) {
            this.gemAmount -= gemAmount;
        }

        //Scepter Methods
        public void SetHasScepter(bool hasScepter) {
            this.hasScepter = hasScepter;
        }
        
        //Bug Methods
        public void SetBugAmount(int bugAmount) {
            this.bugAmount = bugAmount;
        }

        public void AddBugAmount(int bugAmount) {
            this.bugAmount += bugAmount;
        }

        public void SubtractBugAmount(int bugAmount) {
            this.bugAmount -= bugAmount;
        }
        
        //Picked Up Tile Methods
        public void SetPickedUpTile(TileBase tile) {
            this.pickedUpTile = tile;
        }

        public void ResetPickedUpTile() {
            this.pickedUpTile = null;
        }
        
        //Room Methods
        public void SetRoomNumber(int roomNumber) {
            this.roomNumber = roomNumber;
        }
    }
}