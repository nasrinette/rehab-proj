using UnityEngine;

/// <summary>
/// This script is now OBSOLETE when using HandRayInteractor.
/// Keep it disabled or remove it entirely.
/// 
/// The UserSelector buttons now work through ray-based interaction
/// with pinch gestures instead of controller buttons.
/// </summary>
[System.Obsolete("Use HandRayInteractor instead for ray-based UI interaction")]
public class ControllerInputHandler : MonoBehaviour
{
    public UserSelector userSelector;

    void Start()
    {
        Debug.LogWarning("ControllerInputHandler is obsolete. Use HandRayInteractor for ray-based UI interaction.");

        // Disable this component to prevent conflicts
        this.enabled = false;
    }

    void Update()
    {
        // This method is no longer used
    }
}