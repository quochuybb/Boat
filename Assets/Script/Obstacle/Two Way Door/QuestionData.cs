using System;
using UnityEngine;

// This attribute allows the class to be shown in the Unity Inspector
[Serializable]
public class QuestionData
{
    [TextArea(2, 5)] // Makes the question field a larger, multi-line text area in the Inspector
    public string questionText;

    [Header("Options")] // Organizes the Inspector view
    [Tooltip("The first option (e.g., True, Option A)")]
    public string option1;
    [Tooltip("The second option (e.g., False, Option B)")]
    public string option2;

    [Header("Answer")]
    [Tooltip("True if option1 is the correct answer. If false, option2 is correct.")]
    public bool correctAnswer; 
}