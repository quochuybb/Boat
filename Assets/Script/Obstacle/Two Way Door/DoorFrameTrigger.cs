using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DoorFrameTrigger : MonoBehaviour
{
    // These will be assigned by the DoorManager when the door is spawned.
    public DoorManager manager;
    public DoorManager.DoorPair parentPair;

    void Start()
    {
        // Ensure the collider is a trigger.
        var col = GetComponent<BoxCollider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogError("DoorFrameTrigger requires a BoxCollider component on the same GameObject.", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && manager != null && parentPair != null)
        {
            // Tell the manager that this is now the active question.
            manager.SetCurrentQuestion(parentPair);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && manager != null && parentPair != null)
        {
            // Tell the manager to clear this question IF it's still the active one.
            manager.ClearCurrentQuestion(parentPair);
        }
    }
}