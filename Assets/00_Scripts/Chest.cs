using UnityEngine;
using UnityEngine.U2D.Animation;

[ExecuteAlways]
public class Chest : Interactable
{
    private enum State
    {
        Closed = 0,
        Open = 1,
    }

    private enum Type
    {
        Chest = 0,
        ChestRegular = 1,
        ChestSmall = 2,
    }

    [SerializeField] private Type chestCategory;
    
    // Movement
    private State _state = State.Closed;
    
    // Animation
    private SpriteResolver _spriteResolver;
    private string _category;
    private string _label;
    
    private void Awake()
    {
        Init();
    }
    
    private void OnValidate()
    {
        Init();
    }

    private void Init()
    {
        _spriteResolver = GetComponentInChildren<SpriteResolver>();
        _category = chestCategory.ToString();
        _label = "0";
        UpdateSprite();
    }

    public override void Interact()
    {
        _state = State.Open;
        _label = ((int)_state).ToString();
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        _spriteResolver.SetCategoryAndLabel(_category, _label);
    }
}
