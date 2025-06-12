using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public class FeedbackDrawing : MonoBehaviour
{
    public bool isDrawing = false; // Toggle to enable/disable drawing
    public bool isRecording = false; // Toggle to enable/disable recording
    public LineRenderer linePrefab; // Assign a LineRenderer prefab in the Inspector
    private LineRenderer currentLine;
    private List<LineRenderer> allLines = new List<LineRenderer>(); // Store all LineRenderers
    private List<(int lineId, Vector3 position, float time)> recordedPoints = new List<(int, Vector3, float)>(); // Stores lineId, points, and timestamps
    public string exerciseName = "DrawingExercise";
    private int currentLineId = 0; // Unique identifier for each LineRenderer

    private void Start()
    {
        // If no line prefab, create one
        if (linePrefab == null)
        {
            linePrefab = new GameObject("LineRenderer").AddComponent<LineRenderer>();
            linePrefab.material = new Material(Shader.Find("Sprites/Default"));
            linePrefab.startWidth = 0.1f;
            linePrefab.endWidth = 0.1f;
            linePrefab.positionCount = 0;
        }
    }

    private void Update()
    {
        bool isHoldingTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) > 0.1f;
        // TODO REMOVE THIS
        isHoldingTrigger = true; // For testing purposes, always hold the trigger

        if (isDrawing && isHoldingTrigger)
        {
            Draw();
        }
        else if (currentLine != null && !isDrawing) //TODO CHANGE THIS TO !isHoldingTrigger
        {
            // Stop drawing the current line
            currentLine = null;
        }

        if (!isDrawing && isRecording && recordedPoints.Count > 0)
        {
            // Save recording when drawing stops
            SaveRecordingToCSV();
        }
    }

    private void Draw()
    {
        if (currentLine == null)
        {
            // Create a new line when starting to draw
            currentLine = Instantiate(linePrefab, transform.position, Quaternion.identity);
            currentLine.positionCount = 0;
            allLines.Add(currentLine); // Add to the list of lines
            currentLineId++; // Increment the line ID
        }

        // Add the current position to the line and record it
        Vector3 currentPosition = transform.position;
        if (currentLine.positionCount == 0 || currentLine.GetPosition(currentLine.positionCount - 1) != currentPosition)
        {
            currentLine.positionCount++;
            currentLine.SetPosition(currentLine.positionCount - 1, currentPosition);
            recordedPoints.Add((currentLineId, currentPosition, Time.time)); // Record lineId, position, and timestamp
        }
    }

    private void SaveRecordingToCSV()
    {
        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "RehabProject", "Feedbacks");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, exerciseName + "_feedback.csv");
        // Delete file if it exists
        if (File.Exists(filePath))
        {
            //File.Delete(filePath);
            filePath = Path.Combine(folderPath, exerciseName + "_feedback_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv"); // Create a new file with timestamp
        }

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("LineId,X,Y,Z,Time"); // Add header
            foreach (var point in recordedPoints)
            {
                writer.WriteLine($"{point.lineId},{point.position.x},{point.position.y},{point.position.z},{point.time}");
            }
        }
        Debug.Log($"Drawing saved to {filePath}");
        recordedPoints.Clear(); // Clear the points after saving
    }

    public void PlaybackDrawing()
    {
        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "RehabProject", "Feedbacks", exerciseName + "_feedback.csv");
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                string[] values = line.Split(',');
                if (values.Length == 5 &&
                    int.TryParse(values[0], out int lineId) &&
                    float.TryParse(values[1], out float x) &&
                    float.TryParse(values[2], out float y) &&
                    float.TryParse(values[3], out float z) &&
                    float.TryParse(values[4], out float time))
                {
                    Debug.Log($"Playback Line {lineId}: Point {new Vector3(x, y, z)} at Time: {time}");
                }
            }
        }
        else
        {
            Debug.LogError("No recording found to playback.");
        }
    }

    public void setRecordingOn()
    {
        isRecording = true;
        recordedPoints.Clear(); // Clear previous points
        Debug.Log("Recording started.");
    }

    public void setRecordingOff()
    {
        isRecording = false;
        recordedPoints.Clear();
        Debug.Log("Recording stopped.");
    }
}
