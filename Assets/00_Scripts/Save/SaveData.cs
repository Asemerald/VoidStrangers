namespace _00_Scripts.Save
{
    [System.Serializable]
    public class SaveData
    {
        // TODO : Define save data fields here
        
        public int MasterVolume;
        public bool IsFullScreen;
        public ScreenResolution ScreenResolution;
        
        public int LastLevelCompleted;
        
        public int HealthPoints;
        public int BugAmount;
        public bool HasScepter;
        public bool FreeMove;

        public static SaveData CreateDefault()
        {
            SaveData saveData = new SaveData();
            saveData.FreeMove = true;
            saveData.HealthPoints = 7;
            // TODO : Initialize default save data values here
            return saveData;
        }
    }
    
    public enum ScreenResolution
    {
        R_1920x1080,
        R_1600x900,
        R_1366x768,
        R_1280x720
    }
}