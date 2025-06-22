using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Oculus.Avatar2;


public class JointPlayback : MonoBehaviour
{
    public string csvFilePath;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    public string exerciseName = "JointPositions";

    public GameObject entityGO;
    //public  manual, auto;
    public OvrAvatarInputManager manual, auto;
    public SampleAvatarEntity entityAvatarRef;

    private class Frame
    {
        public float time;
        public Vector3 headPos, headRot;
        public Vector3 leftHandPos, leftHandRot;
        public Vector3 rightHandPos, rightHandRot;
    }

    private List<Frame> frames = new List<Frame>();
    public float playbackTime = 0f;
    public bool playOnStart = true;
    private bool isPlaying = false;

    void Start()
    {
        entityAvatarRef = entityGO.GetComponent<SampleAvatarEntity>();
        if (playOnStart)
        {
            SetRecordingToPlay(exerciseName);
            isPlaying = true;
        }
    }

    void Update()
    {
        if (!isPlaying || frames.Count == 0) { }
        else { 

            playbackTime += Time.deltaTime;

            // Find the closest frame by time
            Frame frame = GetFrameForTime(playbackTime);
            if (frame != null)
            {
                ApplyFrame(frame);
            }
        }
    }

    private void LoadCsv()
    {
        frames.Clear();
        if (!File.Exists(csvFilePath))
        {
            Debug.LogWarning("CSV file not found: " + csvFilePath);
            return;
        } else
        {
            Debug.Log(Equals(csvFilePath, "csvFilePath") + " csvFilePath: " + csvFilePath);
        }

        var lines = File.ReadAllLines(csvFilePath);
        for (int i = 1; i < lines.Length; i++) // skip header
        {
            var cols = lines[i].Split(',');
            if (cols.Length != 19) Debug.LogError("wrong format for playback");

            Frame f = new Frame();
            int idx = 0;
            f.time = float.Parse(cols[idx++]);
            f.headPos = new Vector3(float.Parse(cols[idx++]), float.Parse(cols[idx++]), float.Parse(cols[idx++]));
            //f.headRot = Vector3.zero; idx++; idx++; idx++;
            f.headRot = new Vector3(float.Parse(cols[idx++]), float.Parse(cols[idx++]), float.Parse(cols[idx++]));
            f.leftHandPos = new Vector3(float.Parse(cols[idx++]), float.Parse(cols[idx++]), float.Parse(cols[idx++]));
            //f.leftHandRot = Vector3.zero; idx++; idx++; idx++;
            f.leftHandRot = new Vector3(float.Parse(cols[idx++]), float.Parse(cols[idx++]), float.Parse(cols[idx++]));
            f.rightHandPos = new Vector3(float.Parse(cols[idx++]), float.Parse(cols[idx++]), float.Parse(cols[idx++]));
            //f.rightHandRot = Vector3.zero; idx++; idx++; idx++; 
            f.rightHandRot = new Vector3(float.Parse(cols[idx++]), float.Parse(cols[idx++]), float.Parse(cols[idx++]));
            frames.Add(f);
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
            head.localPosition = f.headPos;
            head.localRotation = Quaternion.Euler(f.headRot);
        }
        if (leftHand != null)
        {
            leftHand.localPosition = f.leftHandPos;
            leftHand.localRotation = Quaternion.Euler(f.leftHandRot);
        }
        if (rightHand != null)
        {
            rightHand.localPosition = f.rightHandPos;
            rightHand.localRotation = Quaternion.Euler(f.rightHandRot);
        }
    }

    // Optional: public controls
    public void Play() => isPlaying = true;
    public void Pause() => isPlaying = false;
    public void Stop() { isPlaying = false; playbackTime = 0f; entityAvatarRef.SetInputManager(auto); }
    public void Seek(float time) { playbackTime = time; }
    public void SetRecordingToPlay(string recordingName)
    {
        if (isPlaying) Stop(); // Stop current playback if it's in progress
        exerciseName = recordingName + ".csv";
        csvFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "RehabProject", "MovementRecordings", exerciseName);
        LoadCsv();

        entityAvatarRef.SetInputManager(manual);
    }


}
