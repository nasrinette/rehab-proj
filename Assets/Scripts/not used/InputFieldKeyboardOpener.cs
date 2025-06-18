using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputFieldKeyboardOpener : MonoBehaviour
{
    private InputField unityInputField;
    private TMP_InputField tmpInputField;
    private TouchScreenKeyboard keyboard;

    void Start()
    {
        // Find the first InputField or TMP_InputField in the scene
        unityInputField = FindObjectOfType<InputField>();
        if (unityInputField == null)
        {
            tmpInputField = FindObjectOfType<TMP_InputField>();
        }
    }

    void Update()
    {
        // Detect B button press on the right Oculus Touch controller
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
        {
            OpenKeyboard();
        }
    }

    private void OpenKeyboard()
    {
        if (unityInputField != null)
        {
            unityInputField.Select();
            keyboard = TouchScreenKeyboard.Open(unityInputField.text, TouchScreenKeyboardType.Default);
        }
        else if (tmpInputField != null)
        {
            tmpInputField.Select();
            keyboard = TouchScreenKeyboard.Open(tmpInputField.text, TouchScreenKeyboardType.Default);
        }
    }
}
