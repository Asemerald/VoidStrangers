using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonManager : MonoBehaviour
{
    private UIDocument _document;
    private string currentTabName = "Main-Tab";
    private string previousTabName = "";
    private int currentButtonIndex = 0;
    private int currentPageDepth = 1 ;
    private VisualElement currentButtonContainer = null;
    [SerializeField] private bool debug;
    private void Awake()
    {
        _document = GetComponent<UIDocument>();

        foreach (var button in _document.rootVisualElement.Query<Button>().ToList())
        {
            button.RegisterCallback<ClickEvent>(evt => OnClick(button.name));
        }
    }

    private void Start()
    {
        currentButtonContainer = _document.rootVisualElement.Q<VisualElement>(currentTabName).Q<VisualElement>("Middle").Q<VisualElement>("LeftSide");
        
        UpdateButtonSelected();
    }

    private void Update()
    {
        //HandleKeyboardInput();
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePauseMenu();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (debug) Debug.Log("RightArrow ");
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (debug) Debug.Log("LeftArrow ");
        }
        
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (debug) Debug.Log("UpArrow ");
            UpdateButtonSelected(-1);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (debug) Debug.Log("DownArrow ");
            UpdateButtonSelected(+1);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnClick(currentButtonContainer[currentButtonIndex].name);
        }
    }

    private void OnClick(string buttonName)
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

        currentButtonContainer = _document.rootVisualElement.Q<VisualElement>(currentTabName).Q<VisualElement>("Middle").Q<VisualElement>("LeftSide");
        UpdateButtonSelected(-currentButtonIndex);

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
    
    private void UpdateButtonSelected(int buttonIndexChange = 0)
    {
        if (currentButtonContainer == null || currentButtonIndex + buttonIndexChange < 0 || currentButtonIndex + buttonIndexChange > currentButtonContainer.childCount - 1) 
            return;
        
        //currentButtonContainer[currentButtonIndex].RemoveFromClassList(".button.selected");
        currentButtonContainer[currentButtonIndex].style.color = default;

        currentButtonIndex += buttonIndexChange;
        
        currentButtonContainer[currentButtonIndex].style.color = new Color(255, 255, 255, 255);
        
        Debug.Log(currentButtonContainer[currentButtonIndex].ClassListContains(".selected"));
        
        if(debug)
            Debug.Log(currentButtonContainer[currentButtonIndex].name);
        
        UpdateButtonImage(currentButtonContainer[currentButtonIndex].name);
        
    }
}