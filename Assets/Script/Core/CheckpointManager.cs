using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    public static System.Action<Checkpoint> OnCheckpointChanged;

    [Header("Player Settings")]
    public GameObject player;
    public float fallThreshold = -10f;

    [Header("Checkpoints")]
    public List<Checkpoint> allCheckpoints = new List<Checkpoint>();
    private Checkpoint lastCheckpoint;
    
    private float currentTime;
    private bool isTimerRunning = false;
    private bool isRespawning = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (allCheckpoints.Count > 0)
        {
            SetCheckpoint(allCheckpoints[0]);
        }
    }

    private void Update()
    {
        if (isRespawning) return;

        if (isTimerRunning)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
                RespawnToLastCheckpoint();
        }

        if (player.transform.position.y < fallThreshold)
            RespawnToLastCheckpoint();
    }

    public void SetCheckpoint(Checkpoint checkpoint)
    {
        Checkpoint previousCheckpoint = lastCheckpoint;
        lastCheckpoint = checkpoint;
        currentTime = checkpoint.timeLimit;
        isTimerRunning = true;

        // Note: Checkpoint script now deactivates itself
        // checkpoint.gameObject.SetActive(false); 
        
        if (OnCheckpointChanged != null && previousCheckpoint != checkpoint)
        {
            OnCheckpointChanged(checkpoint);
        }
    }

    public void RespawnToLastCheckpoint()
    {
        if (lastCheckpoint == null || isRespawning) return;
        StartCoroutine(RespawnCoroutine());
    }
    
    private IEnumerator RespawnCoroutine()
    {
        isRespawning = true;
        
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.isKinematic = true;
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }

        player.transform.position = lastCheckpoint.spawnPoint.position;
        player.transform.rotation = lastCheckpoint.spawnPoint.rotation;
        
        yield return new WaitForSeconds(1.5f);

        if (playerRb != null)
        {
            playerRb.isKinematic = false;
        }
        
        currentTime = lastCheckpoint.timeLimit;
        isTimerRunning = true;
        isRespawning = false;
        
        OnCheckpointChanged?.Invoke(lastCheckpoint);
    }

    // Accessors
    public float GetCurrentTime() => currentTime;
    public bool IsTimerRunning() => isTimerRunning;
    public Checkpoint GetCurrentCheckpoint() => lastCheckpoint;
}