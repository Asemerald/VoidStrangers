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
    public enum State
    {
        None = 0,
        FreeMove = 1,
        Move = 2,
        Cutscene = 4,
    }
    
    [SerializeField] private float speed = 3f;
    [SerializeField] private Sprite sprite;
    [SerializeField] private bool freeMove = true;

    // Movement
    private State _state = State.None;
    private Vector3 _moveDirection;
    private Vector2 _lookDirection;
    
    // Animation
    private SpriteResolver _spriteResolver;
    private string _category;
    private string _label;
    private float _timer;
    
    // Input
    private PlayerControls _controls;
    private readonly Dictionary<InputAction, Action<InputAction.CallbackContext>> _handlers = new();
    
    //Rigidbody
    private Rigidbody2D rb;
    
    private void Awake()
    {
        _controls = new PlayerControls();
        _spriteResolver = GetComponentInChildren<SpriteResolver>();
        rb = GetComponent<Rigidbody2D>();
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
        _state = state;
        switch (_state)
        {
            case State.Cutscene:
                StartCoroutine(Wait());
                break;
        }
    }
    
    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (!freeMove)
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
        if (freeMove)
            return;
        
        if (ctx.started)
            _moveDirection = new Vector3(direction.x, direction.y, 0);

        _state = State.Move;
    }

    private void HandleMovement()
    {
        switch (_state)
        {
            case State.FreeMove:
                var deltaPosition = _moveDirection * (speed * Time.fixedDeltaTime);
                if (LevelSetup.Instance.CanMove(transform.position + new Vector3(deltaPosition.x, deltaPosition.y, 0), _moveDirection)) 
                    rb.MovePosition(transform.position + new Vector3(deltaPosition.x, deltaPosition.y, 0));
                else
                    transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), 0);
                break;
            case State.Move:
                if(LevelSetup.Instance.CanMove(transform.position + _moveDirection, _moveDirection))
                    transform.position += _moveDirection;
                
                _state = State.None;
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
                    _timer %= 1f;
                    _label = Mathf.FloorToInt(_timer * 2f).ToString();
                }
                else
                {
                    _timer = 0f;
                    _label = "0";
                }
                break;
            case State.FreeMove:
                _timer += Time.fixedDeltaTime;
                _timer %= 0.5f;
                _label = Mathf.FloorToInt(_timer * 4f).ToString();
                _lookDirection = _moveDirection;
                break;
            case State.Move:
                _lookDirection = _moveDirection;
                break;
        }

        if (_moveDirection.x < 0)
        {
            _category = "Left";
        }
        else if (_moveDirection.x > 0)
        {
            _category = "Right";
        }
        else if (_moveDirection.y > 0)
        {
            _category = "Up";
        }
        else if (_moveDirection.y < 0)
        {
            _category = "Down";
        }
        
        _spriteResolver.SetCategoryAndLabel(_category, _label);
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (_state == State.Cutscene)
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
    
    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1f);
        _state = State.None;
    }
}
