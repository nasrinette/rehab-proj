using UnityEngine;
using UnityEngine.UI;

public class VRButtonTest : MonoBehaviour
{
    public Button myButton;

    void Start()
    {
        if (myButton != null)
        {
            myButton.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogError("Button not assigned in inspector.");
        }
    }

    void OnButtonClicked()
    {
        Debug.Log("VR Button was clicked!");
    }
}
