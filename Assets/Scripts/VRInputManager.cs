using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRInputManager : MonoBehaviour
{
    [SerializeField] private Scrollbar uiScrollbar; // Assign the UI Scrollbar in the Inspector
    [SerializeField] private string joystickYAxis = "Oculus_CrossPlatform_SecondaryThumbstickVertical"; // Input axis for the right-hand joystick Y-axis
    [SerializeField] private float scrollSpeed = 0.1f; // Speed at which the scrollbar value changes

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (uiScrollbar != null)
        {
            // Get the joystick's Y-axis input
            float joystickInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch).y;
            //Debug.Log("Joystick Y Input: " + OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch) + "jhgvjng"+ OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick, OVRInput.Controller.RTouch));
            // Adjust the scrollbar value based on the joystick input
            uiScrollbar.value = Mathf.Clamp01(uiScrollbar.value + joystickInput * scrollSpeed * Time.deltaTime);
            Debug.Log(uiScrollbar.value +"Scrollbar Value: " + uiScrollbar.value + joystickInput * scrollSpeed * Time.deltaTime);
        }
    }
}
