using System;
using _00_Scripts;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class MenuEvents : MonoBehaviour
{
    public static MenuEvents Instance { get; private set; }
    
    private UIDocument _document;
    bool gameStarted = false;
    private string currentTabName = "Menu";
    private string previousTabName = "";
    private int currentButtonIndex = 0;
    private int currentPageDepth = 1 ;
    private VisualElement currentButtonContainer = null;
    
    PlayerControls controls = null;
    
    [SerializeField] private bool debug;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        
        _document = GetComponent<UIDocument>();

        foreach (var button in _document.rootVisualElement.Query<Button>().ToList())
        {
            button.RegisterCallback<ClickEvent>(evt => OnClick(button.name));
        }

        controls = new PlayerControls();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        currentButtonContainer = _document.rootVisualElement.Q<VisualElement>(currentTabName).Q<VisualElement>("Middle").Q<VisualElement>("LeftSide");
        
        OpenTab("Menu");
        GameManager.Instance.playerController.DisableActionMap();
        
        UpdateButtonSelected();
    }

    private void Update()
    {
        UpdateHUD();
    }

    private void OnEnable()
    {
        controls.Enable();
        
        controls.UI.Navigate.performed += evt =>
        {
            float y = evt.ReadValue<Vector2>().y;

            if (y > 0.5f)
                UpdateButtonSelected(-1);   
            else if (y < -0.5f)
                UpdateButtonSelected(+1);  
			
			float x = evt.ReadValue<Vector2>().x;

			if(currentButtonContainer[currentButtonIndex].name =="Master"){
			if (x > 0.5f)
                UpdateVolume(+1);   
            else if (x < -0.5f)
                 UpdateVolume(-1); }
        };
        
        controls.UI.Submit.started += evt =>
        {
            OnClick(currentButtonContainer[currentButtonIndex].name);
        };
        
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    public void OpenPauseMenu()
    {
        OnEnable();
        OpenTab("Main-Tab");
        if (gameStarted)
        {
            _document.rootVisualElement.Q("BottomSide").style.display = DisplayStyle.Flex;
        }

        _document.rootVisualElement.Q("HUD").style.display = DisplayStyle.None;
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
                if (!gameStarted)
                {
                    gameStarted = true;
                    GameManager.Instance.playerController.EnableActionMap();
                }
                break;
            
            case "QuitButton":
                Application.Quit();
                break;
            
            case "BackButton":
                if(gameStarted)
                    OpenTab("Main-Tab",-2);
                else
                {
                    OpenTab("Menu");
                }
                break;
            
            case "BackSettingsButton":
                OpenTab(previousTabName,-1);
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
            
            case "Fullscreen":
                _document.rootVisualElement.Q<VisualElement>(currentTabName).Q<VisualElement>("Middle").Q<VisualElement>("RightSide").Q<VisualElement>("FullscreenLabels").Q<Label>("FullScreenOFF").ToggleInClassList("selected");
                _document.rootVisualElement.Q<VisualElement>(currentTabName).Q<VisualElement>("Middle").Q<VisualElement>("RightSide").Q<VisualElement>("FullscreenLabels").Q<Label>("FullScreenON").ToggleInClassList("selected");
                _document.rootVisualElement.Q<VisualElement>(currentTabName).Q<VisualElement>("Middle").Q<VisualElement>("RightSide").Q<Toggle>("FullScreenToggle").value = _document.rootVisualElement.Q<VisualElement>(currentTabName).Q<VisualElement>("Middle").Q<VisualElement>("RightSide").Q<VisualElement>("FullscreenLabels").Q<Label>("FullScreenON").ClassListContains("selected");
                break;
            
            case "Resolution":
                string text = _document.rootVisualElement.Q<VisualElement>(currentTabName)
                    .Q<VisualElement>("Middle").Q<VisualElement>("RightSide")
                    .Q<Label>("ResolutionLabel")
                    .text;
                
                _document.rootVisualElement.Q<VisualElement>(currentTabName)
                    .Q<VisualElement>("Middle").Q<VisualElement>("RightSide").Q<Label>("ResolutionLabel").text = 
                    text == "1920x1080"? "1366x768" : "1920x1080";

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
        
        currentButtonContainer[currentButtonIndex].RemoveFromClassList("selected");
        currentButtonContainer = _document.rootVisualElement.Q<VisualElement>(currentTabName).Q<VisualElement>("Middle").Q<VisualElement>("LeftSide");
        UpdateButtonSelected(-currentButtonIndex);
        
        if(gameStarted)
            currentPageDepth += pageDepthUpdate;
        
        UpdatePageClass();
    }

    void UpdateButtonImage(string buttonName)
    {
        if (previousTabName != "")
        {
            _document.rootVisualElement.Q<VisualElement>(previousTabName).Q<VisualElement>("Middle").Q<VisualElement>("RightSide")[0].RemoveFromClassList("slidefade");
        }

        var rightSide = _document.rootVisualElement.Q<VisualElement>(currentTabName).Q<VisualElement>("Middle").Q<VisualElement>("RightSide")[0];
        rightSide.RemoveFromClassList("GraphicsButton");
        rightSide.RemoveFromClassList("AudioButton");
        rightSide.RemoveFromClassList("BackSettingsButton");
        rightSide.RemoveFromClassList("ResumeButton");
        rightSide.RemoveFromClassList("SettingsButton");
        rightSide.RemoveFromClassList("QuitButton");
        rightSide.RemoveFromClassList("slidefade");
        
        rightSide.MarkDirtyRepaint();

        rightSide.schedule.Execute((sched) =>
        {

            rightSide.AddToClassList(buttonName);
            rightSide.MarkDirtyRepaint();
            rightSide.AddToClassList("slidefade");
        }).StartingIn(10);

    }

	void UpdateVolume(int i){
		if((_document.rootVisualElement.Q<SliderInt>("SliderInt").value+i)>-1 && (_document.rootVisualElement.Q<SliderInt>("SliderInt").value+i)<11 ){
	_document.rootVisualElement.Q<SliderInt>("SliderInt").value += i;}
	}

    void ClosePauseMenu()
    {
        _document.rootVisualElement.Q(currentTabName).style.display = DisplayStyle.None;
        _document.rootVisualElement.Q("BottomSide").style.display = DisplayStyle.None;
        _document.rootVisualElement.Q("HUD").style.display = DisplayStyle.Flex;
        OnDisable();
        GameManager.Instance.playerController.EnableActionMap();
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

        currentButtonContainer[currentButtonIndex].RemoveFromClassList("selected");
        
        currentButtonIndex += buttonIndexChange;
        
        currentButtonContainer[currentButtonIndex].AddToClassList("selected");
        
        if(debug)
            Debug.Log(currentButtonContainer[currentButtonIndex].name);
        
        UpdateButtonImage(currentButtonContainer[currentButtonIndex].name);
        
    }

    void UpdateHUD()
    {
        VisualElement hud = _document.rootVisualElement.Q<VisualElement>("HUD");
        
        hud.Q<Label>("LVL-Amount").text = "B0"+(LevelSetup.Instance.CurrentLevelIndex+1).ToString("D2");

        if (PlayerData.Instance.bugAmount >= 0)
        {
            hud.Q<Label>("BEE-Amount").text = PlayerData.Instance.bugAmount.ToString("D2");
            hud.Q<Label>("HP-Amount").text  = "HP"+PlayerData.Instance.healthPoints.ToString("D2");
        }
        else
        {
            hud.Q<Label>("BEE-Amount").text ="00";
            hud.Q<Label>("HP-Amount").text  = "VOID";
        }

        if (PlayerData.Instance.hasScepter && !hud.Q<VisualElement>("Sceptre").ClassListContains("sceptre"))
        {
            hud.Q<VisualElement>("Sceptre").AddToClassList("sceptre");
            Debug.Log(hud.Q<VisualElement>("Sceptre").ClassListContains("sceptre"));
        }
        else
        {
            if (PlayerData.Instance.pickedUpTile)
            {
                
                if (PlayerData.Instance.pickedUpTile.name == "spr_stairs_0")
                {
                    hud.Q<VisualElement>("Sceptre").AddToClassList("sceptreStairs");
                }
                else
                {
                    hud.Q<VisualElement>("Sceptre").AddToClassList("sceptreGround");
                }
            }
            else
            {
                hud.Q<VisualElement>("Sceptre").RemoveFromClassList("sceptreStairs");
                hud.Q<VisualElement>("Sceptre").RemoveFromClassList("sceptreGround");
            }
        }
        
        
        
    }
}