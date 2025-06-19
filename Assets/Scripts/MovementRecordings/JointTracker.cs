using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;

public class JointTracker : MonoBehaviour
{
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public Transform entityGO;

    public string csvFilePath;
    public string exerciseName = "JointPositions";

    public bool startRecording = true; // Automatically start recording on Start

    private Coroutine loggingCoroutine;
    private float timeStamp = 0f;

    void Start()
    {
        //InitializeCsvFile();
        StartCoroutine(AssignJointsAfterDelay());
        if (startRecording)
        {
            StartRecording();
        }
    }
    private void FixedUpdate()
    {
        timeStamp += Time.fixedDeltaTime;
    }

    private void InitializeCsvFile()
    {
        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "RehabProject", "MovementRecordings");

        // Ensure the directory exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        csvFilePath = Path.Combine(folderPath, exerciseName + ".csv");
        WriteCsvHeader();
    }

    private IEnumerator AssignJointsAfterDelay()
    {
        yield return new WaitForSeconds(5f);

        if (head == null) head = entityGO.Find("Joint Head");
        if (leftHand == null) leftHand = entityGO.Find("Joint LeftHandWrist");
        if (rightHand == null) rightHand = entityGO.Find("Joint RightHandWrist");
    }

    private void WriteCsvHeader()
    {
        // If file exists, delete it
        if (File.Exists(csvFilePath))
        {
            File.Delete(csvFilePath);
            Debug.LogWarning("Existing CSV file deleted: " + csvFilePath);
        }
        if (!File.Exists(csvFilePath))
        {
            var header = "Time," +
                "HeadX,HeadY,HeadZ,HeadRotX,HeadRotY,HeadRotZ," +
                "LeftHandX,LeftHandY,LeftHandZ,LeftHandRotX,LeftHandRotY,LeftHandRotZ," +
                "RightHandX,RightHandY,RightHandZ,RightHandRotX,RightHandRotY,RightHandRotZ";
            File.WriteAllText(csvFilePath, header + "\n");
        }
    }

    private IEnumerator LogJointsPeriodically()
    {
        // Wait until joints are assigned
        while (head == null || leftHand == null || rightHand == null)
            yield return null;

        while (true)
        {
            Vector3 headRot = head.rotation.eulerAngles;
            Vector3 leftHandRot = leftHand.rotation.eulerAngles;
            Vector3 rightHandRot = rightHand.rotation.eulerAngles;

            var line = string.Format("{0}," +
                "{1},{2},{3},{4},{5},{6}," +
                "{7},{8},{9},{10},{11},{12}," +
                "{13},{14},{15},{16},{17},{18}",
                timeStamp,
                head.position.x, head.position.y, head.position.z, headRot.x, headRot.y, headRot.z,
                leftHand.position.x, leftHand.position.y, leftHand.position.z, leftHandRot.x, leftHandRot.y, leftHandRot.z,
                rightHand.position.x, rightHand.position.y, rightHand.position.z, rightHandRot.x, rightHandRot.y, rightHandRot.z
            );
            File.AppendAllText(csvFilePath, line + "\n");

            yield return new WaitForSeconds(0.2f);
        }
    }

    // Public function to start recording
    public void StartRecording()
    {
        timeStamp = 0f; 
        InitializeCsvFile();
        if (loggingCoroutine == null)
        {
            loggingCoroutine = StartCoroutine(LogJointsPeriodically());
        } else
        {
            Debug.LogWarning("Recording is already in progress. Please stop the current recording before starting a new one.");
        }
    }

    // Public function to stop recording
    public void StopRecording()
    {
        if (loggingCoroutine != null)
        {
            StopCoroutine(loggingCoroutine);
            loggingCoroutine = null;
        } else
        {
            Debug.LogWarning("No recording is in progress to stop.");
        }
    }

    // Public function to change the exercise name and reset the CSV file
    public void NewExercise(string newExerciseName, bool patient = false)
    {
        if(loggingCoroutine != null)
        {
            StopRecording(); // Stop current recording if it's in progress
        }
        exerciseName = newExerciseName;
        exerciseName = (patient ? "Patient_" : "Doctor_") + exerciseName;
        //InitializeCsvFile();
    }
}
