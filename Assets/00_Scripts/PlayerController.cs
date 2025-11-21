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
        Attack = 128,
    }
    
    [SerializeField] private float speed = 3f;
    [SerializeField] private Sprite sprite;
    [SerializeField] private SpriteResolver fxPrefab;
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
    private Dictionary<string, SpriteResolver> _fx;
    private bool _rodMoved;
    
    // Input
    private PlayerControls _controls;
    private readonly Dictionary<InputAction, Action<InputAction.CallbackContext>> _handlers = new();
    
    //Rigidbody
    private Rigidbody2D _rb;
    
    private void Awake()
    {
        _controls = new PlayerControls();
        _spriteResolver = GetComponentInChildren<SpriteResolver>();
        _rb = GetComponent<Rigidbody2D>();
        _fx = new Dictionary<string, SpriteResolver>();
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
        _controls.Player.Pause.started += OnPause;
        
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
        _controls.Player.Pause.started -= OnPause;
        
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
                _fx.Add("Sweat", Instantiate(fxPrefab, transform.position + new Vector3(0.5f, 0.5f, 0f), Quaternion.identity, transform));
                break;
            case State.Attack:
                _state |= State.Cutscene;
                Vector3 v3LookDirection = _lookDirection;
                _fx.Add("Rod", Instantiate(fxPrefab, transform.position + v3LookDirection, Quaternion.identity, transform));
                _fx["Rod"].GetComponent<SpriteRenderer>().sortingOrder = 4;
                var swipeAngle = 0f;
                var swipeMirrored = false;
                var swipeShift = new Vector3(0.5f, 0.5f, 0f);
                if (_lookDirection.y > 0)
                {
                    swipeAngle = 180f;
                    swipeShift.x = 0f;
                }
                else if (_lookDirection.y < 0)
                {
                    swipeShift.x = 1f;
                }
                else if (_lookDirection.x < 0)
                {
                    swipeAngle = -90f;
                    swipeShift.y = 0f;
                }
                else if (_lookDirection.x > 0)
                {
                    swipeAngle = -90f;
                    swipeShift.y = 0f;
                    swipeMirrored = true;
                }
                var swipeRotation = new Vector3(0f, 0f, swipeAngle);
                _fx.Add("Swipe", Instantiate(fxPrefab, transform.position + swipeShift, Quaternion.Euler(swipeRotation), transform));
                var swipeRenderer = _fx["Swipe"].GetComponent<SpriteRenderer>();
                swipeRenderer.sortingOrder = 1;
                if (swipeMirrored)
                    swipeRenderer.flipY = true;
                _fx.Add("Sparkle", Instantiate(fxPrefab, transform.position + v3LookDirection, Quaternion.identity, transform));
                _fx["Sparkle"].GetComponent<SpriteRenderer>().sortingOrder = 5;
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
                    ClearFX();
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
                LevelSetup.Instance.CanMove(this, transform.position + new Vector3(deltaPosition.x, deltaPosition.y, 0),
                    _moveDirection, ref deltaPosition);
                _rb.MovePosition(transform.position + new Vector3(deltaPosition.x, deltaPosition.y, 0));
                break;
            case State.Move:
                var zeroPosition = Vector3.zero;
                if (LevelSetup.Instance.CanMove(this, transform.position + _moveDirection, _moveDirection, ref zeroPosition))
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
                // Player
                _timer += Time.fixedDeltaTime;
                _label = GetFrameLabel(16f, 2f);
                if (_timer > 1.5f)
                {
                    SetState(State.Falling);
                    ClearFX();
                    return;
                }
                // FX
                var fxLabel = GetFrameLabel(15f, 5f);
                _fx["Sweat"].SetCategoryAndLabel("Sweat", fxLabel);
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
            case State.Attack | State.Cutscene:
                // Player
                _timer += Time.fixedDeltaTime;
                _label = GetFrameLabel(8f, 2f);
                if (_label == "1" && !_rodMoved)
                {
                    _rodMoved = true;
                    _fx["Rod"].transform.position += new Vector3(_lookDirection.x / 16f, _lookDirection.y / 16f, 0f);
                }
                if (_label == "0" && _rodMoved)
                {
                    _rodMoved = false;
                    _fx["Rod"].transform.position -= new Vector3(_lookDirection.x / 16f, _lookDirection.y / 16f, 0f);
                }
                if (_timer > 0.375f)
                {
                    _state = State.None;
                    ClearFX();
                    _rodMoved = false;
                    return;
                }
                // FX
                var swipeLabel = GetFrameLabel(20f, 10f);
                var sparkleLabel = GetFrameLabel(16f, 8f);
                _fx["Swipe"].SetCategoryAndLabel("Swipe", swipeLabel);
                _fx["Sparkle"].SetCategoryAndLabel("Sparkle", sparkleLabel);
                break;
        }

        if (!HasState(State.Cutscene) || HasState(State.Attack))
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

        if (HasState(State.Attack))
        {
            _label = "a" + _label;
            _fx["Rod"].SetCategoryAndLabel("Rod", _category);
        }
        
        _spriteResolver.SetCategoryAndLabel(_category, _label);
    }

    private string GetFrameLabel(float animSpeed, float frames)
    {
        return Mathf.FloorToInt((_timer * animSpeed) % frames).ToString();
    }

    private void ClearFX()
    {
        foreach (var fx in _fx)
            Destroy(fx.Value.gameObject);
        _fx.Clear();
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
        {
            if (LevelSetup.Instance.Interact(this, _lookDirection))
                SetState(State.Attack);
        }
    }

    private void OnPause(InputAction.CallbackContext ctx)
    {
        MenuEvents.Instance.OpenPauseMenu();
        DisableActionMap();
    }
}
