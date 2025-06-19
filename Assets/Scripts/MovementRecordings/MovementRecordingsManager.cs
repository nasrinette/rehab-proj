using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;


public class FeedbackList
{
    public List<string> timestamps;
}

public class MovementRecordingsManager : MonoBehaviour
{
    
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
}
