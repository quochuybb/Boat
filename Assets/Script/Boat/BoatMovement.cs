using UnityEngine;

public class BoatMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardForce = 50f; // Force for forward/backward (W/S)
    public float strafeForce = 20f;   // Force for strafing (Q/E)
    public float turnTorque = 40f;    // Torque for turning (A/D)

    [Header("Momentum Settings")]
    public float turnMomentum = 0.1f; // How quickly turning builds up momentum (lower = smoother)
    public float turnDecay = 0.95f;   // How quickly turning slows down (higher = slower decay)

    [Header("Floating Detection")]
    public float timeBeforeRespawn = 5f; // Time before respawning when floating
    public float groundThreshold = -0.5f; // Height threshold for ground

    [Header("References")]
    public Rigidbody boatRigidbody;

    private float moveInput;  // W/S for forward/backward
    private float strafeInput; // Q/E for strafing
    private float turnInput;   // A/D for turning
    private float timeFloating = 0f; // Track how long we've been floating above ground
    private float targetTurnVelocity = 0f; // Target angular velocity we want to reach
    private float currentTurnVelocity = 0f; // Current angular velocity

    void Start()
    {
            boatRigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Get input from InputHandler instead of direct Input calls
        if (InputHandler.Instance != null)
        {
            moveInput = InputHandler.Instance.GetForwardInput(); // W/S
            strafeInput = InputHandler.Instance.GetStrafeInput(); // Q/E
            turnInput = InputHandler.Instance.GetTurnInput(); // A/D
        }
        else
        {
            // Fallback to old input system if InputHandler not found
            moveInput = -Input.GetAxisRaw("Vertical");
            strafeInput = 0f;
            if (Input.GetKey(KeyCode.Q)) strafeInput = -1f;
            if (Input.GetKey(KeyCode.E)) strafeInput = 1f;
            turnInput = Input.GetAxisRaw("Horizontal");
        }
    }

    void FixedUpdate()
    {
        if (boatRigidbody == null) return;

        // Skip movement logic if Rigidbody is kinematic (during respawn)
        if (boatRigidbody.isKinematic) return;

        bool isAboveGroundThreshold = transform.position.y > groundThreshold;
        bool isTouchingGround = IsTouchingGround();

        // Handle floating time tracking
        // Respawn when above ground threshold but NOT actually touching ground
        if (isAboveGroundThreshold && !isTouchingGround)
        {
            timeFloating += Time.fixedDeltaTime;
            
            // Check if we've been floating too long
            if (timeFloating >= timeBeforeRespawn)
            {
                RespawnToLastCheckpoint();
                return; // Exit early since we're respawning
            }
        }
        else
        {
            // Reset floating time when actually on ground or below threshold
            timeFloating = 0f;
        }

        // Only apply movement forces when on ground
        if (isTouchingGround)
        {
            // Apply forward/backward force
            if (moveInput != 0)
            {
                Vector3 forward = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
                boatRigidbody.AddForce(forward * moveInput * forwardForce);
            }

            // Apply strafe force
            if (strafeInput != 0)
            {
                Vector3 right = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
                boatRigidbody.AddForce(right * strafeInput * strafeForce);
            }

            // Apply smooth turning with momentum
            // Calculate target angular velocity based on input
            targetTurnVelocity = turnInput * turnTorque;
            
            // Smoothly interpolate towards target velocity (this creates momentum)
            currentTurnVelocity = Mathf.Lerp(currentTurnVelocity, targetTurnVelocity, turnMomentum);
            
            // Apply the angular velocity directly
            boatRigidbody.angularVelocity = new Vector3(0, currentTurnVelocity, 0);
            
            // Apply decay when no input (this slows down turning gradually)
            if (turnInput == 0)
            {
                currentTurnVelocity *= turnDecay;
                
                // Stop completely if velocity is very small
                if (Mathf.Abs(currentTurnVelocity) < 0.1f)
                {
                    currentTurnVelocity = 0f;
                }
                
                // Update angular velocity
                boatRigidbody.angularVelocity = new Vector3(0, currentTurnVelocity, 0);
            }
        }
        else
        {
            // When not on ground, allow some natural rotation but with decay
            currentTurnVelocity *= 0.9f;
            boatRigidbody.angularVelocity = new Vector3(0, currentTurnVelocity, 0);
        }
    }

    private bool IsTouchingGround()
    {
        // Use Physics.OverlapSphere to check for colliders in the Ground layer
        float checkRadius = 1f; // Adjust based on your boat size
        Vector3 checkPosition = transform.position;
        
        // Create a sphere around the boat to detect ground colliders
        Collider[] hits = Physics.OverlapSphere(checkPosition, checkRadius);
        
        foreach (Collider hit in hits)
        {
            // Check if the collider is on the "Ground" layer
            if (hit.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                return true;
            }
        }
        
        return false;
    }

    private void RespawnToLastCheckpoint()
    {
        // Reset turning momentum on respawn
        currentTurnVelocity = 0f;
        targetTurnVelocity = 0f;
        
        // Only reset angular velocity if not kinematic
        if (boatRigidbody != null && !boatRigidbody.isKinematic)
        {
            boatRigidbody.angularVelocity = Vector3.zero;
        }

        // Check if CheckpointManager exists and has a valid checkpoint
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RespawnToLastCheckpoint();
        }
        else
        {
            Debug.LogWarning("CheckpointManager not found! Cannot respawn player.");
        }
    }
}