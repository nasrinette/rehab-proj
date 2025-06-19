using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

public class RecordAudio : MonoBehaviour
{
    private AudioClip recordedClip;
    [SerializeField] AudioSource audioSource;
    private string filePath = "recording.wav";
    private string directoryPath = "Recordings";
    private float startTime;
    private float recordingLength;

    private void Awake()
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    public void StartRecording()
    {
        string device = Microphone.devices[0];
        int sampleRate = 44100;
        int lengthSec = 3599;

        recordedClip = Microphone.Start(device, false, lengthSec, sampleRate);
        startTime = Time.realtimeSinceStartup;
        Debug.Log("recording started on device: " + device);
    }
    public void PlayRecording()
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


    public void StopRecording()
    {
        Microphone.End(null);
        recordingLength = Time.realtimeSinceStartup - startTime;
        recordedClip = TrimClip(recordedClip, recordingLength);
        SaveRecording();
        Debug.Log("Recording stopped");
    }

    public void SaveRecording()
    {
        Debug.Log("Recording saving");

        if (recordedClip != null)
        {
            try
            {
                string fullPath = Path.Combine(Application.persistentDataPath, "recording.wav");
                WavUtility.Save(fullPath, recordedClip);
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

}