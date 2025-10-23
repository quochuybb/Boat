using UnityEngine;
using System.Collections.Generic;

public class MovingObstacle : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;        // Speed of movement
    public float moveDistance = 5f;     // Distance to move in each direction
    public bool startMovingRight = true; // Direction to start moving

    [Header("Movement Type")]
    public bool smoothMovement = true;  // Smooth sine wave vs linear back-and-forth
    public float smoothMultiplier = 1f; // For sine wave movement speed

    [Header("Physics Settings")]
    public bool useRigidbody = true;    // Use Rigidbody for physics interaction
    public float mass = 10f;            // Mass of the obstacle (higher = harder to push)

    [Header("Checkpoint Settings")]
    public List<Checkpoint> startCheckpoints = new List<Checkpoint>(); // Checkpoints where obstacle starts moving
    public List<Checkpoint> endCheckpoints = new List<Checkpoint>();   // Checkpoints where obstacle stops moving
    public bool activeByDefault = false; // Whether obstacle is active before any checkpoint is reached

    private Vector3 startPosition;
    private bool movingRight;
    private float timeElapsed = 0f;
    private Rigidbody obstacleRigidbody;
    private bool isMoving = false;
    private bool isSubscribed = false;

    void Start()
    {
        startPosition = transform.position;
        movingRight = startMovingRight;

        // Initialize Rigidbody if enabled
        if (useRigidbody)
        {
            obstacleRigidbody = GetComponent<Rigidbody>();
            if (obstacleRigidbody == null)
            {
                obstacleRigidbody = gameObject.AddComponent<Rigidbody>();
            }
            
            obstacleRigidbody.mass = mass;
            obstacleRigidbody.useGravity = false;
            obstacleRigidbody.isKinematic = false; // Allow physics to affect it
        }

        // Set initial state
        isMoving = activeByDefault;
        
        // Subscribe to checkpoint events
        SubscribeToEvents();
        
        // Check initial state
        CheckInitialCheckpointState();
    }

    void SubscribeToEvents()
    {
        if (!isSubscribed)
        {
            CheckpointManager.OnCheckpointChanged += OnCheckpointChanged;
            isSubscribed = true;
        }
    }

    void UnsubscribeFromEvents()
    {
        if (isSubscribed)
        {
            CheckpointManager.OnCheckpointChanged -= OnCheckpointChanged;
            isSubscribed = false;
        }
    }

    void CheckInitialCheckpointState()
    {
        if (CheckpointManager.Instance != null)
        {
            Checkpoint currentCheckpoint = CheckpointManager.Instance.GetCurrentCheckpoint();
            if (currentCheckpoint != null)
            {
                OnCheckpointChanged(currentCheckpoint);
            }
        }
    }

    void OnEnable()
    {
        SubscribeToEvents();
    }

    void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    // Called when checkpoint changes
    void OnCheckpointChanged(Checkpoint newCheckpoint)
    {
        // Check if this is a start checkpoint
        if (startCheckpoints.Contains(newCheckpoint))
        {
            StartMoving();
        }
        // Check if this is an end checkpoint
        else if (endCheckpoints.Contains(newCheckpoint))
        {
            StopMoving();
        }
    }

    public void StartMoving()
    {
        isMoving = true;
    }

    public void StopMoving()
    {
        isMoving = false;
    }

    void Update()
    {
        if (!isMoving) return;
            if (smoothMovement)
            {
                // Smooth sine wave movement
                timeElapsed += Time.deltaTime;
                float offset = Mathf.Sin(timeElapsed * moveSpeed * smoothMultiplier) * moveDistance;
                transform.position = startPosition + Vector3.right * offset;
            }
            else
            {
                // Linear back-and-forth movement
                MoveLinear();
            }
    }

    private void MoveLinear()
    {
        Vector3 targetPosition;
        
        if (movingRight)
        {
            targetPosition = startPosition + Vector3.right * moveDistance;
        }
        else
        {
            targetPosition = startPosition + Vector3.left * moveDistance;
        }

        // Move towards target
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Check if reached target
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            movingRight = !movingRight; // Reverse direction
        }
    }

    // Optional: Visualize movement path in editor
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            startPosition = transform.position;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startPosition + Vector3.left * moveDistance, startPosition + Vector3.right * moveDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(startPosition + Vector3.left * moveDistance, 0.2f);
        Gizmos.DrawSphere(startPosition + Vector3.right * moveDistance, 0.2f);
        
        // Draw checkpoint associations
        Gizmos.color = Color.green;
        foreach (var cp in startCheckpoints)
        {
            if (cp != null)
            {
                Gizmos.DrawLine(transform.position, cp.transform.position);
                Gizmos.DrawSphere(cp.transform.position, 0.5f);
            }
        }
        
        Gizmos.color = Color.red;
        foreach (var cp in endCheckpoints)
        {
            if (cp != null)
            {
                Gizmos.DrawLine(transform.position, cp.transform.position);
                Gizmos.DrawSphere(cp.transform.position, 0.5f);
            }
        }
    }
}