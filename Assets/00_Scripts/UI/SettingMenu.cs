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
            SaveManager.SaveGame();
            ApplySettings();
        });
        
        _document.rootVisualElement.Q<Toggle>("FullScreenToggle")?.RegisterValueChangedCallback(evt =>
        {
            SaveManager.CurrentSaveData.IsFullScreen = evt.newValue;
            SaveManager.SaveGame();
            ApplySettings();
        });
        
        var resolutionLabel = _document.rootVisualElement.Q<VisualElement>("Graphics-Tab").Q<VisualElement>("Middle").Q<VisualElement>("RightSide").Q<Label>("ResolutionLabel");

        resolutionLabel?.RegisterValueChangedCallback(evt =>
        {
                SaveData saveData = SaveManager.CurrentSaveData;

                if (saveData.ScreenResolution == ScreenResolution.R_1680x1080)
                {
                    saveData.ScreenResolution = ScreenResolution.R_1456x936;
                    resolutionLabel.text = "1456x936";
                    Screen.SetResolution(1456, 936, FullScreenMode.FullScreenWindow);
                }
                else
                { 
                    saveData.ScreenResolution = ScreenResolution.R_1680x1080;
                    resolutionLabel.text = "1680x1080";
                    Screen.SetResolution(1680, 1080, FullScreenMode.FullScreenWindow);
                }

                SaveManager.SaveGame();
                ApplySettings();
        });
    }
    
    private void LoadSettings()
{
    SaveData saveData = SaveManager.CurrentSaveData;

    _document.rootVisualElement
        .Q<SliderInt>("SliderInt").value = saveData.MasterVolume;

    _document.rootVisualElement
        .Q<Toggle>("FullScreenToggle").value = saveData.IsFullScreen;

    var label = _document.rootVisualElement
        .Q<VisualElement>("Graphics-Tab")
        .Q<VisualElement>("Middle")
        .Q<VisualElement>("RightSide")
        .Q<Label>("ResolutionLabel");

    label.text = saveData.ScreenResolution switch
    {
        ScreenResolution.R_1680x1080 => "1680x1080",
        ScreenResolution.R_1456x936 => "1456x936",
        _ => "1680x1080"
    };
}

    
    private void ApplySettings()
    {
        SaveData saveData = SaveManager.CurrentSaveData;
        
        AudioListener.volume = saveData.MasterVolume / 100f;
        Screen.fullScreen = saveData.IsFullScreen;
        
        switch (saveData.ScreenResolution)
        {
            case ScreenResolution.R_1680x1080:
                Screen.SetResolution(1680, 1080, saveData.IsFullScreen);
                break;
            case ScreenResolution.R_1456x936:
                Screen.SetResolution(1456, 936, saveData.IsFullScreen);
                break;
        }
    }
}
