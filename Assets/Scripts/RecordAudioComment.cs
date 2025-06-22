using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using System;

public class RecordAudio : MonoBehaviour
{
    private AudioClip recordedClip;
    [SerializeField] AudioSource audioSource;
    private string filePath = "recording.wav";
    private string directoryPath = "Recordings";
    private float startTime;
    private float recordingLength;

    public void StartRecording()
    {
        string device = Microphone.devices[0];
        int sampleRate = 44100;
        int lengthSec = 3599;

        recordedClip = Microphone.Start(device, false, lengthSec, sampleRate);
        startTime = Time.realtimeSinceStartup;
        Debug.Log("recording started on device: " + device);
    }
    public void StartPlayback()
    {
        if (recordedClip != null)
        {
            audioSource.clip = recordedClip;
            audioSource.Play();
            Debug.LogWarning("Recording playing");

        }
        else
        {
            Debug.LogWarning("No recording available to play.");
        }
    }
    public void StopPlayback()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("Recording paused");
        }
        else
        {
            Debug.LogWarning("No recording is currently playing to pause.");
        }
    }

    public void StopRecording(string exerciseName, string timestamp)
    {
        Microphone.End(null);
        recordingLength = Time.realtimeSinceStartup - startTime;
        recordedClip = TrimClip(recordedClip, recordingLength);
        SaveRecording(exerciseName, timestamp);
        Debug.Log("Recording stopped");
    }

    public void SaveRecording(string exerciseName, string timestamp)
    {
        Debug.Log("Recording saving");

        if (recordedClip != null)
        {
            try
            {
                SaveWavFile(exerciseName, timestamp);
                Debug.Log("Recording saved as " + filePath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Exception during save: " + ex);
            }
        }
        else
        {
            Debug.LogError("No recording found to save.");
        }
    }

    public void LoadRecording(string exerciseName, string timestamp)
    {
        Debug.Log("Loading recording");

        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "RehabProject", "AudioRecordings");
        string fullPath = Path.Combine(folderPath, $"{exerciseName}_{timestamp}.wav");

        if (File.Exists(fullPath))
        {
            recordedClip =  WavUtility.ToAudioClip(fullPath);
            audioSource.clip = recordedClip;
            Debug.Log("Recording loaded from " + fullPath);
        }
        else
        {
            Debug.LogError("Recording file not found: " + fullPath);
        }
    }
    private AudioClip TrimClip(AudioClip clip, float length)
    { 
        Debug.Log("trim starting");
        int samples = (int)(clip.frequency * length);
        float[] data = new float[samples];
        clip.GetData(data, 0);

        AudioClip trimmedClip = AudioClip.Create(clip.name, samples,
        clip.channels, clip.frequency, false);
        trimmedClip.SetData(data, 0);
        Debug.Log("trim stopped");
        return trimmedClip;
    }


    //util
    public string SaveWavFile(string exerciseName, string timestamp)
    {
        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "RehabProject", "AudioRecordings");
        string fileName = $"{exerciseName}_{timestamp}.wav";

        string filepath;
        byte[] bytes = WavUtility.FromAudioClip(recordedClip, out filepath, true, folderPath, fileName);

        return filepath;
    }

}