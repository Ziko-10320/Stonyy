using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // ── Run ───────────────────────────────────────────────────────────────
    [Header("Run")]
    [SerializeField] float runSpeed = 8f;
    float runDirection = 1f;

    // ── Jump ──────────────────────────────────────────────────────────────
    [Header("Jump")]
    [SerializeField] float jumpVelocity = 16f;          // Y velocity set on jump
    [SerializeField] float jumpHoldGravityScale = 1f;   // gravity while holding button (rising)
    [SerializeField] float fallGravityScale = 4f;       // gravity after release / falling
    [SerializeField] float maxHoldTime = 0.25f;         // max time hold extends the jump
    [SerializeField] float jumpCutVelocityY = 2f;       // Y velocity clamped to on button release

    // ── Jump Buffer ───────────────────────────────────────────────────────
    [Header("Jump Buffer")]
    [SerializeField] float jumpBufferTime = 0.15f;

    // ── Air Dash ──────────────────────────────────────────────────────────
    [Header("Air Dash")]
    [SerializeField] float airDashSpeed = 20f;
    [SerializeField] float airDashDuration = 0.18f;
    [SerializeField] float airDashGravityScale = 0f;    // no gravity during dash
    [SerializeField] float airDashSpeedBoostPercent = 25f;
    [SerializeField] float airDashBoostDuration = 6f;
    // ── Land Recovery ─────────────────────────────────────────────────────
    [Header("Land")]
    [SerializeField] float landRecoveryTime = 0.08f;

    // ── Ground Check ──────────────────────────────────────────────────────
    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.1f;
    [SerializeField] LayerMask groundLayer;

    // ── Wall Jump ──────────────────────────────────────────────────────
    [Header("Wall")]
    [SerializeField] Vector2 wallCheckOffset = new Vector2(0.3f, 0f);
    [SerializeField] float wallCheckDistance = 0.15f;
    [SerializeField] LayerMask wallLayer;
    [SerializeField] float wallSlideMinSpeed = 0.5f;
    [SerializeField] float wallSlideMaxSpeed = 4f;
    [SerializeField] float wallSlideRampTime = 1.5f;
    [SerializeField] float wallClimbEntrySpeed = 8f;

    // ── Input ─────────────────────────────────────────────────────────────
    [Header("Input")]
    [SerializeField] InputActionAsset inputActions;
    [Header("Slide")]
    [SerializeField] InputActionAsset slideInputActions; // drag same asset
    [SerializeField] float slideSpeedBoostPercent = 25f;  // % boost during and after slide
    [SerializeField] float slideBoostDuration = 6f;        // seconds boost lasts after slide ends
    [SerializeField] float airSlamSpeed = 20f;

    InputAction slideAction;

    [Header("Speed Boost Trails")]
    [SerializeField] TrailRenderer trail1Boost1;
    [SerializeField] TrailRenderer trail2Boost1;
    [SerializeField] TrailRenderer trail1Boost2;
    [SerializeField] TrailRenderer trail2Boost2;

    // ── Animator triggers ─────────────────────────────────────────────────
    const string ANIM_JUMP = "Jump";
    const string ANIM_FALL = "Fall";
    const string ANIM_LAND = "Land";
    const string ANIM_AIR_DASH = "AirDash";
    const string ANIM_WALL_SLIDE = "WallSlide";
    const string ANIM_WALL_JUMP = "WallJump";
    const string ANIM_IDLE_WALL = "IdleWall";
    const string ANIM_SLIDE = "Slide";
    const string ANIM_DIAGONAL_SLIDE = "DiagonalSlide";
    // ── State ─────────────────────────────────────────────────────────────
    enum State { Run, Jump, Fall, AirDash, Land, WallSlide, Slide, AirSlide }
    // ── Private references ────────────────────────────────────────────────
    Rigidbody2D rb;
    Animator anim;
    InputAction mvmtAction;

    // ── Runtime state ─────────────────────────────────────────────────────
    State currentState;
    bool isGrounded;

    // Jump
    bool btnHeld;
    bool btnPressedThisFrame;
    bool isHoldingJump;
    float jumpHoldTimer;
    bool jumpCutApplied;

    // Jump buffer
    float jumpBufferTimer;

    // Air dash
    bool canAirDash;
    bool isDashing;
    float dashTimer;

    // Land
    float landTimer;

    bool isTouchingWall;
    bool isWallSliding;
    float wallSlideTimer;
    bool isWallClimbing;
    float wallClimbHeightUsed;

    bool slideBtnHeld;
    float slideBoostTimer;
    bool isSlideBoostActive;

    float airDashBoostTimer;
    bool isAirDashBoostActive;

    public bool JustLanded => currentState == State.Land && landTimer >= landRecoveryTime - Time.deltaTime;
    public bool WasWallSliding { get; private set; }

    public bool IsGrounded => isGrounded;
    // ─────────────────────────────────────────────────────────────────────
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        var map = inputActions.FindActionMap("Player", throwIfNotFound: true);
        mvmtAction = map.FindAction("Mvmt", throwIfNotFound: true);
        slideAction = map.FindAction("Slide", throwIfNotFound: true);
    }

    void OnEnable()
    {
        mvmtAction.Enable();
        mvmtAction.performed += OnMvmtPerformed;
        mvmtAction.canceled += OnMvmtCanceled;
        slideAction.Enable();
        slideAction.performed += OnSlidePerformed;
        slideAction.canceled += OnSlideCanceled;
    }

    void OnDisable()
    {
        mvmtAction.performed -= OnMvmtPerformed;
        mvmtAction.canceled -= OnMvmtCanceled;
        mvmtAction.Disable();
        slideAction.performed -= OnSlidePerformed;
        slideAction.canceled -= OnSlideCanceled;
        slideAction.Disable();
    }

    void OnMvmtPerformed(InputAction.CallbackContext ctx)
    {
        btnPressedThisFrame = true;
        btnHeld = true;
        jumpBufferTimer = jumpBufferTime;
    }

    void OnMvmtCanceled(InputAction.CallbackContext ctx)
    {
        btnHeld = false;
    }
    void OnSlidePerformed(InputAction.CallbackContext ctx)
    {
        slideBtnHeld = true;
    }

    void OnSlideCanceled(InputAction.CallbackContext ctx)
    {
        slideBtnHeld = false;
        if (currentState == State.Slide)
            StopSlide();
    }
    // ─────────────────────────────────────────────────────────────────────

    void UpdateTrails()
    {
        bool boost1 = isSlideBoostActive || isAirDashBoostActive;
        bool boost2 = isSlideBoostActive && isAirDashBoostActive;

        // Boost 1 trails
        if (boost1)
        {
            trail1Boost1.gameObject.SetActive(true);
            trail2Boost1.gameObject.SetActive(true);
            trail1Boost1.emitting = true;
            trail2Boost1.emitting = true;
        }
        else
        {
            StartCoroutine(StopTrail(trail1Boost1));
            StartCoroutine(StopTrail(trail2Boost1));
        }

        // Boost 2 trails
        if (boost2)
        {
            trail1Boost2.gameObject.SetActive(true);
            trail2Boost2.gameObject.SetActive(true);
            trail1Boost2.emitting = true;
            trail2Boost2.emitting = true;
        }
        else
        {
            StartCoroutine(StopTrail(trail1Boost2));
            StartCoroutine(StopTrail(trail2Boost2));
        }
    }
    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        jumpBufferTimer -= Time.deltaTime;

        bool slideWasActive = isSlideBoostActive;
        bool dashWasActive = isAirDashBoostActive;

        if (isSlideBoostActive)
        {
            slideBoostTimer -= Time.deltaTime;
            if (slideBoostTimer <= 0f) isSlideBoostActive = false;
        }
        if (isAirDashBoostActive)
        {
            airDashBoostTimer -= Time.deltaTime;
            if (airDashBoostTimer <= 0f) isAirDashBoostActive = false;
        }

        if (slideWasActive != isSlideBoostActive || dashWasActive != isAirDashBoostActive)
            UpdateTrails();
        CheckWall();
        switch (currentState)
        {
            case State.Run: HandleRun(); break;
            case State.Jump: HandleJump(); break;
            case State.Fall: HandleFall(); break;
            case State.AirDash: HandleAirDash(); break;
            case State.Land: HandleLand(); break;
            case State.WallSlide: HandleWallSlide(); break;
            case State.Slide: HandleSlide(); break;
            case State.AirSlide: HandleAirSlide(); break;
        }

        // Gravity scale: hold = low, release/fall = high → snappy SMB feel
        if (!isDashing && !isWallSliding && !isWallClimbing)
        {
            if (isHoldingJump && rb.linearVelocity.y > 0f)
                rb.gravityScale = jumpHoldGravityScale;
            else
                rb.gravityScale = fallGravityScale;
        }
        btnPressedThisFrame = false;
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            // Maintain dash velocity, no gravity interference
            rb.linearVelocity = new Vector2(airDashSpeed * runDirection, 0f);
            return;
        }
        if (currentState == State.Slide)
        {
            float boostedSpeed = runSpeed * (1f + slideSpeedBoostPercent / 100f);
            rb.linearVelocity = new Vector2(boostedSpeed * runDirection, rb.linearVelocity.y);
            return;
        }

        if (currentState == State.AirSlide)
        {
            rb.linearVelocity = new Vector2(runSpeed * runDirection, -airSlamSpeed);
            return;
        }
        if (isWallClimbing)
        {
            if (!btnHeld || !isTouchingWall)
            {
                isWallClimbing = false;
                isWallSliding = true;
                isHoldingJump = false;
                jumpHoldTimer = 0f;
                wallSlideTimer = 0f;
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
                return;
            }

            // Only count height while actually rising
            if (rb.linearVelocity.y > 0f)
                wallClimbHeightUsed += rb.linearVelocity.y * Time.fixedDeltaTime;

            // Momentum has died — player is no longer rising, force slide
            if (rb.linearVelocity.y <= 0f)
            {
                isWallClimbing = false;
                isWallSliding = true;
                isHoldingJump = false;
                jumpHoldTimer = 0f;
                wallSlideTimer = 0f;
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
                return;
            }

            // Still rising — lock X, let upward momentum carry naturally
            rb.gravityScale = jumpHoldGravityScale;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }
        if (isWallSliding)
        {
            // Always control fully — ignore btnHeld, gravity scale is always 0 here
            rb.gravityScale = 0f;
            wallSlideTimer += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(wallSlideTimer / wallSlideRampTime);
            float slideY = -Mathf.Lerp(wallSlideMinSpeed, wallSlideMaxSpeed, t);
            rb.linearVelocity = new Vector2(0f, slideY);
            return;
        }
        // Horizontal: always auto-run
        float speedMultiplier = 1f;
        if (isSlideBoostActive) speedMultiplier += slideSpeedBoostPercent / 100f;
        if (isAirDashBoostActive) speedMultiplier += airDashSpeedBoostPercent / 100f;
        float currentSpeed = runSpeed * speedMultiplier;
        rb.linearVelocity = new Vector2(currentSpeed * runDirection, rb.linearVelocity.y);

        // Jump cut: button released while rising → snap Y down instantly
        if (!btnHeld && !jumpCutApplied && rb.linearVelocity.y > jumpCutVelocityY)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpCutVelocityY);
            jumpCutApplied = true;
            isHoldingJump = false;
        }

        // Hold timer: stop holding boost once timer runs out
        if (isHoldingJump)
        {
            jumpHoldTimer -= Time.fixedDeltaTime;
            if (jumpHoldTimer <= 0f)
                isHoldingJump = false;
        }
    }

    // ── State handlers ────────────────────────────────────────────────────

    void HandleRun()
    {
        if (!isGrounded)
        {
            ChangeState(State.Fall);
            return;
        }
        if (isTouchingWall)
        {
            anim.SetBool(ANIM_IDLE_WALL, true);
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
        else
        {
            anim.SetBool(ANIM_IDLE_WALL, false);
        }
        if (slideBtnHeld)
        {
            StartSlide();
            return;
        }
        if (btnPressedThisFrame || jumpBufferTimer > 0f)
            StartJump();
    }

    void HandleJump()
    {
        // Holding jump
        if (btnHeld && jumpHoldTimer > 0f)
            isHoldingJump = true;

        if (slideBtnHeld)
        {
            StartAirSlide();
            return;
        }
        // Air dash
        if (btnPressedThisFrame && canAirDash)
        {
            StartAirDash();
            return;
        }
        if (isTouchingWall && !isGrounded)
        {
            EnterWallSlide();
            return;
        }
        // Transition to fall once Y velocity goes negative
        if (rb.linearVelocity.y < 0f)
        {
            isHoldingJump = false;
            jumpHoldTimer = 0f;
            ChangeState(State.Fall);
        }
    }

    void HandleFall()
    {
        if (isTouchingWall && !isGrounded)
        {
            EnterWallSlide();
            return;
        }
        if (slideBtnHeld)
        {
            StartAirSlide();
            return;
        }
        // Air dash
        if (btnPressedThisFrame && canAirDash)
        {
            StartAirDash();
            return;
        }

        if (isGrounded)
            Land();
    }

    void HandleAirDash()
    {
        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f || isGrounded)
        {
            isDashing = false;
            rb.gravityScale = fallGravityScale;

            if (isGrounded)
                Land();
            else
                ChangeState(rb.linearVelocity.y < 0f ? State.Fall : State.Jump);
        }
    }

    void HandleLand()
    {
        landTimer -= Time.deltaTime;

        if (landTimer <= 0f)
            ChangeState(State.Run);

        // Jump buffer: if player pressed just before landing, jump immediately
        if (isGrounded && jumpBufferTimer > 0f)
            StartJump();
    }

    // ── Actions ───────────────────────────────────────────────────────────

    void StartJump()
    {
        jumpBufferTimer = 0f;
        jumpCutApplied = false;
        canAirDash = true;
        isHoldingJump = true;
        jumpHoldTimer = maxHoldTime;
        WasWallSliding = false;
        // Set Y velocity directly — clean, no accumulated forces
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);

        ChangeState(State.Jump);
    }

    void StartAirDash()
    {
        canAirDash = false;
        isDashing = true;
        dashTimer = airDashDuration;
        isHoldingJump = false;
        jumpHoldTimer = 0f;
        rb.gravityScale = airDashGravityScale;

        rb.linearVelocity = new Vector2(airDashSpeed * runDirection, 0f);

        ChangeState(State.AirDash);
    }

    void Land()
    {
        ExitWallSlide();
        canAirDash = true;
        landTimer = landRecoveryTime;
        ChangeState(State.Land);
    }

    // ── State change + animator ───────────────────────────────────────────

    void ChangeState(State newState)
    {
        State previous = currentState;
        currentState = newState;

        switch (newState)
        {
            case State.Jump: anim.SetTrigger(ANIM_JUMP); break;
            case State.Fall:
                if (previous == State.AirSlide)
                    anim.ResetTrigger(ANIM_DIAGONAL_SLIDE);
                anim.SetTrigger(ANIM_FALL);
                break;
            case State.AirDash: anim.SetTrigger(ANIM_AIR_DASH); break;
            case State.Land: anim.SetTrigger(ANIM_LAND); break;
            case State.Run: anim.SetBool(ANIM_IDLE_WALL, false); break;
        }
    }
    void CheckWall()
    {
        Vector2 origin = (Vector2)transform.position;

        bool hitRight = Physics2D.Raycast(
            origin + new Vector2(wallCheckOffset.x, wallCheckOffset.y),
            Vector2.right, wallCheckDistance, wallLayer);

        bool hitLeft = Physics2D.Raycast(
            origin + new Vector2(-wallCheckOffset.x, wallCheckOffset.y),
            Vector2.left, wallCheckDistance, wallLayer);

        isTouchingWall = runDirection > 0f ? hitRight : hitLeft;
    }

    void EnterWallSlide()
    {
        wallSlideTimer = 0f;
        wallClimbHeightUsed = 0f;
        isWallClimbing = false;

        if (btnHeld)
        {
            isWallSliding = false;
            isWallClimbing = true;
            wallClimbHeightUsed = 0f;
            // Cap entry speed so close-range jumps don't give huge climb height
            float clampedY = Mathf.Min(rb.linearVelocity.y, wallClimbEntrySpeed);
            rb.linearVelocity = new Vector2(0f, clampedY);
            anim.SetBool(ANIM_WALL_SLIDE, true);
        }
        else
        {
            isWallSliding = true;
            isWallClimbing = false;
            anim.SetBool(ANIM_WALL_SLIDE, true);
        }

        ChangeState(State.WallSlide);
    }

    void HandleWallSlide()
    {
        if (isWallClimbing)
        {
            if (!isTouchingWall)
            {
                ExitWallSlide();
                ChangeState(State.Jump);
                return;
            }

            if (btnPressedThisFrame)
            {
                StartJump();
                return;
            }
            return;
        }
        if (!isTouchingWall || isGrounded)
        {
            ExitWallSlide();
            ChangeState(isGrounded ? State.Run : State.Fall);
            return;
        }

        if (btnPressedThisFrame)
        {
            ExitWallSlide();

            // Flip direction
            runDirection *= -1f;
            Vector3 s = transform.localScale;
            s.x *= -1f;
            transform.localScale = s;

            // Reuse the exact same jump system
            anim.SetTrigger(ANIM_WALL_JUMP);
            StartJump();
            return;
        }
    }
    void ExitWallSlide()
    {
        WasWallSliding = isWallSliding || isWallClimbing;
        isWallSliding = false;
        isWallClimbing = false;
        anim.SetBool(ANIM_WALL_SLIDE, false);
        anim.SetBool(ANIM_IDLE_WALL, false);
    }
    void HandleSlide()
    {
        if (!isGrounded)
        {
            StopSlide();
            return;
        }

        if (!slideBtnHeld)
            StopSlide();
    }

    void StartSlide()
    {
        anim.SetBool(ANIM_SLIDE, true);
        ChangeState(State.Slide);
    }

    void StopSlide()
    {
        anim.SetBool(ANIM_SLIDE, false);
        isSlideBoostActive = true;
        slideBoostTimer = slideBoostDuration;
        UpdateTrails();
        ChangeState(State.Run);
    }

    void HandleAirSlide()
    {
        if (isGrounded)
        {
            if (slideBtnHeld)
                StartSlide();
            else
                Land();
            return;
        }

        if (!slideBtnHeld)
        {
            ChangeState(State.Fall);
            return;
        }
    }

    void StartAirSlide()
    {
        isHoldingJump = false;
        jumpHoldTimer = 0f;
        anim.ResetTrigger(ANIM_FALL);
        anim.SetTrigger(ANIM_DIAGONAL_SLIDE);
        ChangeState(State.AirSlide);
    }
    public void OnAirDashAnimationEnd()
    {
        isAirDashBoostActive = true;
        airDashBoostTimer = airDashBoostDuration;
        UpdateTrails();
    }
    public void FlipDirection()
    {
        runDirection *= -1f;
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
        ChangeState(State.Run);
        anim.Play("Run"); // reset to start of run anim on flip, prevents sliding sprite if flipping while falling
    }
    IEnumerator StopTrail(TrailRenderer trail)
    {
        trail.emitting = false;
        yield return new WaitForSeconds(trail.time); // wait for existing trail to fade
        trail.gameObject.SetActive(false);
    }
    // ── Gizmos ────────────────────────────────────────────────────────────

    void OnDrawGizmos()
    {
        if (groundCheck == null) return;

        Gizmos.color = (Application.isPlaying && isGrounded)
            ? new Color(0.2f, 1f, 0.2f, 0.4f)
            : new Color(1f, 1f, 1f, 0.2f);
        Gizmos.DrawSphere(groundCheck.position, groundCheckRadius);

        Gizmos.color = (Application.isPlaying && isGrounded)
            ? new Color(0.2f, 1f, 0.2f, 1f)
            : new Color(1f, 0.3f, 0.3f, 1f);
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Vector2 origin = (Vector2)transform.position;
        Vector2 rightOrigin = origin + new Vector2(wallCheckOffset.x, wallCheckOffset.y);
        Vector2 leftOrigin = origin + new Vector2(-wallCheckOffset.x, wallCheckOffset.y);

        Gizmos.color = (Application.isPlaying && isTouchingWall && runDirection > 0f)
            ? new Color(1f, 0.5f, 0f, 1f) : new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawLine(rightOrigin, rightOrigin + Vector2.right * wallCheckDistance);

        Gizmos.color = (Application.isPlaying && isTouchingWall && runDirection < 0f)
            ? new Color(1f, 0.5f, 0f, 1f) : new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawLine(leftOrigin, leftOrigin + Vector2.left * wallCheckDistance);
    }
}