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

    [SerializeField]
    private TilemapController tilemapController;

    public bool _idle = true;
    public Animator _animator;

    private Rigidbody2D _rb;
    private Vector2 _waywardPoint;

    private PlayerInputActions _playerInputActions;

    private bool _waywardMovement = false;

    private Vector2 _waywardVelocity = new Vector2(0, 0);

    private const string DIRECTION_IDLE = "Idle";
    private const string DIRECTION_NORTH = "North";
    private const string DIRECTION_NORTH_EAST = "NorthEast";
    private const string DIRECTION_EAST = "East";
    private const string DIRECTION_SOUTH_EAST = "SouthEast";
    private const string DIRECTION_SOUTH = "South";
    private const string DIRECTION_SOUTH_WEST = "SouthWest";
    private const string DIRECTION_WEST = "West";
    private const string DIRECTION_NORTH_WEST = "NorthWest";

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
        _playerInputActions.Enable();
    }

    /***************************************************************
     * LIFECYCLE
     */
    private void OnDisable()
    {
        _playerInputActions.Disable();
    }

    /***************************************************************
     * LIFECYCLE
     */
    void Start()
    {

        Debug.Log("tilemapController found: " + tilemapController.name);

        _walkSpeed = 2.0f;
        _playerInputActions.Player.WaywardAction.performed += 
            _ => WaywardAction();

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
                return;

            //Are we in the direct line of the pointer location?
            //if so, change the velocity to be a straight line
            } else if (wx == px) {
                _waywardVelocity.x = 0;  //Stop moving in the X axis
                _waywardVelocity.y = (_waywardVelocity.y == 0 ? 0 : _waywardVelocity.y > 0 ? 1 : -1) * _walkSpeed;
                _rb.transform.position = new Vector2(_waywardPoint.x, _rb.position.y);
                Debug.Log("X done:" + _waywardVelocity.y);
            } else if (wy == py)
            {
                _waywardVelocity.y = 0; //Stop moving in the Y axis
                _rb.position = new Vector2(_rb.position.x, _waywardPoint.y);
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
        //AnimatePlayer();

    }

    /***************************************************************
     * Sets the Animation
     */
    private void AnimatePlayer()
    {

        //Idle
        if (_rb.velocity.x == 0 && _rb.velocity.y == 0)
        {
            _animator.Play(PlayerController.DIRECTION_IDLE);
        }

        // Up (North)
        else if (_rb.velocity.x == 0 && _rb.velocity.y > 0)
        {
            _animator.Play(PlayerController.DIRECTION_NORTH);
        }

        // Down (South)
        else if (_rb.velocity.x == 0 && _rb.velocity.y < 0)
        {
            _animator.Play(PlayerController.DIRECTION_SOUTH);
        }

        // Right (East)
        else if (_rb.velocity.x < 0 && _rb.velocity.y == 0)
        {
            _animator.Play(PlayerController.DIRECTION_EAST);
        }

        // Right (West)
        else if (_rb.velocity.x > 0 && _rb.velocity.y == 0)
        {
            _animator.Play(PlayerController.DIRECTION_WEST);
        }

        // Up to the Right (North East)
        else if (_rb.velocity.x > 0 && _rb.velocity.y > 0)
        {
            _animator.Play(PlayerController.DIRECTION_NORTH_EAST);
        }

        // Up to the Left (North West)
        else if (_rb.velocity.x < 0 && _rb.velocity.y > 0)
        {
            _animator.Play(PlayerController.DIRECTION_NORTH_WEST);
        }

        // Down to the Right (South East)
        else if (_rb.velocity.x > 0 && _rb.velocity.y < 0)
        {
            _animator.Play(PlayerController.DIRECTION_SOUTH_EAST);
        }

        // Down to the Left (South West)
        else if (_rb.velocity.x < 0 && _rb.velocity.y < 0)
        {
            _animator.Play(PlayerController.DIRECTION_SOUTH_WEST);
        }

    }

}
