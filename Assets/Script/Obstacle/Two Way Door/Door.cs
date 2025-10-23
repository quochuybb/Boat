using UnityEngine;
using System;

public class Door : MonoBehaviour
{
    public bool isCorrect = false;

    // Events for other scripts to subscribe to
    public event Action<Door> OnPlayerEnteredCorrect;
    public event Action<Door> OnPlayerEnteredIncorrect;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isCorrect)
            {
                Debug.Log("Correct door chosen!");
                // Invoke the event for subscribers to handle.
                OnPlayerEnteredCorrect?.Invoke(this); 
            }
            else
            {
                Debug.Log("Wrong door chosen!");
                // Invoke the event for subscribers to handle.
                OnPlayerEnteredIncorrect?.Invoke(this); 
            }
        }
    }
}