using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }

    [Header("Input Actions")]
    public PlayerInput playerInput;
    
    // Input Actions
    private InputAction moveAction;
    private InputAction strafeAction;
    private InputAction turnAction;
    private InputAction cameraDistanceUpAction;
    private InputAction cameraDistanceDownAction;
    private InputAction cameraHeightUpAction;
    private InputAction cameraHeightDownAction;

    // Input Values
    public Vector2 MoveInput { get; private set; } = Vector2.zero;
    public float StrafeInput { get; private set; } = 0f;
    public float TurnInput { get; private set; } = 0f;
    
    // Camera Control States
    public bool CameraDistanceUpPressed { get; private set; } = false;
    public bool CameraDistanceDownPressed { get; private set; } = false;
    public bool CameraHeightUpPressed { get; private set; } = false;
    public bool CameraHeightDownPressed { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }

        if (playerInput != null)
        {
            // Get input actions from the PlayerInput component
            moveAction = playerInput.actions["Move"];
            strafeAction = playerInput.actions["Strafe"];
            turnAction = playerInput.actions["Turn"];
            cameraDistanceUpAction = playerInput.actions["CameraDistanceUp"];
            cameraDistanceDownAction = playerInput.actions["CameraDistanceDown"];
            cameraHeightUpAction = playerInput.actions["CameraHeightUp"];
            cameraHeightDownAction = playerInput.actions["CameraHeightDown"];
        }
    }

    private void Update()
    {
        if (playerInput != null)
        {
            // Read input values every frame
            MoveInput = moveAction.ReadValue<Vector2>();
            StrafeInput = strafeAction.ReadValue<float>();
            TurnInput = turnAction.ReadValue<float>();
            
            // Read camera control inputs (check if buttons are currently held)
            CameraDistanceUpPressed = cameraDistanceUpAction.IsPressed();
            CameraDistanceDownPressed = cameraDistanceDownAction.IsPressed();
            CameraHeightUpPressed = cameraHeightUpAction.IsPressed();
            CameraHeightDownPressed = cameraHeightDownAction.IsPressed();
        }
    }

    // Public methods for other scripts to get input values
    public float GetForwardInput() => -MoveInput.y; // W/S inverted
    public float GetStrafeInput() => StrafeInput;
    public float GetTurnInput() => TurnInput;

    // Camera control methods - check if buttons are held
    public bool IsCameraDistanceUpHeld() => CameraDistanceUpPressed;
    public bool IsCameraDistanceDownHeld() => CameraDistanceDownPressed;
    public bool IsCameraHeightUpHeld() => CameraHeightUpPressed;
    public bool IsCameraHeightDownHeld() => CameraHeightDownPressed;

    // Alternative method using direct action reading (if needed)
    public Vector2 GetMoveVector()
    {
        return MoveInput;
    }
}