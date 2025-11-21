using System;
using UnityEngine;

namespace _00_Scripts.Save
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }
        private const string SaveFileName = "savefile.json";
        
        private static string _saveFilePath;
        
        public static SaveData CurrentSaveData { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            
            _saveFilePath = System.IO.Path.Combine(Application.persistentDataPath, SaveFileName);
            
            LoadOrCreateSaveData();
        }
        

        private void LoadOrCreateSaveData()
        {
            if (System.IO.File.Exists(_saveFilePath))
            {
                LoadSaveData();
            }
            else
            {
                CreateNewSaveData();
            }
        }
        
        private void LoadSaveData()
        {
            string json = System.IO.File.ReadAllText(_saveFilePath);
            CurrentSaveData = JsonUtility.FromJson<SaveData>(json);
        }
        
        private void CreateNewSaveData()
        {
            CurrentSaveData = Save.SaveData.CreateDefault();
            SaveGame();
        }
        
        public static void SaveGame()
        {
            string json = JsonUtility.ToJson(CurrentSaveData, true);
            System.IO.File.WriteAllText(_saveFilePath, json);
        }
        
        
    }
}