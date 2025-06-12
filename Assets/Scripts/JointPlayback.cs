using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class JointPlayback : MonoBehaviour
{
    public string csvFilePath;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    public string exerciseName = "JointPositions";

    private class Frame
    {
        public float time;
        public Vector3 headPos, headRot;
        public Vector3 leftHandPos, leftHandRot;
        public Vector3 rightHandPos, rightHandRot;
    }

    private List<Frame> frames = new List<Frame>();
    private float playbackTime = 0f;
    public bool playOnStart = true;
    private bool isPlaying = false;

    void Start()
    {
        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "RehabProject", "Recordings");
        csvFilePath = Path.Combine(folderPath, exerciseName + ".csv");

        LoadCsv();
        if (playOnStart) isPlaying = true;
    }

    void Update()
    {
        if (!isPlaying || frames.Count == 0)
            return;

        playbackTime += Time.deltaTime;

        // Find the closest frame by time
        Frame frame = GetFrameForTime(playbackTime);
        if (frame != null)
        {
            ApplyFrame(frame);
        }
    }

    private void LoadCsv()
    {
        frames.Clear();
        if (!File.Exists(csvFilePath))
        {
            Debug.LogWarning("CSV file not found: " + csvFilePath);
            return;
        }

        var lines = File.ReadAllLines(csvFilePath);
        for (int i = 1; i < lines.Length; i++) // skip header
        {
            var cols = lines[i].Split(',');
            if (cols.Length < 19) continue;

            Frame f = new Frame();
            int idx = 0;
            f.time = float.Parse(cols[idx++]);
            f.headPos = new Vector3(float.Parse(cols[idx++]), float.Parse(cols[idx++]), float.Parse(cols[idx++]));
            f.headRot = Vector3.zero; idx++; idx++; idx++;  //new Vector3(float.Parse(cols[idx++]), float.Parse(cols[idx++]), float.Parse(cols[idx++]));
            f.leftHandPos = new Vector3(float.Parse(cols[idx++]), float.Parse(cols[idx++]), float.Parse(cols[idx++]));
            f.leftHandRot = Vector3.zero; idx++; idx++; idx++;  //new Vector3(float.Parse(cols[idx++]), float.Parse(cols[idx++]), float.Parse(cols[idx++]));
            f.rightHandPos = new Vector3(float.Parse(cols[idx++]), float.Parse(cols[idx++]), float.Parse(cols[idx++]));
            f.rightHandRot = Vector3.zero; idx++; idx++; idx++; //new Vector3(float.Parse(cols[idx++]), float.Parse(cols[idx++]), float.Parse(cols[idx++]));
            frames.Add(f);
            //Vector3.zero; idx++; idx++; idx++;
        }
    }

    private Frame GetFrameForTime(float t)
    {
        // Simple nearest frame (no interpolation)
        Frame closest = null;
        float minDiff = float.MaxValue;
        foreach (var f in frames)
        {
            float diff = Mathf.Abs(f.time - t);
            if (diff < minDiff)
            {
                minDiff = diff;
                closest = f;
            }
        }
        return closest;
    }

    private void ApplyFrame(Frame f)
    {
        if (head != null)
        {
            head.position = f.headPos;
            head.rotation = Quaternion.Euler(f.headRot);
        }
        if (leftHand != null)
        {
            leftHand.position = f.leftHandPos;
            leftHand.rotation = Quaternion.Euler(f.leftHandRot);
        }
        if (rightHand != null)
        {
            rightHand.position = f.rightHandPos;
            rightHand.rotation = Quaternion.Euler(f.rightHandRot);
        }
    }

    // Optional: public controls
    public void Play() => isPlaying = true;
    public void Pause() => isPlaying = false;
    public void Stop() { isPlaying = false; playbackTime = 0f; }
    public void Seek(float time) { playbackTime = time; }
    public void SetRecordingToPlay(string recordingName)
    {
        exerciseName = recordingName;
        csvFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "RehabProject", "Recordings", exerciseName);
        LoadCsv();
    }
}
