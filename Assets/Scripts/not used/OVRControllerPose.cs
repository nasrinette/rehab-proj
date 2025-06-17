using UnityEngine;

public class OVRControllerPose : MonoBehaviour
{
    public OVRInput.Controller hand = OVRInput.Controller.RTouch;

    void Update()
    {
        transform.localPosition = OVRInput.GetLocalControllerPosition(hand);
        transform.localRotation = OVRInput.GetLocalControllerRotation(hand);
    }
}
