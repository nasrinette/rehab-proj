using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class RecordingUIController : MonoBehaviour
{
    public GameObject joinTracker; // Reference to your JoinTracker script or GameObject


    private void Update()
    {
        // Only show UI if joinTracker is enabled
        if (joinTracker != null)
            gameObject.SetActive(joinTracker.activeInHierarchy);


        // Map A (Button.One) to Start, B (Button.Two) to Stop on right controller
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            StartRecording();
        }
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
        {
            StopRecording();
        }

    }

    private void StartRecording()
    {
        Debug.Log("Start Recording");
        // TODO: Call your recording logic here
    }

    private void StopRecording()
    {
        Debug.Log("Stop Recording");
        // TODO: Call your stop recording logic here
    }
}

