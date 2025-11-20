using System;
using System.Collections;
using System.Collections.Generic;
using _00_Scripts;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.U2D.Animation;

[RequireComponent(typeof(PlayerData))]
public class PlayerController : MonoBehaviour
{
    [Flags]
    public enum State
    {
        None = 0,
        FreeMove = 1,
        Move = 2,
        Cutscene = 4,
        Blink = 8,
        OpenChest = 16,
        Edging = 32,
        Falling = 64,
    }
    
    [SerializeField] private float speed = 3f;
    [SerializeField] private Sprite sprite;
    [SerializeField] private bool freeMove = true;

    // Movement
    private State _state = State.Cutscene | State.Blink;
    private Vector3 _moveDirection;
    private Vector2 _lookDirection;
    private Vector3 _previousPosition;
    
    // Animation
    private SpriteResolver _spriteResolver;
    private string _category;
    private string _label;
    private float _timer;
    
    // Input
    private PlayerControls _controls;
    private readonly Dictionary<InputAction, Action<InputAction.CallbackContext>> _handlers = new();
    
    private void Awake()
    {
        _controls = new PlayerControls();
        _spriteResolver = GetComponentInChildren<SpriteResolver>();
    }

    private void OnEnable()
    {
        EnableActionMap();
    }

    private void OnDisable()
    {
        DisableActionMap();
    }

    private void FixedUpdate()
    {
        HandleSpriteChange();
        HandleMovement();
    }

    public void EnableActionMap()
    {
        _controls.Enable();
        
        _controls.Player.Move.started += OnMove;
        _controls.Player.Move.performed += OnMove;
        _controls.Player.Move.canceled += OnMove;

        _controls.Player.Interact.started += OnInteract;
        
        AddHandler(_controls.Player.MoveLeft, Vector2.left);
        AddHandler(_controls.Player.MoveRight, Vector2.right);
        AddHandler(_controls.Player.MoveUp, Vector2.up);
        AddHandler(_controls.Player.MoveDown, Vector2.down);
    }
    
    private void AddHandler(InputAction action, Vector2 dir)
    {
        var handler = new Action<InputAction.CallbackContext>(ctx => OnMoveTile(ctx, dir));
        _handlers[action] = handler;
        action.started += handler;
    }

    public void DisableActionMap()
    {
        _controls.Player.Move.started -= OnMove;
        _controls.Player.Move.performed -= OnMove;
        _controls.Player.Move.canceled -= OnMove;
        
        _controls.Player.Interact.started -= OnInteract;
        
        foreach (var kvp in _handlers)
            kvp.Key.started -= kvp.Value;
        
        _controls.Disable();
    }

