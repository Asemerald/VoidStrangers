using System;
using System.Collections;
using _00_Scripts;
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

    private enum Item
    {
        None = 0,
        Rod = 1,
        Bug = 2,
    }

    [SerializeField] private Type chestCategory;
    [SerializeField] private Item itemInside;
    [SerializeField] private GameObject[] items;

    private Transform _animatedItem;
    
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

    public override void Interact(PlayerController player, Vector2 direction)
    {
        if (direction != Vector2.up)
            return;
        
        _state = State.Open;
        _label = ((int)_state).ToString();
        UpdateSprite();
        ObtainItem(player);
    }

    private void ObtainItem(PlayerController player)
    {
        switch (itemInside)
        {
            case Item.None:
                Debug.Log("No item inside");
                break;
            case Item.Rod:
                player.SetState(PlayerController.State.Cutscene);
                player.DisableFreeMove();
                _animatedItem = Instantiate(items[(int)itemInside], transform).transform;
                StartCoroutine(ItemAnimation());
                itemInside = Item.None;
                PlayerData.Instance.SetHasScepter(true);
                break;
            case Item.Bug:
                player.SetState(PlayerController.State.Cutscene);
                _animatedItem = Instantiate(items[(int)itemInside], transform).transform;
                StartCoroutine(ItemAnimation());
                itemInside = Item.None;
                PlayerData.Instance.AddBugAmount(1);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private IEnumerator ItemAnimation() {
        _animatedItem.localPosition = new Vector3(0, 0, -1);
        var timer = 1f;
        while (timer > 0f)
        {
            _animatedItem.position += Vector3.up * Time.deltaTime;
            timer -= Time.deltaTime;
            yield return null;
        }
    }

    private void UpdateSprite()
    {
        _spriteResolver.SetCategoryAndLabel(_category, _label);
    }
}
