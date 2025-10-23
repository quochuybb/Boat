using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [Tooltip("The position and rotation where the player will respawn.")]
    public Transform spawnPoint;
    [Tooltip("Time limit in seconds to reach the next checkpoint from this one.")]
    public float timeLimit = 30f;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.SetCheckpoint(this);
                // Deactivate so it's not triggered repeatedly.
                gameObject.SetActive(false); 
            }
        }
    }

    // Helper for easier setup in the editor
    private void OnValidate()
    {
        if (spawnPoint == null)
            spawnPoint = transform;
    }
}