    public void DisableFreeMove()
    {
        freeMove = false;
        transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), 0);
    }

    public void SetState(State state)
    {
        _timer = 0f;
        _state = state;
        switch (_state)
        {
            case State.OpenChest:
            case State.Blink:
            case State.Falling:
                _state |= State.Cutscene;
                break;
            case State.Edging:
                _previousPosition = transform.position;
                break;
        }
    }
    
    private bool HasState(State flag)
    {
        return (_state & flag) != 0;
    }
    
    private string GetCurrentFlagName(int index)
    {
        var stateNameArray = _state.ToString().Split(',');
        return index > stateNameArray.Length - 1 ? stateNameArray[^1].Trim() : stateNameArray[index].Trim();
    }
    
    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (!freeMove || HasState(State.Cutscene))
            return;
        
        var input = ctx.ReadValue<Vector2>();
        _state = State.FreeMove;

        if (ctx.performed)
        {
            _moveDirection = input;
        }

        if (ctx.canceled)
        {
            _state = State.None;
            _moveDirection = Vector2.zero;
        }
    }

    private void OnMoveTile(InputAction.CallbackContext ctx, Vector2 direction)
    {
        if (freeMove || HasState(State.Cutscene))
            return;

        if (ctx.started)
        {
            if (!HasState(State.Edging))
            {
                _moveDirection = new Vector3(direction.x, direction.y, 0);
            }
            else
            {
                if (_lookDirection + direction == Vector2.zero)
                {
                    transform.position = _previousPosition;
                    SetState(State.None);
                }
                return;
            }
        }

        _state = State.Move;
    }

    private void HandleMovement()
    {
        switch (_state)
        {
            case State.FreeMove:
                var deltaPosition = _moveDirection * (speed * Time.fixedDeltaTime);
                if (LevelSetup.Instance.CanMove(this, transform.position + new Vector3(deltaPosition.x, deltaPosition.y, 0))) 
                    transform.position += new Vector3(deltaPosition.x, deltaPosition.y, 0);
                else
                    transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), 0);
                break;
            case State.Move:
                if(LevelSetup.Instance.CanMove(this, transform.position + _moveDirection))
                    transform.position += _moveDirection;
                if (!HasState(State.Edging))
                    _state = State.None;
                break;
            case State.Edging:
                transform.position += _moveDirection * (0.666666f * Time.fixedDeltaTime);
                break;
        }
    }

    private void HandleSpriteChange()
    {
        switch (_state)
        {
            case State.None:
                if (!freeMove)
                {
                    _timer += Time.fixedDeltaTime;
                    _label = GetFrameLabel(2f, 2f);
                }
                else
                {
                    _timer = 0f;
                    _label = "0";
                }
                break;
            case State.FreeMove:
                _timer += Time.fixedDeltaTime;
                _label = GetFrameLabel(4f, 2f);
                _lookDirection = _moveDirection;
                break;
            case State.Move:
                _lookDirection = _moveDirection;
                break;
            case State.OpenChest | State.Cutscene:
                _timer += Time.fixedDeltaTime;
                _label = "0";
                if (_timer > 1.5f)
                {
                    _state = State.None;
                    _lookDirection = Vector2.down;
                }
                break;
            case State.Blink | State.Cutscene:
                _timer += Time.fixedDeltaTime;
                _label = GetFrameLabel(8f, 4f);
                if (_timer > 1f)
                {
                    _state = State.None;
                    _lookDirection = Vector2.down;
                }
                break;
            case State.Edging:
                _timer += Time.fixedDeltaTime;
                _label = GetFrameLabel(16f, 2f);
                if (_timer > 1.5f)
                {
                    SetState(State.Falling);
                }
                break;
            case State.Falling | State.Cutscene:
                _timer += Time.fixedDeltaTime;
                _label = GetFrameLabel(6f, 12f);
                if (_timer > 2f)
                {
                    var level = LevelSetup.Instance.ReloadLevel();
                    PlayerData.Instance.ResetPickedUpTile();
                    PlayerData.Instance.SubtractBugAmount(1);
                    if (level == 1)
                    {
                        PlayerData.Instance.SetHasScepter(false);
                        freeMove = true;
                    }
                    _state = State.Blink | State.Cutscene;
                }
                break;
        }

        if (!HasState(State.Cutscene))
        {
            if (_lookDirection.x < 0)
            {
                _category = "Left";
            }
            else if (_lookDirection.x > 0)
            {
                _category = "Right";
            }
            else if (_lookDirection.y > 0)
            {
                _category = "Up";
            }
            else if (_lookDirection.y < 0)
            {
                _category = "Down";
            }
        }
        else
        {
            _category = GetCurrentFlagName(1);
        }
        
        _spriteResolver.SetCategoryAndLabel(_category, _label);
    }

    private string GetFrameLabel(float animSpeed, float frames)
    {
        return Mathf.FloorToInt((_timer * animSpeed) % frames).ToString();
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (HasState(State.Cutscene) || HasState(State.Edging))
            return;
        
        var objects = FindObjectsByType<Collider2D>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        var position = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        foreach (var obj in objects)
        {
            Vector2 objPosition = obj.transform.position;
            Vector2 objSize = obj.bounds.size;
            for (var i = 0; i < objSize.x; i++)
                if (objPosition + Vector2.right * i == position + _lookDirection)
                    if (obj.TryGetComponent(out Interactable interactable))
                    {
                        interactable.Interact(this, _lookDirection);
                        return;
                    }
        }
        
        if (PlayerData.Instance.hasScepter)
            LevelSetup.Instance.Interact(this, _lookDirection);
    }
}
