using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    private float _walkSpeed;

    [SerializeField]
    private float _jumpSpeed;

    [SerializeField]
    private TilemapController tilemapController;

    [SerializeField]
    private ParticleSystem _wayPointParticles;

    public bool _idle = true;
    public Animator _animator;

    private Rigidbody2D _rb;
    private Vector2 _waywardPoint;

    private PlayerInputActions _playerInputActions;

    private bool _waywardMovement = false;

    private Vector2 _waywardVelocity = new Vector2(0, 0);

    private const string DIRECTION_IDLE_SW = "Player_IDLE_SW";
    private const string DIRECTION_IDLE_SE = "Player_IDLE_SE";
    private const string DIRECTION_IDLE_NE = "Player_IDLE_SE";
    private const string DIRECTION_IDLE_NW = "Player_IDLE_SE";
    private const string DIRECTION_IDLE_E = "Player_IDLE_SE";
    private const string DIRECTION_IDLE_W = "Player_IDLE_SE";
    private const string DIRECTION_IDLE_N = "Player_IDLE_SE";
    private const string DIRECTION_IDLE_S = "Player_IDLE_SE";

    private const string DIRECTION_NORTH = "Player_N";
    private const string DIRECTION_NORTH_EAST = "Player_NE";
    private const string DIRECTION_EAST = "Player_E";
    private const string DIRECTION_SOUTH_EAST = "Player_SE";
    private const string DIRECTION_SOUTH = "Player_S";
    private const string DIRECTION_SOUTH_WEST = "Player_SW";
    private const string DIRECTION_WEST = "Player_W";
    private const string DIRECTION_NORTH_WEST = "Player_NW";

    private string play_idle_direction = DIRECTION_IDLE_SW;

    private List<Vector2[]> debugPoints = new List<Vector2[]>();

    /***************************************************************
     * LIFECYCLE: https://docs.unity3d.com/Manual/ExecutionOrder.html
     * 
     */
    private void Awake()
    {
        _animator = this.GetComponent<Animator>();
        _playerInputActions = new PlayerInputActions();
        _rb = GetComponent<Rigidbody2D>();
    }

    /***************************************************************
     * LIFECYCLE
     */
    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        _playerInputActions.Enable();
    }

    /***************************************************************
     * LIFECYCLE
     */
    private void OnDisable()
    {
        _playerInputActions.Disable();
        EnhancedTouchSupport.Disable();
    }

    /***************************************************************
     * LIFECYCLE
     */
    void Start()
    {

        Debug.Log("tilemapController found: " + tilemapController.name);

        _walkSpeed = 2.0f;

        //Register the PlayerInputAction to a method
        _playerInputActions.Player.WaywardAction.performed += 
            _ => WaywardAction();

        _playerInputActions.UI.Zoom.performed += ctx => Pinch(ctx);

        _playerInputActions.UI.Pinch.performed += ctx => PinchTouch(ctx);

        //set to be the same as the player's initial position.
        _waywardPoint = tilemapController.GetTileMidPoint(_rb.position);
        _rb.position = _waywardPoint;
    }

    /***************************************************************
     * LIFECYCLE
     */
    void Update()
    {
        if(debugPoints.Count > 0)
        {
            foreach (Vector2[] points in debugPoints)
                Debug.DrawLine(points[0], points[1]);
        }

    }

    /***************************************************************
     * LIFECYCLE
     * 
     * MonoBehaviour.FixedUpdate has the frequency of the physics 
     * system; it is called every fixed frame-rate frame.
     * 
     * .02 seconds (50 calls per second) is the default time 
     * between calls.
     * 
     */
    void FixedUpdate()
    {
        MoveViaWayward();
    }

    /***************************************************************
     * Check if we have a Click or Tab event to start a Player's 
     * Wayward Velocity
     */
    private void WaywardAction()
    {
        //Stop any current motion
        _waywardMovement = false;

        //Get and Convert the pointer location to a Woild Point
        Vector2 point = Camera.main.ScreenToWorldPoint(
            _playerInputActions.Player.WaywardVector.ReadValue<Vector2>());

        Debug.Log("START: " + _rb.position.ToString());
        Vector2 midTileTarget = tilemapController.GetTileMidPoint(point);

        //Off Tilemap click
        if (midTileTarget.x == Vector2.negativeInfinity.x && midTileTarget.y == Vector2.negativeInfinity.y)
        {
            Debug.Log("Not valid tile");
            _waywardVelocity = Vector2.zero;
            _waywardMovement = false;
            return;
        }

        _waywardPoint.x = midTileTarget.x;
        _waywardPoint.y = midTileTarget.y;

        _wayPointParticles.transform.position = new Vector3(_waywardPoint.x, _waywardPoint.y, 300);

        if (_wayPointParticles.isPlaying)
            _wayPointParticles.Stop();

        _wayPointParticles.Play();

        //We want units of 1 ie: (1,1) or (-1,1) or (1,-1)
        _waywardVelocity = Vector2.zero;
        _waywardVelocity.x = _waywardPoint.x - _rb.position.x == 0 ? 0 : _waywardPoint.x - _rb.position.x > 0 ? 2f : -2f;
        _waywardVelocity.y = _waywardPoint.y - _rb.position.y == 0 ? 0 : _waywardPoint.y - _rb.position.y > 0 ? 1 : -1;
        _waywardVelocity.Normalize();
        _waywardVelocity = _waywardVelocity * _walkSpeed;

        Debug.Log("Wayward Velocity: " + _waywardVelocity.x + ", " + _waywardVelocity.y);

        //Tell the Move loop its ok to move
        _waywardMovement = true;

    }

    /***************************************************************
     * Future JUMP support
     */
    private bool IsGrounded()
    {
        Debug.Log("IsGrounded" + _rb.transform.position.z);
        return _rb.transform.position.z == 0;
    }

    /***************************************************************
     * Two point wayward action.
     * 
     * Walk in one of the 8 isometric directions until player's x 
     * or y is same.  Then walk straight to target.
     */
    private void MoveViaWayward()
    {

        //Did the user click or tab?
        if(_waywardMovement)
        {

            //TODO:  Stop on collisions


            //figure out a close enough (x or y), exact point 
            //probably won't ever match..
            //
            //Suppose we could do a better job detecting the exact 
            //isometric tile and getting its mid-point....
            double wx = System.Math.Round(_waywardPoint.x, 1);
            double wy = System.Math.Round(_waywardPoint.y, 1);
            double px = System.Math.Round(_rb.position.x, 1);
            double py = System.Math.Round(_rb.position.y, 1);

            //Already where we need to be, no movement required
            if (wx == px && wy == py)
            {
                _rb.velocity = Vector2.zero; //stop moving
                _rb.transform.position = _waywardPoint; //update position to be exact
                Debug.Log("STOP: " + _rb.position.ToString());
                _waywardMovement = false;

            //Are we in the direct line of the pointer location?
            //if so, change the velocity to be a straight line
            } else if (wx == px) {
                _waywardVelocity.x = 0;  //Stop moving in the X axis
                _waywardVelocity.y = (_waywardVelocity.y == 0 ? 0 : _waywardVelocity.y > 0 ? 1 : -1) * _walkSpeed;
                _rb.transform.position = new Vector2(_waywardPoint.x, _rb.position.y); ////reposition the sprite

            } else if (wy == py)
            {
                _waywardVelocity.y = 0; //Stop moving in the Y axis
                _rb.transform.position = new Vector2(_rb.position.x, _waywardPoint.y); //reposition the sprite
            }

            Vector2[] pair = new Vector2[2];
            pair[0] = _rb.position;
            pair[1] = _waywardPoint;
            debugPoints.Add(pair);

            _rb.velocity = _waywardVelocity;
        } else
        {
            _rb.velocity = Vector2.zero;
        }

        // Animation
        AnimatePlayer();

    }

    private void PinchTouch(InputAction.CallbackContext ctx)
    {
        Debug.Log(UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count);
    }

    private void Pinch(InputAction.CallbackContext ctx)
    {
        Debug.Log("called");
        Vector2 value = ctx.ReadValue<Vector2>();
        Debug.Log(value);

        Camera cam = Camera.main;

        float size = cam.orthographicSize;

        if (value.y > 0 && size > 1)
        {
            cam.orthographicSize -= .1f;
            Debug.Log("UP");

        } else if (value.y < 0 && size < 3)
        {
            cam.orthographicSize += .1f;
            Debug.Log("DOWN");
        }

    }

    /***************************************************************
     * Sets the Animation
     */
    private void AnimatePlayer()
    {

        //Idle
        if (_rb.velocity.x == 0 && _rb.velocity.y == 0
            && !_animator.GetCurrentAnimatorStateInfo(0).IsName(play_idle_direction))
        {
            Debug.Log("Play IDLE: " + play_idle_direction);
            _animator.Play(play_idle_direction);
        }

        // Up (North)
        else if (_rb.velocity.x == 0 && _rb.velocity.y > 0 
            && !_animator.GetCurrentAnimatorStateInfo(0).IsName(PlayerController.DIRECTION_NORTH))
        {
            Debug.Log("Play N");
            _animator.Play(PlayerController.DIRECTION_NORTH);
            play_idle_direction = PlayerController.DIRECTION_IDLE_SE;
        }

        // Down (South)
        else if (_rb.velocity.x == 0 && _rb.velocity.y < 0
            && !_animator.GetCurrentAnimatorStateInfo(0).IsName(PlayerController.DIRECTION_SOUTH))
        {
            Debug.Log("Play S");
            _animator.Play(PlayerController.DIRECTION_SOUTH);
            play_idle_direction = PlayerController.DIRECTION_IDLE_SE;
        }

        // Left (West)
        else if (_rb.velocity.x < 0 && _rb.velocity.y == 0
            && !_animator.GetCurrentAnimatorStateInfo(0).IsName(PlayerController.DIRECTION_WEST))
        {
            Debug.Log("Play W: " + _rb.velocity.ToString());
            _animator.Play(PlayerController.DIRECTION_WEST);
            play_idle_direction = PlayerController.DIRECTION_IDLE_SW;
        }

        // Right (East)
        else if (_rb.velocity.x > 0 && _rb.velocity.y == 0 && (_waywardPoint.x + 1) >_rb.position.x
            && !_animator.GetCurrentAnimatorStateInfo(0).IsName(PlayerController.DIRECTION_EAST))
        {
            Debug.Log("Play E: " + _rb.velocity.ToString());
            _animator.Play(PlayerController.DIRECTION_EAST);
            play_idle_direction = PlayerController.DIRECTION_IDLE_SE;
        }

        // Up to the Right (North East)
        else if (_rb.velocity.x > 0 && _rb.velocity.y > 0
            && !_animator.GetCurrentAnimatorStateInfo(0).IsName(PlayerController.DIRECTION_NORTH_EAST))
        {
            Debug.Log("Play NE");
            _animator.Play(PlayerController.DIRECTION_NORTH_EAST);
            play_idle_direction = PlayerController.DIRECTION_IDLE_SE;
        }

        // Up to the Left (North West)
        else if (_rb.velocity.x < 0 && _rb.velocity.y > 0
            && !_animator.GetCurrentAnimatorStateInfo(0).IsName(PlayerController.DIRECTION_NORTH_WEST))
        {
            Debug.Log("Play NW");
            _animator.Play(PlayerController.DIRECTION_NORTH_WEST);
            play_idle_direction = PlayerController.DIRECTION_IDLE_SW;
        }

        // Down to the Right (South East)
        else if (_rb.velocity.x > 0 && _rb.velocity.y < 0
            && !_animator.GetCurrentAnimatorStateInfo(0).IsName(PlayerController.DIRECTION_SOUTH_EAST))
        {
            Debug.Log("Play SE");
            _animator.Play(PlayerController.DIRECTION_SOUTH_EAST);
            play_idle_direction = PlayerController.DIRECTION_IDLE_SE;
        }

        // Down to the Left (South West)
        else if (_rb.velocity.x < 0 && _rb.velocity.y < 0
            && !_animator.GetCurrentAnimatorStateInfo(0).IsName(PlayerController.DIRECTION_SOUTH_WEST))
        {
            Debug.Log("Play SW");
            _animator.Play(PlayerController.DIRECTION_SOUTH_WEST);
            play_idle_direction = PlayerController.DIRECTION_IDLE_SW;
        }

    }

}
