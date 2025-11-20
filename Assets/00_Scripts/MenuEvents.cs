using UnityEngine;
using UnityEngine.UIElements;

public class ButtonManager : MonoBehaviour
{
    private UIDocument _document;
    private string currentTabName = "Main-Tab";
    private string previousTabName = "";
    private int currentPageDepth = 1 ;
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
                OpenTab("Main-Tab",-1);
                break;
            
            case "SettingsButton":
                OpenTab("Settings-Tab",1);
                break;
            
            case "GraphicsButton":
                OpenTab("Graphics-Tab",1);
                break;
            
            case "AudioButton":
                OpenTab("Audio-Tab",1);
                break;
          
        }
    }

    void PreviousTab()
    {
        if (previousTabName == "")
            return;
        
        OpenTab(previousTabName,-1);
    }

    void OpenTab(string tabName, int pageDepthUpdate = 0)
    {
        if (_document.rootVisualElement.Q(tabName) == null)
            return;
        
        (currentTabName, previousTabName) = (tabName, currentTabName);
        
        _document.rootVisualElement.Q(previousTabName).style.display = DisplayStyle.None;
        _document.rootVisualElement.Q(currentTabName).style.display = DisplayStyle.Flex;

        currentPageDepth += pageDepthUpdate;
        
        UpdatePageClass();
    }

    void UpdateButtonImage(string buttonName)
    {
        if(debug)
            Debug.Log("WIP : Devrait changer l'image de droite");
    }

    void ClosePauseMenu()
    {
        _document.rootVisualElement.Q(currentTabName).style.display = DisplayStyle.None;
    }
    
    private void UpdatePageClass()
    {
        VisualElement pageContainer = _document.rootVisualElement.Q("PageCounter");
        for (int i = 1; i <= 3; i++)
            pageContainer.RemoveFromClassList($"page-{i}");

        pageContainer.AddToClassList($"page-{currentPageDepth}");
    }
}