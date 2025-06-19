using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public class FeedbackDrawing : MonoBehaviour
{
    [System.Serializable]
    public struct RecordedPoint
    {
        public int lineId;
        public Vector3 position;
        public float time;

        public RecordedPoint(int lineId, Vector3 position, float time)
        {
            this.lineId = lineId;
            this.position = position;
            this.time = time;
        }
    }

    public bool isDrawing = false;
    public bool isRecording = false;
    public LineRenderer linePrefab;
    private LineRenderer currentLine;
    private List<LineRenderer> allLines = new List<LineRenderer>();
    private List<RecordedPoint> recordedPoints = new List<RecordedPoint>();
    public string exerciseName = "DrawingExercise";
    private int currentLineId = 0;

    private void Start()
    {
        if (linePrefab == null)
        {
            GameObject lineGO = new GameObject("LineRenderer");
            linePrefab = lineGO.AddComponent<LineRenderer>();
            linePrefab.material = new Material(Shader.Find("Sprites/Default"));
            linePrefab.startWidth = 0.1f;
            linePrefab.endWidth = 0.1f;
            linePrefab.positionCount = 0;
        }
    }

    private void Update()
    {
        bool isHoldingTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) > 0.1f;

        if (isDrawing && isHoldingTrigger)
        {
            Draw();
        }
        else if (currentLine != null && !isHoldingTrigger)
        {
            // Stop drawing this line when trigger is released
            currentLine = null;
        }
    }

    private void Draw()
    {
        if (currentLine == null)
        {
            // Instantiate from prefab GameObject properly
            GameObject newLineGO = Instantiate(linePrefab.gameObject, transform.position, Quaternion.identity);
            currentLine = newLineGO.GetComponent<LineRenderer>();
            currentLine.positionCount = 0;
            allLines.Add(currentLine);
            currentLineId++;
        }

        Vector3 currentPosition = transform.position;
        if (currentLine.positionCount == 0 || currentLine.GetPosition(currentLine.positionCount - 1) != currentPosition)
        {
            currentLine.positionCount++;
            currentLine.SetPosition(currentLine.positionCount - 1, currentPosition);
            recordedPoints.Add(new RecordedPoint(currentLineId, currentPosition, Time.time));
        }
    }

    private void SaveRecordingToCSV(string exerciseTimeStamp = "")
    {
        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "RehabProject", "Feedbacks");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string fileName = $"{exerciseName}_{exerciseTimeStamp}_feedback.csv";
        string filePath = Path.Combine(folderPath, fileName);

        if (File.Exists(filePath))
        {
            filePath = Path.Combine(folderPath, $"{exerciseName}_{exerciseTimeStamp}_feedback_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("LineId,X,Y,Z,Time");
                foreach (var point in recordedPoints)
                {
                    writer.WriteLine($"{point.lineId},{point.position.x},{point.position.y},{point.position.z},{point.time}");
                }
            }
            Debug.Log($"Drawing saved to {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save CSV: {e.Message}");
        }

        recordedPoints.Clear();
    }

    public void PlaybackDrawing()
    {
        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "RehabProject", "Feedbacks");
        string filePath = Path.Combine(folderPath, $"{exerciseName}_feedback.csv");

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            Dictionary<int, List<Vector3>> linePoints = new Dictionary<int, List<Vector3>>();

            foreach (string line in lines)
            {
                if (line.StartsWith("LineId")) continue; // Skip header

                string[] values = line.Split(',');
                if (values.Length == 5 &&
                    int.TryParse(values[0], out int lineId) &&
                    float.TryParse(values[1], out float x) &&
                    float.TryParse(values[2], out float y) &&
                    float.TryParse(values[3], out float z))
                {
                    Vector3 pos = new Vector3(x, y, z);
                    if (!linePoints.ContainsKey(lineId))
                        linePoints[lineId] = new List<Vector3>();
                    linePoints[lineId].Add(pos);
                }
            }

            // Recreate lines visually
            foreach (var kvp in linePoints)
            {
                GameObject newLineGO = Instantiate(linePrefab.gameObject);
                LineRenderer lr = newLineGO.GetComponent<LineRenderer>();
                lr.positionCount = kvp.Value.Count;
                lr.SetPositions(kvp.Value.ToArray());
                allLines.Add(lr);
            }

            Debug.Log("Playback complete: lines redrawn.");
        }
        else
        {
            Debug.LogError("No recording found to playback.");
        }
    }

    public void setRecordingOn()
    {
        isRecording = true;
        recordedPoints.Clear();
        Debug.Log("Recording started.");
    }

    public void setRecordingOff(string exerciseTimeStamp = "")
    {
        isRecording = false;
        SaveRecordingToCSV(exerciseTimeStamp);
        Debug.Log("Recording stopped and saved.");
    }
}
