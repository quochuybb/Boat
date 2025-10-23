using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [Header("Ball Settings")]
    public GameObject ballPrefab;           // The ball prefab to spawn
    public Transform spawnParent;          // Parent for spawned balls (optional)
    
    [Header("Checkpoint Settings")]
    public List<Checkpoint> startCheckpoints = new List<Checkpoint>(); // Checkpoints where spawner starts
    public List<Checkpoint> endCheckpoints = new List<Checkpoint>();   // Checkpoints where spawner stops
    
    [Header("Spawn Settings")]
    public float minSpawnTime = 2f;        // Minimum time between spawns
    public float maxSpawnTime = 5f;        // Maximum time between spawns
    public float minHeightOffset = 2f;     // Minimum height above spawner
    public float maxHeightOffset = 10f;    // Maximum height above spawner
    public float horizontalRangeMultiplier = 1f; // Multiplier for horizontal range based on scale
    
    // [Header("Force Settings")]
    // public float minHorizontalForce = -5f; // Minimum horizontal force
    // public float maxHorizontalForce = 5f;  // Maximum horizontal force
    // public float minVerticalForce = -2f;   // Minimum vertical force
    // public float maxVerticalForce = 2f;    // Maximum vertical force
    
    [Header("Pooling Settings")]
    public int poolSize = 20;              // Number of balls to pre-spawn
    public bool canGrow = true;            // Allow pool to grow if needed
    
    private Queue<GameObject> ballPool;
    private bool isSpawning = false;
    private Coroutine spawnCoroutine;
    private Checkpoint currentActiveCheckpoint;

    void Start()
    {
        InitializePool();
        
        // Subscribe to checkpoint events
        if (CheckpointManager.Instance != null)
        {
            // Check initial state
            Checkpoint initialCheckpoint = CheckpointManager.Instance.GetCurrentCheckpoint();
            if (initialCheckpoint != null)
            {
                OnCheckpointChanged(initialCheckpoint);
            }
        }
    }

    void OnEnable()
    {
        // Subscribe to checkpoint change events
        CheckpointManager.OnCheckpointChanged += OnCheckpointChanged;
    }

    void OnDisable()
    {
        // Unsubscribe from checkpoint change events
        CheckpointManager.OnCheckpointChanged -= OnCheckpointChanged;
        
        // Stop spawning when disabled
        StopSpawning();
    }

    void InitializePool()
    {
        ballPool = new Queue<GameObject>();
        
        for (int i = 0; i < poolSize; i++)
        {
            GameObject ball = Instantiate(ballPrefab, spawnParent);
            ball.SetActive(false);
            ballPool.Enqueue(ball);
        }
    }

    GameObject GetBallFromPool()
    {
        if (ballPool.Count > 0)
        {
            GameObject ball = ballPool.Dequeue();
            ball.SetActive(true);
            return ball;
        }
        else if (canGrow)
        {
            GameObject ball = Instantiate(ballPrefab, spawnParent);
            return ball;
        }
        
        return null;
    }

    public void ReturnBallToPool(GameObject ball)
    {
        if (ball != null)
        {
            // Reset ball physics
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            ball.SetActive(false);
            ballPool.Enqueue(ball);
        }
    }

    // Called when checkpoint changes
    void OnCheckpointChanged(Checkpoint newCheckpoint)
    {
        currentActiveCheckpoint = newCheckpoint;
        
        // Check if this is a start checkpoint
        if (startCheckpoints.Contains(newCheckpoint))
        {
            StartSpawning();
        }
        // Check if this is an end checkpoint
        else if (endCheckpoints.Contains(newCheckpoint))
        {
            StopSpawning();
        }
    }

    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            spawnCoroutine = StartCoroutine(SpawnBalls());
        }
    }

    public void StopSpawning()
    {
        if (isSpawning)
        {
            isSpawning = false;
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
            }
        }
    }

    IEnumerator SpawnBalls()
    {
        while (isSpawning)
        {
            float spawnDelay = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(spawnDelay);
            
            SpawnBall();
        }
    }

    void SpawnBall()
    {
        GameObject ball = GetBallFromPool();
        if (ball == null) return;

        // Calculate spawn parameters based on spawner's position and scale
        Vector3 spawnPosition = CalculateSpawnPosition();
        ball.transform.position = spawnPosition;
        ball.transform.rotation = Quaternion.identity;

        // Apply random force
        Rigidbody rb = ball.GetComponent<Rigidbody>();

        // Reset the ball's lifetime tracking
        BallController ballController = ball.GetComponent<BallController>();
        if (ballController != null)
        {
            ballController.OnSpawn();
        }
    }

    Vector3 CalculateSpawnPosition()
    {
        // Use spawner's position as base
        Vector3 basePosition = transform.position;
        
        // Calculate horizontal range based on spawner's scale
        float halfWidth = transform.lossyScale.x * horizontalRangeMultiplier * 0.5f;
        float minX = basePosition.x - halfWidth;
        float maxX = basePosition.x + halfWidth;
        
        // Random horizontal position within range
        float spawnX = Random.Range(minX, maxX);
        
        // Random height between min and max offsets
        float heightOffset = Random.Range(minHeightOffset, maxHeightOffset);
        float spawnY = basePosition.y + heightOffset;
        
        // Keep Z same as spawner
        float spawnZ = basePosition.z;
        
        return new Vector3(spawnX, spawnY, spawnZ);
    }

    // Public method to spawn a ball immediately
    public void SpawnBallImmediate()
    {
        SpawnBall();
    }

    // Check if spawner should be active based on current checkpoint
    public bool ShouldBeActive()
    {
        if (CheckpointManager.Instance == null) return false;
        
        Checkpoint currentCheckpoint = CheckpointManager.Instance.GetCurrentCheckpoint();
        return currentCheckpoint != null && startCheckpoints.Contains(currentCheckpoint);
    }

    void OnDrawGizmosSelected()
    {
        // Visualize spawn area based on current position and scale
        if (!Application.isPlaying)
        {
            Vector3 basePosition = transform.position;
            Vector3 scale = transform.lossyScale;
            
            float halfWidth = scale.x * horizontalRangeMultiplier * 0.5f;
            float minX = basePosition.x - halfWidth;
            float maxX = basePosition.x + halfWidth;
            
            // Draw spawn height range
            float minSpawnY = basePosition.y + minHeightOffset;
            float maxSpawnY = basePosition.y + maxHeightOffset;
            
            // Draw horizontal lines at min and max heights
            Gizmos.color = Color.red;
            Vector3 leftMin = new Vector3(minX, minSpawnY, basePosition.z);
            Vector3 rightMin = new Vector3(maxX, minSpawnY, basePosition.z);
            Vector3 leftMax = new Vector3(minX, maxSpawnY, basePosition.z);
            Vector3 rightMax = new Vector3(maxX, maxSpawnY, basePosition.z);
            
            Gizmos.DrawLine(leftMin, rightMin);
            Gizmos.DrawLine(leftMax, rightMax);
            
            // Draw vertical lines to show range
            Gizmos.DrawLine(leftMin, leftMax);
            Gizmos.DrawLine(rightMin, rightMax);
            
            // Draw center line
            Gizmos.color = Color.yellow;
            Vector3 centerMin = new Vector3(basePosition.x, minSpawnY, basePosition.z);
            Vector3 centerMax = new Vector3(basePosition.x, maxSpawnY, basePosition.z);
            Gizmos.DrawLine(centerMin, centerMax);
            
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
}