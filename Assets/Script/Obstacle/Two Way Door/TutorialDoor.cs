using UnityEngine;
using System.Linq;

[RequireComponent(typeof(BoxCollider))]
public class TutorialDoor : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [Tooltip("The question for this tutorial door.")]
    public QuestionBank tutorialQuestionBank;
    
    [Header("Activation")]
    [Tooltip("The Door Manager to activate after this tutorial is successfully completed.")]
    public DoorManager doorManagerToActivate;

    private Door leftDoor;
    private Door rightDoor;
    private QuestionData question;

    private void Awake()
    {
        // Ensure the root collider is a trigger for detecting the player
        var trigger = GetComponent<BoxCollider>();
        if (trigger != null) trigger.isTrigger = true;
        else Debug.LogError("TutorialDoor requires a BoxCollider on its root object to function as a trigger.", this);

        SetupDoors();
    }

    private void SetupDoors()
    {
        if (tutorialQuestionBank == null || tutorialQuestionBank.questions.Count == 0)
        {
            Debug.LogError("Tutorial Door requires a Question Bank with at least one question.", this);
            gameObject.SetActive(false);
            return;
        }
        question = tutorialQuestionBank.questions[0];

        Door[] doors = GetComponentsInChildren<Door>();
        if (doors.Length < 2)
        {
            Debug.LogError("TutorialDoor object needs two child objects with the Door component.", this);
            return;
        }

        // Reliably find left/right doors based on their local X position
        leftDoor = doors.OrderBy(d => d.transform.localPosition.x).First();
        rightDoor = doors.OrderBy(d => d.transform.localPosition.x).Last();

        leftDoor.isCorrect = question.correctAnswer;
        rightDoor.isCorrect = !question.correctAnswer;

        // Subscribe to events on both doors
        leftDoor.OnPlayerEnteredCorrect += HandleCorrectChoice;
        rightDoor.OnPlayerEnteredCorrect += HandleCorrectChoice;
        leftDoor.OnPlayerEnteredIncorrect += HandleIncorrectChoice;
        rightDoor.OnPlayerEnteredIncorrect += HandleIncorrectChoice;
    }

    private void HandleCorrectChoice(Door door)
    {
        Debug.Log("Tutorial completed successfully!");
        
        if (QuestionUI.Instance != null)
        {
            QuestionUI.Instance.HideQuestion();
        }

        if (doorManagerToActivate != null)
        {
            doorManagerToActivate.ActivateChain();
        }
        else
        {
            Debug.LogWarning("No DoorManager assigned to activate after the tutorial.", this);
        }

        // Deactivate this tutorial so it doesn't trigger again
        gameObject.SetActive(false);
    }

    private void HandleIncorrectChoice(Door door)
    {
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RespawnToLastCheckpoint();
        }
    }
    
    // --- UI Triggers ---

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (QuestionUI.Instance != null)
            {
                QuestionUI.Instance.ShowQuestion(question);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (QuestionUI.Instance != null)
            {
                QuestionUI.Instance.HideQuestion();
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up event subscriptions to prevent memory leaks
        if (leftDoor != null)
        {
            leftDoor.OnPlayerEnteredCorrect -= HandleCorrectChoice;
            leftDoor.OnPlayerEnteredIncorrect -= HandleIncorrectChoice;
        }
        if (rightDoor != null)
        {
            rightDoor.OnPlayerEnteredCorrect -= HandleCorrectChoice;
            rightDoor.OnPlayerEnteredIncorrect -= HandleIncorrectChoice;
        }
    }
}