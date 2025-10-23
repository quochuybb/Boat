using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuestionBank", menuName = "Quiz/Question Bank")]
public class QuestionBank : ScriptableObject
{
    public List<QuestionData> questions = new List<QuestionData>();
}