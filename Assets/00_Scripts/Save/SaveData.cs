namespace _00_Scripts.Save
{
    [System.Serializable]
    public class SaveData
    {
        // TODO : Define save data fields here
        
        public int MasterVolume;
        public bool IsFullScreen;
        public ScreenResolution ScreenResolution;
        
        public static SaveData CreateDefault()
        {
            SaveData saveData = new SaveData();
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