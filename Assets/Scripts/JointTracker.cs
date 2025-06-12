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

    public string csvFilePath;
    public string exerciseName = "JointPositions";

    void Start()
    {
        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "RehabProject", "Recordings");

        // Ensure the directory exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        csvFilePath = Path.Combine(folderPath, exerciseName + ".csv");
        WriteCsvHeader();
        StartCoroutine(AssignJointsAfterDelay());
        StartCoroutine(LogJointsPeriodically());
    }


    void Update()
    {
    }

    private IEnumerator AssignJointsAfterDelay()
    {
        yield return new WaitForSeconds(5f);

        if (head == null) head = transform.Find("Joint Head");
        if (leftHand == null) leftHand = transform.Find("Joint LeftHandWrist");
        if (rightHand == null) rightHand = transform.Find("Joint RightHandWrist");

    }


    private void WriteCsvHeader()
    {
        //if file exists delete it
        if (File.Exists(csvFilePath))
        {
            File.Delete(csvFilePath);
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
                Time.time,
                head.position.x, head.position.y, head.position.z, headRot.x, headRot.y, headRot.z,
                leftHand.position.x, leftHand.position.y, leftHand.position.z, leftHandRot.x, leftHandRot.y, leftHandRot.z,
                rightHand.position.x, rightHand.position.y, rightHand.position.z, rightHandRot.x, rightHandRot.y, rightHandRot.z
            );
            File.AppendAllText(csvFilePath, line + "\n");

            yield return new WaitForSeconds(0.2f);
        }
    }

    public List<string[]> ReadCsv()
    {
        var lines = new List<string[]>();
        if (File.Exists(csvFilePath))
        {
            var allLines = File.ReadAllLines(csvFilePath);
            foreach (var line in allLines)
            {
                lines.Add(line.Split(','));
            }
        }
        return lines;
    }
}
