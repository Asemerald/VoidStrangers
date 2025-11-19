using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private enum State
    {
        None = 0,
        FreeMove = 1,
        Move = 2,
    }
    
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private Sprite sprite;

    private State _state = State.None;
    [SerializeField] private bool _freeMove = true;

    private Vector2 _moveDirection;
    private Vector3 _targetPosition;
    
    private PlayerControls _controls;

    private readonly Dictionary<InputAction, Action<InputAction.CallbackContext>> _handlers = new();
    
    private void Awake()
    {
        _controls = new PlayerControls();
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
        switch (_state)
        {
            case State.FreeMove:
                var deltaPosition = _moveDirection * (speed * Time.fixedDeltaTime);
                transform.position += new Vector3(deltaPosition.x, deltaPosition.y, 0);
                break;
            case State.Move:
                break;
        }
    }

    public void EnableActionMap()
    {
        _controls.Enable();
        
        _controls.Player.Move.started += OnMove;
        _controls.Player.Move.performed += OnMove;
        _controls.Player.Move.canceled += OnMove;
        
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
        
        foreach (var kvp in _handlers)
            kvp.Key.started -= kvp.Value;
        
        _controls.Disable();
    }
    
    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (!_freeMove)
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
        if (_freeMove)
            return;
        
        if (ctx.started)
            transform.position += new Vector3(direction.x, direction.y, 0) * sprite.bounds.size.x;
    }
}
