using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using TMPro;


public class FeedbackList
{
    public List<string> timestamps = new();
}

public class MovementRecordingsManager : MonoBehaviour
{
    public GameObject feedbackButtonPrefab;
    private void Start()
    {
        List<string> tempTimeStamps = MakeFeedbackListFromFiles("Patient_de").timestamps;
        foreach (string timestamp in tempTimeStamps)
        {
            Debug.Log("Timestamp: " + timestamp);
        }
    }
    public FeedbackList MakeFeedbackListFromFiles(string exerciseName) // exercisename should be like "Patient_DrawingExercise" or "Doctor_DrawingExercise"
    {
        string timestampFromFile;

        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "RehabProject", "Feedbacks");
        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning("Directory does not exist: " + folderPath);
            return null;
        }

        string[] files = Directory.GetFiles(folderPath, exerciseName + "*.csv");
        FeedbackList feedbacksList = new FeedbackList();

        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            
            // if filename starts with patient_ or doctor_ then get the timestamp which is the part after 2nd _ otherwise it is after the first 
            timestampFromFile = fileName.Contains("octor") || fileName.Contains("atient") ? fileName.Split('_')[2] : fileName.Split('_')[1];
            Debug.Log("Timestamp from file: " + timestampFromFile);
            feedbacksList.timestamps.Add(timestampFromFile);
        }
        return feedbacksList;
    }

    public ExerciseList MakeExerciseListFromFiles(bool patient)
    {
        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "RehabProject", "MovementRecordings");
        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning("Directory does not exist: " + folderPath);
            return null;
        }

        string[] files = Directory.GetFiles(folderPath, (patient ? "Patient_" : "Doctor_") + "*.csv");
        ExerciseList exerciseList = new ExerciseList();

        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            ExerciseData data = new ExerciseData { title = fileName, description = "Recording from " + fileName };
            exerciseList.exercises.Add(data);
        }
        return exerciseList;
    }

    public void generateFeedBackUI(Transform content, string exerciseName)
    {
        // create a new tmbutton for each timestamp in the feedback list
        FeedbackList feedbackList = MakeFeedbackListFromFiles(exerciseName);
        if (feedbackList == null || feedbackList.timestamps.Count == 0)
        {
            Debug.LogWarning("No feedbacks found for the specified exercise.");
            return;
        }
        foreach (string timestamp in feedbackList.timestamps)
        {
            GameObject newButton = Instantiate(feedbackButtonPrefab, content);
            newButton.name = "FeedbackButton_" + timestamp;
            newButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = timestamp;
            //newButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnFeedbackButtonClicked(timestamp));
        }
    }
}
