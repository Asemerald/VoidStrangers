using _00_Scripts.Save;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingMenu : MonoBehaviour
{
    private UIDocument _document;
    
    private void Awake()
    {
        _document = GetComponent<UIDocument>();
    }
    
    private void Start()
    {
        LoadSettings();
        ApplySettings();
        
        //Handle UI interactions here 
        
        _document.rootVisualElement.Q<SliderInt>("SliderInt")?.RegisterValueChangedCallback(evt =>
        {
            SaveManager.CurrentSaveData.MasterVolume = (int)evt.newValue;
            SaveManager.SaveData();
            ApplySettings();
        });
        
        _document.rootVisualElement.Q<Toggle>("FullScreenToggle")?.RegisterValueChangedCallback(evt =>
        {
            SaveManager.CurrentSaveData.IsFullScreen = evt.newValue;
            SaveManager.SaveData();
            ApplySettings();
        });
        
        _document.rootVisualElement.Q<Foldout>("Resolution")?.RegisterValueChangedCallback(evt =>
        {
            SaveData saveData = SaveManager.CurrentSaveData;
            ScreenResolution[] resolutions = (ScreenResolution[])System.Enum.GetValues(typeof(ScreenResolution));
            int currentIndex = System.Array.IndexOf(resolutions, saveData.ScreenResolution);
            int nextIndex = (currentIndex + 1) % resolutions.Length;
            saveData.ScreenResolution = resolutions[nextIndex];
            _document.rootVisualElement.Q<Foldout>("Resolution").text = saveData.ScreenResolution.ToString();
            SaveManager.SaveData();
            ApplySettings();
        });
    }
    
    private void LoadSettings()
    {
        SaveData saveData = SaveManager.CurrentSaveData;
        
        
        SliderInt masterVolumeSlider = _document.rootVisualElement.Q<SliderInt>("SliderInt");
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = saveData.MasterVolume;
            masterVolumeSlider.RegisterValueChangedCallback(evt =>
            {
                saveData.MasterVolume = (int)evt.newValue;
                SaveManager.SaveData();
            });
        }

        Toggle fullScreenToggle = _document.rootVisualElement.Q<Toggle>("FullScreenToggle");
        if (fullScreenToggle != null)
        {
            fullScreenToggle.value = saveData.IsFullScreen;
            fullScreenToggle.RegisterValueChangedCallback(evt =>
            {
                saveData.IsFullScreen = evt.newValue;
                SaveManager.SaveData();
            });
        }

        Foldout resolutionDropdown = _document.rootVisualElement.Q<Foldout>("Resolution");
        if (resolutionDropdown != null)
        {
            resolutionDropdown.value = false; 
            resolutionDropdown.text = saveData.ScreenResolution.ToString();
            resolutionDropdown.RegisterValueChangedCallback(evt =>
            {
                // Handle resolution change logic here
                // For simplicity, we just cycle through the enum values
                ScreenResolution[] resolutions = (ScreenResolution[])System.Enum.GetValues(typeof(ScreenResolution));
                int currentIndex = System.Array.IndexOf(resolutions, saveData.ScreenResolution);
                int nextIndex = (currentIndex + 1) % resolutions.Length;
                saveData.ScreenResolution = resolutions[nextIndex];
                resolutionDropdown.text = saveData.ScreenResolution.ToString();
                SaveManager.SaveData();
            });
        }
    }
    
    private void ApplySettings()
    {
        SaveData saveData = SaveManager.CurrentSaveData;
        
        AudioListener.volume = saveData.MasterVolume / 100f;
        Screen.fullScreen = saveData.IsFullScreen;
        
        switch (saveData.ScreenResolution)
        {
            case ScreenResolution.R_1920x1080:
                Screen.SetResolution(1920, 1080, saveData.IsFullScreen);
                break;
            case ScreenResolution.R_1600x900:
                Screen.SetResolution(1600, 900, saveData.IsFullScreen);
                break;
            case ScreenResolution.R_1366x768:
                Screen.SetResolution(1366, 768, saveData.IsFullScreen);
                break;
            case ScreenResolution.R_1280x720:
                Screen.SetResolution(1280, 720, saveData.IsFullScreen);
                break;
        }
    }
    
}
