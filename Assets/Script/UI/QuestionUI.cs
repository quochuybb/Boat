using UnityEngine;
using TMPro;

public class QuestionUI : MonoBehaviour
{
    public static QuestionUI Instance;

    [Header("UI Elements")]
    public GameObject uiPanel;
    public TMP_Text questionText;
    public TMP_Text leftOptionText;
    public TMP_Text rightOptionText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (uiPanel != null)
            uiPanel.SetActive(false);
    }

    /// <summary>
    /// Displays a question on the UI panel.
    /// </summary>
    public void ShowQuestion(QuestionData questionData)
    {
        if (uiPanel == null || questionData == null) return;
        
        questionText.text = questionData.questionText;
        leftOptionText.text = questionData.option1;
        rightOptionText.text = questionData.option2;

        uiPanel.SetActive(true);
    }

    /// <summary>
    /// Hides the question UI panel.
    /// </summary>
    public void HideQuestion()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }
    }
}