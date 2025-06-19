using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public class MovementRecordingsManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
