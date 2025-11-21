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
            saveData.MasterVolume = 100;
            saveData.IsFullScreen = true;
            saveData.ScreenResolution = ScreenResolution.R_1456x936;
            saveData.LastLevelCompleted = 0;
            saveData.HasScepter = false;
            return saveData;
        }
    }
    
    public enum ScreenResolution
    {
        R_1680x1080,
        R_1456x936,
    }
}