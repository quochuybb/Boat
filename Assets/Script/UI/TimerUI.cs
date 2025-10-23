using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    public TMP_Text timerText;

    void Update()
    {
        if (CheckpointManager.Instance != null)
        {
            float timeLeft = CheckpointManager.Instance.GetCurrentTime();
            timerText.text = Mathf.CeilToInt(timeLeft).ToString();
        }
    }
}