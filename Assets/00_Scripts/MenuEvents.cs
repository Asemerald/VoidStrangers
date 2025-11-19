using UnityEngine;
using UnityEngine.UIElements;

public class ButtonManager : MonoBehaviour
{
    private UIDocument _document;
    private string currentTabName = "Main-Tab";
    private string previousTabName = "";
    [SerializeField] private bool debug;
    private void Awake()
    {
        _document = GetComponent<UIDocument>();

        foreach (var button in _document.rootVisualElement.Query<Button>().ToList())
        {
            button.RegisterCallback<ClickEvent>(evt => OnClick(evt, button.name));
        }
    }

    private void OnClick(ClickEvent evt, string buttonName)
    {
        if(debug)
            Debug.Log(buttonName+" clicked!");
        
        UpdateButtonImage(buttonName);
        
        switch (buttonName)
        {
            case "ResumeButton":
                ClosePauseMenu();
                break;
            
            case "QuitButton":
                Application.Quit();
                break;
            
            case "BackButton":
                PreviousTab();
                break;
            
            case "BackSettingsButton":
                OpenTab("Main-Tab");
                break;
            
            case "SettingsButton":
                OpenTab("Settings-Tab");
                break;
            
            case "GraphicsButton":
                OpenTab("Graphics-Tab");
                break;
            
            case "AudioButton":
                OpenTab("Audio-Tab");
                break;
          
        }
    }

    void PreviousTab()
    {
        if (previousTabName == "")
            return;
        
        OpenTab(previousTabName);
    }

    void OpenTab(string tabName)
    {
        if (_document.rootVisualElement.Q(tabName) == null)
            return;
        
        (currentTabName, previousTabName) = (tabName, currentTabName);
        
        _document.rootVisualElement.Q(previousTabName).style.display = DisplayStyle.None;
        _document.rootVisualElement.Q(currentTabName).style.display = DisplayStyle.Flex;
        
    }

    void UpdateButtonImage(string buttonName)
    {
        Debug.Log("WIP : Devrait changer l'image de droite");
    }

    void ClosePauseMenu()
    {
        Debug.Log("WIP : Devrait fermer le menu pause");
    }
}