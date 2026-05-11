using UnityEngine;
using UnityEngine.InputSystem;

public class SecondSight : MonoBehaviour
{
    private bool isActive = false;
    private bool isUnlocked = false;

    public static SecondSight Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (!isUnlocked) return;

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            if (isActive)
                Deactivate();
            else
                Activate();
        }
    }

    public void UnlockAbility()
    {
        isUnlocked = true;
    }

    void Activate()
    {
        isActive = true;

        RevealableObject[] objects = FindObjectsOfType<RevealableObject>();
        foreach (var obj in objects)
        {
            obj.OnSecondSightActivate();
        }

        Debug.Log("Альтернативное зрение включено");
    }

    void Deactivate()
    {
        isActive = false;

        RevealableObject[] objects = FindObjectsOfType<RevealableObject>();
        foreach (var obj in objects)
        {
            obj.OnSecondSightDeactivate();
        }

        Debug.Log("Альтернативное зрение выключено");
    }

    public bool IsVisionActive()
    {
        return isActive;
    }

    public bool IsUnlocked()
    {
        return isUnlocked;
    }
}