using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(BoxCollider))]
public class DoorManager : MonoBehaviour
{
    // (DoorPair class is unchanged)
    [System.Serializable]
    public class DoorPair
    {
        public GameObject doorFrame;
        public Door leftDoor;
        public Door rightDoor;
        public QuestionData question;
        [HideInInspector] public bool hasBeenAnswered = false;

        public void Setup(QuestionData newQuestion, DoorManager manager)
        {
            this.question = newQuestion;
            this.hasBeenAnswered = false;
            
            leftDoor.isCorrect = newQuestion.correctAnswer;
            rightDoor.isCorrect = !newQuestion.correctAnswer;

            leftDoor.OnPlayerEnteredCorrect -= manager.HandleCorrectChoice;
            leftDoor.OnPlayerEnteredIncorrect -= manager.HandleIncorrectChoice;
            rightDoor.OnPlayerEnteredCorrect -= manager.HandleCorrectChoice;
            rightDoor.OnPlayerEnteredIncorrect -= manager.HandleIncorrectChoice;
            
            leftDoor.OnPlayerEnteredCorrect += manager.HandleCorrectChoice;
            leftDoor.OnPlayerEnteredIncorrect += manager.HandleIncorrectChoice;
            rightDoor.OnPlayerEnteredCorrect += manager.HandleCorrectChoice;
            rightDoor.OnPlayerEnteredIncorrect += manager.HandleIncorrectChoice;
            
            doorFrame.SetActive(true);
        }

        public void Deactivate()
        {
            hasBeenAnswered = true;
            if (doorFrame != null)
            {
                doorFrame.SetActive(false);
            }
        }
    }

    [Header("Door Settings")]
    public GameObject doorFramePrefab;
    public QuestionBank questionBank;
    public float movementSpeed = 5f;
    
    [Header("Slope Geometry")]
    public Transform slopeStart;
    public Transform slopeEnd;
    public float doorSpacing = 15f;
    
    [Header("Checkpoint Settings")]
    public List<Checkpoint> startCheckpoints = new List<Checkpoint>();
    public List<Checkpoint> endCheckpoints = new List<Checkpoint>();

    private List<DoorPair> doorPairs = new List<DoorPair>();
    private bool isChainActive = false;
    private int questionBankIndex = 0;
    private float slopeLength;
    private Vector3 slopeDirection;
    private bool isManagerActive = false;

    // NEW: This will store the question the player is currently inside of.
    private DoorPair currentQuestionPair;

    // --- Unity Methods (Awake, Start, Enable, Disable, Update) ---
    // These are mostly unchanged except for removing old trigger logic.
    void Awake()
    {
        // We no longer need the manager's main trigger for UI.
        // It can be disabled or used for something else.
        var mainTrigger = GetComponent<BoxCollider>();
        if (mainTrigger != null) mainTrigger.enabled = false;

        if (slopeStart == null || slopeEnd == null)
        {
            Debug.LogError("DoorManager needs a Slope Start and End transform.", this);
            enabled = false;
            return;
        }
        
        slopeDirection = (slopeEnd.position - slopeStart.position).normalized;
        slopeLength = Vector3.Distance(slopeStart.position, slopeEnd.position);

        PreSpawnDoors();
        SetAllDoorsActive(false); 
    }
    
    void Start()
    {
        if (CheckpointManager.Instance != null)
        {
            Checkpoint initialCheckpoint = CheckpointManager.Instance.GetCurrentCheckpoint();
            if (initialCheckpoint != null) OnCheckpointChanged(initialCheckpoint);
        }
    }

    void OnEnable() { CheckpointManager.OnCheckpointChanged += OnCheckpointChanged; }
    void OnDisable() { CheckpointManager.OnCheckpointChanged -= OnCheckpointChanged; }

    void Update()
    {
        if (isChainActive && isManagerActive)
        {
            MoveAndRecycleDoors();
        }
    }
    
    // --- Checkpoint and Activation Logic (Unchanged) ---
    void OnCheckpointChanged(Checkpoint newCheckpoint)
    {
        if (startCheckpoints.Contains(newCheckpoint))
        {
            isManagerActive = true;
            Debug.Log("DoorManager activated by start checkpoint.", this);
        }
        else if (endCheckpoints.Contains(newCheckpoint))
        {
            isManagerActive = false;
            DeactivateManager();
            Debug.Log("DoorManager deactivated by end checkpoint.", this);
        }
    }

    public void ActivateChain()
    {
        if (isChainActive || !isManagerActive) return;
        isChainActive = true;
        SetAllDoorsActive(true);
    }
    
    public void DeactivateManager()
    {
        isManagerActive = false;
        isChainActive = false;
        currentQuestionPair = null; // Clear the current question
        SetAllDoorsActive(false);
        if (QuestionUI.Instance != null) QuestionUI.Instance.HideQuestion();
    }
    
    // --- NEW: Methods to handle question display from individual triggers ---

