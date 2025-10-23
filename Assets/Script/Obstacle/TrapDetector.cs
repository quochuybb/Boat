using UnityEngine;

public class TrapDetector : MonoBehaviour
{
    [Header("Trap Settings")]
    public string trapTag = "Trap";
    public bool useTriggerDetection = true;  // Use OnTriggerEnter
    public bool useCollisionDetection = false; // Use OnCollisionEnter (for non-trigger traps)

    void OnTriggerEnter(Collider other)
    {
        if (useTriggerDetection && other.CompareTag(trapTag))
        {
            TriggerTrapResponse();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (useCollisionDetection && collision.gameObject.CompareTag(trapTag))
        {
            TriggerTrapResponse();
            Debug.Log("hit trap");
        }
    }

    void TriggerTrapResponse()
    {
        if (CheckpointManager.Instance != null)
        {
            // FIX: Changed OnPlayerTrapTrigger() to the correct method RespawnToLastCheckpoint()
            CheckpointManager.Instance.RespawnToLastCheckpoint();
        }
    }

    // Optional: Method to manually trigger trap response
    public void ManualTrapTrigger()
    {
        TriggerTrapResponse();
    }
}