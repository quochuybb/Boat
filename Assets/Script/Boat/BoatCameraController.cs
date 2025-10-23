using UnityEngine;

public class BoatCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform boatTransform; 
    
    [Header("Distance Settings")]
    public float followDistance = 10f; 
    public float minDistance = 1f;
    public float maxDistance = 30f;
    public float distanceChangeSpeed = 5f; // How fast distance changes when holding button
    
    [Header("Height Settings")]
    public float followHeight = 5f;   
    public float minHeight = 2f;
    public float maxHeight = 20f;
    public float heightChangeSpeed = 5f; // How fast height changes when holding button
    
    [Header("Movement Settings")]
    public float followSpeed = 5f;    
    public float followSmoothness = 0.3f; 
    
    [Header("FOV Settings")]
    public float minFOV = 40f; 
    public float maxFOV = 60f; 
    public float fovChangeSpeed = 2f; 

    // Target values for smooth interpolation
    private float targetDistance;
    private float targetHeight;
    private Rigidbody boatRigidbody;

    void Start()
    {
        boatRigidbody = boatTransform.GetComponent<Rigidbody>();
        
        // Initialize target values
        targetDistance = Mathf.Clamp(followDistance, minDistance, maxDistance);
        targetHeight = Mathf.Clamp(followHeight, minHeight, maxHeight);
    }

    void Update()
    {
        if (boatTransform == null || boatRigidbody == null) return;

        HandleCameraControls();
        UpdateCameraPosition();
        UpdateFOV();
    }

    void HandleCameraControls()
    {
        float distanceChange = distanceChangeSpeed * Time.deltaTime;
        float heightChange = heightChangeSpeed * Time.deltaTime;

        if (InputHandler.Instance != null)
        {
            // Camera Distance Controls - hold to adjust
            if (InputHandler.Instance.IsCameraDistanceUpHeld())
            {
                targetDistance = Mathf.Clamp(targetDistance + distanceChange, minDistance, maxDistance);
            }
            if (InputHandler.Instance.IsCameraDistanceDownHeld())
            {
                targetDistance = Mathf.Clamp(targetDistance - distanceChange, minDistance, maxDistance);
            }

            // Camera Height Controls - hold to adjust
            if (InputHandler.Instance.IsCameraHeightUpHeld())
            {
                targetHeight = Mathf.Clamp(targetHeight + heightChange, minHeight, maxHeight);
            }
            if (InputHandler.Instance.IsCameraHeightDownHeld())
            {
                targetHeight = Mathf.Clamp(targetHeight - heightChange, minHeight, maxHeight);
            }
        }
        else
        {
            // Fallback to keyboard controls
            if (Input.GetKey(KeyCode.PageUp)) // Increase distance
            {
                targetDistance = Mathf.Clamp(targetDistance + distanceChange, minDistance, maxDistance);
            }
            if (Input.GetKey(KeyCode.PageDown)) // Decrease distance
            {
                targetDistance = Mathf.Clamp(targetDistance - distanceChange, minDistance, maxDistance);
            }
            if (Input.GetKey(KeyCode.Home)) // Increase height
            {
                targetHeight = Mathf.Clamp(targetHeight + heightChange, minHeight, maxHeight);
            }
            if (Input.GetKey(KeyCode.End)) // Decrease height
            {
                targetHeight = Mathf.Clamp(targetHeight - heightChange, minHeight, maxHeight);
            }
        }

        // Smoothly interpolate to target values
        followDistance = Mathf.Lerp(followDistance, targetDistance, 5f * Time.deltaTime);
        followHeight = Mathf.Lerp(followHeight, targetHeight, 5f * Time.deltaTime);
    }

    void UpdateCameraPosition()
    {
        Vector3 boatForward = -Vector3.ProjectOnPlane(boatTransform.right, Vector3.up).normalized;
        Vector3 targetPosition = boatTransform.position - boatForward * followDistance + Vector3.up * followHeight;

        transform.position = Vector3.Lerp(transform.position, targetPosition, followSmoothness * Time.deltaTime * followSpeed);

        transform.LookAt(boatTransform.position);
    }

    void UpdateFOV()
    {
        float speed = boatRigidbody.linearVelocity.magnitude;
        float maxSpeed = 15f;
        float speedRatio = Mathf.Clamp01(speed / maxSpeed);

        Camera camera = GetComponent<Camera>();
        if (camera != null)
        {
            float targetFOV = Mathf.Lerp(maxFOV, minFOV, speedRatio);
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, targetFOV, fovChangeSpeed * Time.deltaTime);
        }
    }

    // Public methods for UI or other scripts to adjust camera
    public void SetDistance(float distance)
    {
        targetDistance = Mathf.Clamp(distance, minDistance, maxDistance);
    }
    
    public void SetHeight(float height)
    {
        targetHeight = Mathf.Clamp(height, minHeight, maxHeight);
    }
    
    public void IncreaseDistance(float amount = 1f)
    {
        targetDistance = Mathf.Clamp(targetDistance + amount, minDistance, maxDistance);
    }
    
    public void DecreaseDistance(float amount = 1f)
    {
        targetDistance = Mathf.Clamp(targetDistance - amount, minDistance, maxDistance);
    }
    
    public void IncreaseHeight(float amount = 1f)
    {
        targetHeight = Mathf.Clamp(targetHeight + amount, minHeight, maxHeight);
    }
    
    public void DecreaseHeight(float amount = 1f)
    {
        targetHeight = Mathf.Clamp(targetHeight - amount, minHeight, maxHeight);
    }

    // Get current values for UI display
    public float GetDistance() => followDistance;
    public float GetHeight() => followHeight;
    public float GetTargetDistance() => targetDistance;
    public float GetTargetHeight() => targetHeight;
}