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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                SaveGame();
                Debug.Log("Game Saved!");
            }
            else if (Input.GetKeyDown(KeyCode.F9))
            {
                DeleteSaveData();
                Debug.Log("Save Data Deleted!");
            }
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
        
        private static void CreateNewSaveData()
        {
            CurrentSaveData = Save.SaveData.CreateDefault();
            SaveGame();
        }
        
        public static void SaveGame()
        {
            string json = JsonUtility.ToJson(CurrentSaveData, true);
            System.IO.File.WriteAllText(_saveFilePath, json);
        }
        
        public static void DeleteSaveData()
        {
            if (System.IO.File.Exists(_saveFilePath))
            {
                System.IO.File.Delete(_saveFilePath);
            }
            CreateNewSaveData();
        }
        
        private void OnApplicationQuit()
        {
            SaveGame();
        }
        
        
    }
}