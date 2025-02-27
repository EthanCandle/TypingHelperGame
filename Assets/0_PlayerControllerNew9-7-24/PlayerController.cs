using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    public bool hasControl = true;

    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 7.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 20.0f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 2.5f;
    public float jumpTimeHold = 0, jumpTimeHoldReset = 0.35f;

    public float coyoteJumpTime = 0, coyoteJumpTimeReset = 0.15f;
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -35.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.0f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;
    [Tooltip("The character uses its own gravity value for slopes. The engine default is -9.81f")]
    public float GravityOfSloper = -45.0f;
    public bool groundedOnceCheck = false;


    public bool isGoingToBeOffGround = false; // used to make falling off ledges more smooth without causing a dip

    public float timeToHoldJumpBufferCurrent = 0;
    public float timeToHoldJumpBufferMax = 0.25f;
    public bool jumpBufferIsOn = false;

    public float slideFactor = 0.5f; // Sliding intensity
    public float slideThreshold = 0.7f; // Controls when to start sliding, 1 means directly into slope

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // player
    public float speedCurrent;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    public float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    public float _jumpTimeoutDelta; /// i think this is input delay on jumping after touchign the ground
    private float _fallTimeoutDelta;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;
    public float pos1; // this is for the roof check
    public bool canMove = true, canJump = true;

    public PlayerInput _playerInput;

    public Animator _animator;
    public CharacterController _controller;
    public InputManager _input;
    public GameObject _mainCamera;

    public const float _threshold = 0.01f;

    public bool _hasAnimator;

    public Sound landedSFX, jumpSFX, dashSFX, moveSFX;
    public bool isRoofed = false;
    public float roofOffsetSphere = 2.0f;
    public float fallSpeedInitial = -2.0f;


    public float dashSpeed = 15.0f, dashDuration = 1.0f;
    public float dashRechargeCurrent = 0.0f, dashRechargeMax = 100.0f, dashRechargeRate = 25.0f, dashRechargeTime = 1.0f;
    public bool isDashing = false, isJumping = false, isDead = false;
    public bool unlockedDash = true, canDash;

    public SliderNew dashSlider;
    public Animator playerCanvasAnimator;
    public GameObject dashUI;

    public float posLow, posHigh;

    public float footStepTimerCurrent, footStepTimerLow, footStepTimerMax;

    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
            return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
        }
    }


    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    private void Start()
    {
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<InputManager>();
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

        AssignAnimationIDs();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        // _fallTimeoutDelta = FallTimeout;
        SetRechargeRate();

        if (unlockedDash)
        {
            TurnOnDash();
        }
        else
        {
            TurnOffDash();
        }

        // turns on all general canvas that holds all player ui (they can be disabled indiviually)
        playerCanvasAnimator.SetTrigger("On");

    }

    public void GainPlayerControl()
    {
        SetPlayerControl(true);
        _playerInput.actions["Move"].Enable();
        _playerInput.actions["Jump"].Enable();
        _playerInput.actions["JumpHold"].Enable();
    }

    public void LosePlayerControl()
    {
        SetPlayerControl(false);
        SetSpeedToZero();
        _playerInput.actions["Move"].Disable();
        _playerInput.actions["Jump"].Disable();
        _playerInput.actions["JumpHold"].Disable();
        _animator.SetBool(_animIDJump, false);
        _animator.SetBool(_animIDFreeFall, false);
    }

    public void GainCameraControl()
    {
        LockCameraPosition = false;
    }

    public void LoseCameraControl()
    {
        LockCameraPosition = true;
    }
    

    public void SetPlayerControl(bool playerControlState = true)
    {
        hasControl = playerControlState;
        //  _playerInput.enabled = playerControlState;
    }
    private void FixedUpdate()
    {
        GroundedCheck();
        CheckIfRoofed();
    }

    private void Update()
    {
        //print(_animator.GetFloat(_animIDSpeed));
        ///_hasAnimator = TryGetComponent(out _animator); // prolly not needed? since its already at start

        if (hasControl)
        {

            JumpAndGravity(); // should be after ground check because it still thinks its on ground when we jump because we havn't moved yet causing a bug
            Move();

            BufferJumpFunction();
            DashMechanic();
            //print($"y: {transform.position.y}");
        }
        else
        {
            if (isDashing)
            {
                return;
            }
            AddGravity();
        }
        if(transform.position.y < posLow)
        {
            posLow = transform.position.y;
        }
        if (transform.position.y > posHigh)
        {
            posLow = transform.position.y;
            posHigh = transform.position.y;
        }

    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void GroundedCheck()
    {
        // no need to check if we are going to land on ground if we are currently rising
        if (_verticalVelocity > 0 && !Grounded)
        {
            return;
        }
        // print("Set bool" + Grounded);
        bool lastCheckGrounded = Grounded;
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        //   OnDrawGizmos();
        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, Grounded);
            //  print("Set bool2" + Grounded);
        }

        if (Grounded && !lastCheckGrounded)
        {
            print($"Ground: {Grounded}, LastGround: {lastCheckGrounded}");
            FindObjectOfType<AudioManager>().PlaySoundInstantiate(landedSFX);
        }
        if (!Grounded && lastCheckGrounded)
        {
            print($"Ground: {Grounded}, LastGround: {lastCheckGrounded}");

        }
    }

    public void SetDead(bool shouldDead)
    {
        isDead = shouldDead;
    }

    public void SetCharacterController(bool shouldDead)
    {
        _controller.enabled = shouldDead;
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z), GroundedRadius);

        Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y + roofOffsetSphere,
            transform.position.z), GroundedRadius);
    }

    public void BufferJumpFunction()
    {
        if (_input.jump)
        {
            timeToHoldJumpBufferCurrent = timeToHoldJumpBufferMax;
            _input.jump = false;
        }

        if (timeToHoldJumpBufferCurrent > 0)
        {
            timeToHoldJumpBufferCurrent -= 1 * Time.deltaTime;
            jumpBufferIsOn = true;
            if (timeToHoldJumpBufferCurrent <= 0) {
                jumpBufferIsOn = false;
            }
        }


    }


    private void CameraRotation()
    {
        if (isDead)
        {
            // sets these 2 to 0 so it resets back to natural position
            //_cinemachineTargetYaw = 0; 
            _cinemachineTargetPitch = 0;
            return;
        }
        // if there is an input and camera position is not fixed
        if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        //  _cinemachineTargetYaw = _cinemachineTargetPitch = 0;
        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    private void Move()
    {
        if (!canMove)
        {
            _animator.SetFloat(_animIDSpeed, 0);
            return;
        }
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed;
        if (_input.sprint)
        {
            targetSpeed = SprintSpeed;
        }
        else
        {
            targetSpeed = MoveSpeed;
        }
        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        if (_input.move == Vector2.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        // float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;
        float inputMagnitude;
        if (_input.analogMovement)
        {
            inputMagnitude = _input.move.magnitude;
        }
        else
        {
            inputMagnitude = 1.0f;
        }

        // 0, 2.4
        // 1. 11.34
        // 2, 19.94
        // 3, 28.56

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            speedCurrent = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            speedCurrent = Mathf.Round(speedCurrent * 1000f) / 1000f;
        }
        else
        {
            speedCurrent = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // normalise input direction
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (_input.move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

            if (Grounded)
            {
                PlayMovingSFX();
            }
        }

        if (canMove)
        {
            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (speedCurrent * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }
        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }



    }

    public void PlayMovingSFX()
    {
        footStepTimerCurrent -= 1 * Time.deltaTime;

        if (footStepTimerCurrent <= 0)
        {
            FindObjectOfType<AudioManager>().PlaySoundInstantiate(moveSFX);
            SetFootStepTimer();
        }
    }

    public void SetFootStepTimer()
    {
        footStepTimerCurrent = Random.Range(footStepTimerLow, footStepTimerMax);
    }


    private void JumpAndGravity()
    {

        // this prevents the player from hovering after leaving the ground assuming no slopes were detected
        if (Grounded == false && groundedOnceCheck && _verticalVelocity <= 0) // prevent stop jumping
        {
            print("In grounded once check guard");
            groundedOnceCheck = false;
            _verticalVelocity = fallSpeedInitial;

            SetFootStepTimer();
        }

        // jump timeout (prevents player from jumping 
        if (_jumpTimeoutDelta >= 0.0f)
        {
            _jumpTimeoutDelta -= Time.deltaTime;
        }

        if (Grounded && _jumpTimeoutDelta <= 0.0f)
        {
            //  print($"On ground right now _input.jump = ({_input.jump} _jumpTimeoutDelta = ({_jumpTimeoutDelta}");
            groundedOnceCheck = true;
            if (_input.jumpHold && jumpTimeHold > 0) // edds
            {
                jumpTimeHold = 0;
            }

            // increase dash meter when on the ground
            ChargeDashMeter();



            jumpTimeHold = jumpTimeHoldReset; // resets jump holding

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                // Get the bounds of the player's collider
                Bounds bounds = _controller.bounds;

                // Calculate the front-most point of the collider (front of the player)
                Vector3 frontEdge = transform.position + transform.forward * bounds.extents.z;
                // Adjust the raycast position to be at the bottom front edge of the collider
                Vector3 rayOrigin = new Vector3(frontEdge.x, bounds.min.y, frontEdge.z);

                // Downward direction for the raycast
                Vector3 downwardDirection = Vector3.down;

                // Debug Ray (for visualization)
                Debug.DrawRay(rayOrigin, downwardDirection * 1, Color.blue);



                // Cast the ray
                RaycastHit hit;

                // guard to prevent cylinder collider messhaps when falling off edges
                // shoots in a downwards direction 1 space in front of the plauyer, means we are on ground/slope
                if (Physics.Raycast(rayOrigin, downwardDirection, out hit, 1.0f, GroundLayers))
                {
                    _verticalVelocity = GravityOfSloper;
                    // Debug.Log("Ray hit: " + hit.collider.name);

                    // You can add logic here for when the ray hits something
                }
                else
                {
                    // print("No floor ahead, velocity = 0");
                    _verticalVelocity = 0;
                }

            }

            // Jump
            if (jumpBufferIsOn)
            {
                FindObjectOfType<AudioManager>().PlaySoundInstantiate(jumpSFX);
                AddJumpForce();
                //  Debug.Log("Jump Velocity: " + _verticalVelocity);  // Should be ~7.67 m/s
                coyoteJumpTime = 0;
                //_input.jump = false;
                // update animator if using character
                if (_hasAnimator)
                {

                    _animator.SetBool(_animIDJump, true);
                }
                _jumpTimeoutDelta = JumpTimeout;
                // stops the falling check at the ttop if you jump 
                groundedOnceCheck = false;
                _input.jump = false;
                isJumping = true;
            }
            else
            {
                coyoteJumpTime = coyoteJumpTimeReset; // reset coyote timer
            }



        }
        else if (coyoteJumpTime > 0)
        {

            // _input.jump = false;
            coyoteJumpTime -= 1 * Time.deltaTime; // lower coyote time;
                                                  // Jump

            // Jump
            if (jumpBufferIsOn)
            {
                FindObjectOfType<AudioManager>().PlaySoundInstantiate(jumpSFX);
                print("Coyote Jumping");
                AddJumpForce();
                //  Debug.Log("Jump Velocity: " + _verticalVelocity);  // Should be ~7.67 m/s
                coyoteJumpTime = 0;
                //_input.jump = false;
                // update animator if using character
                if (_hasAnimator)
                {

                    _animator.SetBool(_animIDJump, true);
                }
                _jumpTimeoutDelta = JumpTimeout;
                // stops the falling check at the ttop if you jump 
                groundedOnceCheck = false;
                _input.jump = false;
                isJumping = true;
            }

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDFreeFall, true);
            }

        }
        else
        {
            //  print($"Jumping: {jumpTimeHold}, inputHold: {_input.jumpHold}, jumpHoldTime:{jumpTimeHold}");
            if (_input.jumpHold && isJumping && jumpTimeHold > 0) // holding jump to still jump higher
            {
                //print("else in jump hold");
                AddJumpForce();
                jumpTimeHold -= 1 * Time.deltaTime;
                //  print("JumpHOldBugger");
                //print(jumpTimeHold);
                //  print(Grounded);

                return;
            }
            else
            {
                // _input.jumpHold = false;
                jumpTimeHold = 0;
                isJumping = false;
            }
            // reset the jump timeout timer
            ///

            // fall timeout

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDFreeFall, true);
            }

            //   _input.jump = false;
        }
        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;


        }
        ///AddGravity();
    }

    public void AddGravity()
    {
        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity && _controller.enabled == true)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
            _controller.Move(new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        }
    }

    public void CheckIfRoofed()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y + roofOffsetSphere,
            transform.position.z);
        isRoofed = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        if (isRoofed)
        {
            print("roofed");
            jumpTimeHold = 0;
            coyoteJumpTime = 0;
            _verticalVelocity = -2.0f;
            StopJumpInputs();
        }
    }

    // moves the player (maybe use for bouncepad)
    public void AddJumpForce(float multiplier = 1)
    {
        // 1 * -2 * -30
        _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity) * Mathf.Sqrt(multiplier);
    }

    public void SetSpeedToZero()
    {
        speedCurrent = 0;
        _animator.SetFloat(_animIDSpeed, 0);
        print(_animator.GetFloat(_animIDSpeed));
    }

    // stops the jump and jump hold intputs from the input manager
    public void StopJumpInputs()
    {
        _input.jump = false;
        _input.jumpHold = false;
        canJump = false;
    }


    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }


    public void DashMechanic()
    {
        // if we are suppose to be in dashing, then add force
        if (isDashing)
        {
            DashinSpeedBoost();
            return;
        }

        // if hte player isn't pressing the interact button or hasn't unlocked the dash then ignore
        if (!_input.interact || !unlockedDash)
        {
            // print($"{_input.interact} {isDashing}");
            return;
        }

        if (canDash)
        {
            StartDash();
        }

    }

    public void StartDash()
    {
        // the initial press of the dash
        _input.interact = false;
        FindObjectOfType<AudioManager>().PlaySoundInstantiate(dashSFX);
        // only happens the first time the player presses interact

        StartCoroutine(DashWait());

        UsedDashMeter();
        _verticalVelocity = 0;
        isDashing = true;

        canDash = false;
    }

    public void DashinSpeedBoost()
    {
        // increases the players speed, prevents input, moves in current held direction

     //   print("Dashing");
        // normalise input direction
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
      //  print(_input.move);
        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (_input.move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }
        else
        {
            // if no input i guess (should make player dash forwards from camera
            _targetRotation = _mainCamera.transform.eulerAngles.y;
        }
       // _playerInput.actions["Move"].Disable();



        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
        canMove = false;
        // move the player
        _controller.Move(targetDirection.normalized * (dashSpeed * Time.deltaTime));
        

    }

    public IEnumerator DashWait()
    {
        yield return new WaitForSeconds(dashDuration);
        StopDash();
      //  print("Stop dashing");
        // end the dash and regive player movement
    }

    public void StopDash()
    {
        isDashing = false;
        canMove = true;
        _playerInput.actions["Move"].Enable();
    }

    public void SetCameraRotation(Quaternion rotation)
    {
        print("Set cam rotation");
        // sets the player's camera to a set angle (should be on respawn)

        gameObject.transform.rotation = rotation;
        _cinemachineTargetYaw = gameObject.transform.rotation.eulerAngles.y;
        //CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(0, gameObject.transform.rotation.eulerAngles.y, 0);
        print(rotation.eulerAngles);        print(gameObject.transform.rotation.eulerAngles);
        print(CinemachineCameraTarget.transform.rotation.eulerAngles);

    }

    public void ChargeDashMeter()
    {
        // called when on ground

        // entry gate when we are already charged
        if (dashRechargeCurrent >= dashRechargeMax)
        {
            return;
        }


        // increase to new amount
        dashRechargeCurrent += dashRechargeRate * Time.deltaTime;
        dashSlider.SetSlider(dashRechargeCurrent);
        // checks it once when dash has fully charge
        // also checks if the previous one wasn't
        if (dashRechargeCurrent >= dashRechargeMax)
        {
            FullyChargeDashMeter();
        }

    }

    public void FullyChargeDashMeter()
    {
        // called when you want to fully charge the dash meter instantly
        // or call when the meter naturally fills up
        canDash = true;
        dashRechargeCurrent = dashRechargeMax;
        dashSlider.FullyFillSlider();
    }

    public void UsedDashMeter()
    {
        dashRechargeCurrent = 0;
        // need to call deplete
        dashSlider.DepleteSlider();
    }

    public void SetRechargeRate()
    {
        // this is based on the dash time and converting it to the recharge rate
        dashRechargeRate = dashRechargeMax / dashRechargeTime;

        dashSlider.maxValue = dashRechargeMax;
    }

    public void TurnOnDash()
    {
        unlockedDash = true;
        dashUI.SetActive(true);
        // need to turn on canvas and stuff
    }

    public void TurnOffDash()
    {
        unlockedDash = false;
        dashUI.SetActive(false);
        // need to turn on canvas and stuff
    }
    // this should bounce the player 3x their jump height
    public void Bounce()
    {
        coyoteJumpTime = 0;
        //_input.jump = false;
        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDJump, true);
        }
        _jumpTimeoutDelta = JumpTimeout;
        // stops the falling check at the ttop if you jump 
        groundedOnceCheck = false;
        _input.jump = false;

        // give player dash back
        FullyChargeDashMeter();
        AddJumpForce(6);
    }

    public void SetPlayerPosition(GameObject placeToPutPlayer)
    {
        // moves player to set vector position
        SetCharacterController(false); // need it to be false to prevent gravity and other movements being applied and re-teleporting the player back to where they were
        transform.position = placeToPutPlayer.transform.position; // teleports player to cp
        SetCameraRotation(placeToPutPlayer.transform.rotation); // sets camera to cp's rotation
        SetCharacterController(true); // re enables the character controller so they can move

    }
}
