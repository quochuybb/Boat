using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Ball Settings")]
    public float despawnDepth = 200f; // Distance below spawner before despawn
    public float lifetime = 30f;     // Maximum lifetime before forced despawn
    public float forwardForce = 10f;
    private float spawnTime;
    private BallSpawner spawner;
    private float spawnerYPosition;
    private Rigidbody ballRigidbody;
    private bool isActive = false;


    void Start()
    {
        ballRigidbody = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        ballRigidbody.AddForce(Vector3.back * forwardForce, ForceMode.Force);
    }
    public void OnSpawn()
    {
        // Reset all ball state when spawned from pool
        spawnTime = Time.time;
        isActive = true;
        
        if (ballRigidbody != null)
        {
            ballRigidbody.linearVelocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
        }
        
        // Find the nearest spawner
        spawner = FindNearestSpawner();
        if (spawner != null)
        {
            spawnerYPosition = spawner.transform.position.y;
        }
    }

    BallSpawner FindNearestSpawner()
    {
        BallSpawner[] spawners = FindObjectsOfType<BallSpawner>();
        BallSpawner nearest = null;
        float minDistance = Mathf.Infinity;
        
        foreach (BallSpawner s in spawners)
        {
            float distance = Vector3.Distance(transform.position, s.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = s;
            }
        }
        
        return nearest;
    }

    void Update()
    {
        if (!isActive) return; // Don't update if not active

        // Despawn if below spawner threshold
        if (spawner != null && transform.position.y < (spawnerYPosition - despawnDepth))
        {
            ReturnToPool();
            return;
        }
        
        // Despawn if lifetime exceeded
        if (Time.time - spawnTime > lifetime)
        {
            ReturnToPool();
            return;
        }
    }

    void ReturnToPool()
    {
        isActive = false; // Mark as inactive
        
        if (spawner != null)
        {
            spawner.ReturnBallToPool(gameObject);
        }
        else
        {
            // If no spawner found, just disable the ball
            gameObject.SetActive(false);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Optional: Add effects when ball hits something
    }

    // Called when ball is enabled (from pool)
    void OnEnable()
    {
        // Note: OnSpawn() should be called separately by the spawner
        // This just ensures we don't have any leftover state
    }

    // Called when ball is disabled (returned to pool)
    void OnDisable()
    {
        // Clean up any ongoing processes
        isActive = false;
    }
}