    /// <summary>
    /// Called by DoorFrameTrigger when the player enters a door's trigger zone.
    /// </summary>
    public void SetCurrentQuestion(DoorPair pair)
    {
        // If we are already showing this question, do nothing.
        if (currentQuestionPair == pair) return;
        
        currentQuestionPair = pair;
        if (QuestionUI.Instance != null && !pair.hasBeenAnswered)
        {
            QuestionUI.Instance.ShowQuestion(pair.question);
        }
    }

    /// <summary>
    /// Called by DoorFrameTrigger when the player exits a door's trigger zone.
    /// </summary>
    public void ClearCurrentQuestion(DoorPair pair)
    {
        // Only clear the UI if the exiting door is the one we are currently displaying.
        // This prevents a fast player from clearing the UI for the *next* door.
        if (currentQuestionPair == pair)
        {
            currentQuestionPair = null;
            if (QuestionUI.Instance != null)
            {
                QuestionUI.Instance.HideQuestion();
            }
        }
    }

    // --- Event Handlers from Doors ---
    
    // MODIFIED: Added logic to clear the current question after it's answered.
    private void HandleCorrectChoice(Door door)
    {
        var answeredPair = doorPairs.FirstOrDefault(p => p.leftDoor == door || p.rightDoor == door);
        if (answeredPair != null && !answeredPair.hasBeenAnswered)
        {
            answeredPair.Deactivate();
            // Crucially, clear this question from the UI now that it's answered.
            ClearCurrentQuestion(answeredPair); 
        }
    }

    private void HandleIncorrectChoice(Door door)
    {
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RespawnToLastCheckpoint();
        }
    }

    // --- Spawning and Recycling Logic ---
    
    // MODIFIED: Added setup for the new DoorFrameTrigger script.
    private void PreSpawnDoors()
    {
        if (doorFramePrefab == null || questionBank == null || questionBank.questions.Count == 0) return;
        
        int doorCount = Mathf.CeilToInt(slopeLength / doorSpacing) + 3;

        for (int i = 0; i < doorCount; i++)
        {
            Vector3 spawnPos = slopeStart.position + (i * doorSpacing * slopeDirection);
            GameObject frameObj = Instantiate(doorFramePrefab, spawnPos, transform.rotation, transform);

            Door[] doorsInPrefab = frameObj.GetComponentsInChildren<Door>();
            if (doorsInPrefab.Length < 2) continue;

            var newPair = new DoorPair
            {
                doorFrame = frameObj,
                leftDoor = doorsInPrefab.OrderBy(d => d.transform.localPosition.x).First(),
                rightDoor = doorsInPrefab.OrderBy(d => d.transform.localPosition.x).Last(),
            };
            
            // NEW: Set up the trigger script on the spawned prefab instance.
            var trigger = frameObj.GetComponent<DoorFrameTrigger>();
            if (trigger != null)
            {
                trigger.manager = this;
                trigger.parentPair = newPair;
            }
            else
            {
                Debug.LogError("The doorFramePrefab is missing the DoorFrameTrigger script!", doorFramePrefab);
            }
            
            newPair.Setup(GetNextQuestion(), this);
            AlignDoorToSlope(newPair);
            doorPairs.Add(newPair);
        }
    }
    
    // MODIFIED: On recycle, we no longer need to call UpdateQuestionUI.
    private void MoveAndRecycleDoors()
    {
        foreach (var pair in doorPairs)
        {
            if (pair.doorFrame != null)
            {
                pair.doorFrame.transform.position += slopeDirection * movementSpeed * Time.deltaTime;
                
                float distance = Vector3.Dot(pair.doorFrame.transform.position - slopeStart.position, slopeDirection);
                if (distance > slopeLength + doorSpacing)
                {
                    // If the recycled door was the currently displayed question, hide the UI.
                    if (currentQuestionPair == pair) ClearCurrentQuestion(pair);

                    var firstDoor = doorPairs.OrderBy(p => Vector3.Dot(p.doorFrame.transform.position - slopeStart.position, slopeDirection)).First();
                    pair.doorFrame.transform.position = firstDoor.doorFrame.transform.position - slopeDirection * doorSpacing;

                    pair.Setup(GetNextQuestion(), this);
                    AlignDoorToSlope(pair);
                    // We no longer call UpdateQuestionUI() here, preventing the race condition.
                }
            }
        }
    }
    
    // --- Helper Methods ---
    private void SetAllDoorsActive(bool active) => doorPairs.ForEach(p => p.doorFrame?.SetActive(active));
    private QuestionData GetNextQuestion()
    {
        QuestionData q = questionBank.questions[questionBankIndex];
        questionBankIndex = (questionBankIndex + 1) % questionBank.questions.Count;
        return q;
    }
    
    private void AlignDoorToSlope(DoorPair pair)
    {
        pair.doorFrame.transform.rotation = Quaternion.LookRotation(-slopeDirection);
    }
}