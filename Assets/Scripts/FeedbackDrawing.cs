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
    private int currentLineId = 0;
    public float feedbackTimestamp = 0f;

    public string exerciseName;
    private string exerciseTimeStamp;

    private bool isPlaying = false;
    private List<RecordedPoint> playbackPoints = new List<RecordedPoint>();
    private float playbackTimer = 0f;
    private int playbackIndex = 0;

    public bool triggersSetRecordingOn, triggersSetRecordingOff, triggersStartPlayback, triggersStopPlayback;
    public string tempExerciseName;
    public float tempExerciseTimeStamp;
    public bool triggerSetExerciseName;

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
        //isHoldingTrigger = isDrawing; // TODO FOR TESTING
        if (isDrawing && isHoldingTrigger)
        {
            Draw();
        }
        else if (currentLine != null && !isHoldingTrigger)
        {
            // Stop drawing this line when trigger is released
            currentLine = null;
        }

        if (isPlaying)
        {
            //playbackTimer += Time.deltaTime;
            PlaybackUpdate();
        }


        // testing code
        if (triggersSetRecordingOn)
        {
            triggersSetRecordingOn = false;
            setRecordingOn();
        }
        if (triggersSetRecordingOff)
        {
            triggersSetRecordingOff = false;
            setRecordingOff();
        }
        if (triggerSetExerciseName)
        {
            triggerSetExerciseName = false;
            SetExerciseName(tempExerciseName, tempExerciseTimeStamp.ToString());
        }
        if (triggersStartPlayback)
        {
            triggersStartPlayback = false;
            startPlayback();
        }
        if (triggersStopPlayback)
        {
            triggersStopPlayback = false;
            StopPlayback();
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
            recordedPoints.Add(new RecordedPoint(currentLineId, currentPosition, feedbackTimestamp));
        }

        feedbackTimestamp += Time.deltaTime;
    }
    private void SaveRecordingToCSV()
    {
        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "RehabProject", "Feedbacks");
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        string fileName = $"{exerciseName}_{exerciseTimeStamp}_feedback.csv";
        string filePath = Path.Combine(folderPath, fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
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
        string filePath = Path.Combine(folderPath, $"{exerciseName}_{exerciseTimeStamp}_feedback.csv");

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            playbackPoints.Clear();

            foreach (string line in lines)
            {
                if (line.StartsWith("LineId")) continue; // Skip header

                string[] values = line.Split(',');
                if (values.Length == 5 &&
                    int.TryParse(values[0], out int lineId) &&
                    float.TryParse(values[1], out float x) &&
                    float.TryParse(values[2], out float y) &&
                    float.TryParse(values[3], out float z) &&
                    float.TryParse(values[4], out float time))
                {
                    Vector3 pos = new Vector3(x, y, z);
                    playbackPoints.Add(new RecordedPoint(lineId, pos, time));
                }
            }
        }
        else
        {
            Debug.LogError("No recording found to playback.");
        }
    }
    private void PlaybackUpdate()
    {
        if (playbackIndex >= playbackPoints.Count)
        {
            isPlaying = false;
            Debug.Log("Playback complete.");
            return;
        }

        playbackTimer += Time.deltaTime;

        while (playbackIndex < playbackPoints.Count && playbackPoints[playbackIndex].time <= playbackTimer)
        {
            RecordedPoint point = playbackPoints[playbackIndex];

            // Find or create the correct line
            LineRenderer line = allLines.Find(lr => lr.name == "Line_" + point.lineId);
            if (line == null)
            {
                GameObject newLineGO = Instantiate(linePrefab.gameObject);
                line = newLineGO.GetComponent<LineRenderer>();
                line.positionCount = 0;
                line.name = "Line_" + point.lineId;
                allLines.Add(line);
            }

            // Add the point
            line.positionCount++;
            line.SetPosition(line.positionCount - 1, point.position);

            playbackIndex++;
        }
    }


    // public functions for external control
    public void setRecordingOn()
    {
        if(exerciseName == null || exerciseName == "" || exerciseTimeStamp == null || exerciseTimeStamp == "")
        {
            Debug.LogError("Exercise name or timestamp is not set. Please set them before starting recording.");
            return;
        }
        isRecording = true;
        isDrawing = true;
        feedbackTimestamp = 0f;
        recordedPoints.Clear();
        Debug.Log("Recording started.");
    }

    public void setRecordingOff()
    {
        isRecording = false;
        isDrawing = false;
        SaveRecordingToCSV();
        Debug.Log("Recording stopped and saved.");
    }

    public void SetExerciseName(string newExerciseName, string newExerciseTimeStamp)
    {
        exerciseName = newExerciseName;
        exerciseTimeStamp = newExerciseTimeStamp;

        Debug.Log($"Exercise name set to: {exerciseName} with timestamp: {exerciseTimeStamp}");

        if (exerciseName.Contains("Doctor") || exerciseName.Contains("Patient")){ }
        else
        {
            Debug.LogWarning("Exercise name does not contain 'Doctor' or 'Patient'. Timestamp may not be set correctly.");
        }
    }

    public void startPlayback()
    {
        if (!isPlaying)
        {
            if (playbackPoints.Count > 0)
            {
                isPlaying = true;
                playbackTimer = 0f;
                playbackIndex = 0;
                allLines.Clear();
                Debug.Log("Playback started.");
            }
            else
            {
                Debug.LogWarning("No points found in file.");
            }
            PlaybackDrawing();
        }
        else
        {
            Debug.LogWarning("Playback is already in progress.");
        }
    }
    public void StopPlayback()
    {
        isPlaying = false;
        playbackPoints.Clear();
        Debug.Log("Playback manually stopped.");
    }

}
