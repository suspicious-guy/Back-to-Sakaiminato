using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Обучение")]
    public TutorialManager tutorialManager;

    [Header("Настройки")]
    public bool oneTimeOnly = true;
    public float enterDelay = 0.5f;

    private bool used = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !used && tutorialManager != null)
        {
            used = oneTimeOnly;
            Invoke(nameof(StartTutorial), enterDelay);
        }
    }

    void StartTutorial()
    {
        tutorialManager.StartTutorial();
    }
}