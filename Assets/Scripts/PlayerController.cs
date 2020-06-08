using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    private float _walkSpeed;

    [SerializeField]
    private float _jumpSpeed;

    public bool _idle = true;
    public Animator _animator;

    private Rigidbody2D _rb;
    private Vector2 _movementVector = new Vector2(0, 0);

    private PlayerInputActions _playerInputActions;

    private void Awake()
    {
        _animator = this.GetComponent<Animator>();
        _playerInputActions = new PlayerInputActions();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        _playerInputActions.Enable();
    }

    private void OnDisable()
    {
        _playerInputActions.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        _walkSpeed = 5.0f;
        _jumpSpeed = 5.0f;
        _playerInputActions.Land.Jump.performed += _ => Jump();
    }

    // Update is called once per frame
    void Update()
    {
        //Move();
    }

    void FixedUpdate()
    {
        Move();
    }

    private void Jump()
    {

        Debug.Log("JUMPED");

        //if(IsGrounded())
        //{
            _rb.AddForce(new Vector2(0, _jumpSpeed), ForceMode2D.Impulse);
        //}

    }

    private bool IsGrounded()
    {
        Debug.Log("IsGrounded" + _rb.transform.position.z);
        return _rb.transform.position.z == 0;
    }


    private void Move()
    {
        _rb.velocity = _playerInputActions.Land.Movement.ReadValue<Vector2>() * _walkSpeed;

        if (_rb.velocity == null)
            return;

        Debug.Log(_rb.velocity.x + ", " + _rb.velocity.y);

        // Animation
        if (_rb.velocity.x == 0 && _rb.velocity.y < 0)
        {
            _animator.SetBool("PlayerIdle", false);
            _animator.SetBool("PlayerDown", true);
            _animator.SetBool("PlayerUp", false);
            _animator.SetBool("PlayerRight", false);
            _animator.SetBool("PlayerLeft", false);
        }
        else if (_rb.velocity.x == 0 && _rb.velocity.y == 0)        
        {
            _animator.SetBool("PlayerIdle", true);
            _animator.SetBool("PlayerDown", false);
            _animator.SetBool("PlayerUp", false);
            _animator.SetBool("PlayerRight", false);
            _animator.SetBool("PlayerLeft", false);
        }

    }

}
