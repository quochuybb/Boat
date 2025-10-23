// using UnityEngine;

// public class QuestionTrigger : MonoBehaviour
// {
//     [Header("Question Source")]
//     public DoorManager doorManager;
//     public TutorialDoor tutorialDoor;
    
//     [Header("Settings")]
//     public bool isTutorialTrigger = false;

//     void OnTriggerEnter(Collider other)
//     {
//         if (other.CompareTag("Player"))
//         {
//             if (QuestionUI.Instance != null)
//             {
//                 if (isTutorialTrigger && tutorialDoor != null)
//                 {
//                     QuestionUI.Instance.SetCurrentTutorialDoor(tutorialDoor);
//                 }
//                 else if (!isTutorialTrigger && doorManager != null)
//                 {
//                     QuestionUI.Instance.SetCurrentDoorManager(doorManager);
//                 }
//             }
//         }
//     }

//     void OnTriggerExit(Collider other)
//     {
//         if (other.CompareTag("Player"))
//         {
//             if (QuestionUI.Instance != null)
//             {
//                 if (isTutorialTrigger && QuestionUI.Instance.currentTutorialDoor == tutorialDoor)
//                 {
//                     QuestionUI.Instance.ClearCurrentTutorialDoor();
//                 }
//                 else if (!isTutorialTrigger && QuestionUI.Instance.currentDoorManager == doorManager)
//                 {
//                     QuestionUI.Instance.ClearCurrentDoorManager();
//                 }
//             }
//         }
//     }
// }