using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

[System.Serializable]
public class ExerciseData
{
    public string title;
    public string description;
}

[System.Serializable]
public class ExerciseList
{
    public List<ExerciseData> exercises = new List<ExerciseData>();
}

public class UIManager : MonoBehaviour
{
    public UserSelector userSelector;
    public GameObject exerciseItem;
    public Transform contentParent;
    public TMP_InputField titleInput;
    public TMP_InputField descriptionInput;


    private string jsonPath;
    private ExerciseList exerciseList = new ExerciseList();
    private string currentExerciseTitle = "";


    void Start()
    {
        jsonPath = Path.Combine(Application.persistentDataPath, "exercises.json");
        LoadExercisesFromJson();
        RenderAllExercises();
    }

    public void OnContinueAddExercise()
    {
        string title = titleInput.text.Trim();
        string description = descriptionInput.text.Trim();

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
        {
            Debug.LogWarning("Please input Title and Description！");
            return;
        }

        ExerciseData data = new ExerciseData { title = title, description = description };
        exerciseList.exercises.Add(data);
        SaveExercisesToJson();
        CreateExerciseItem(data);
    }    
    
        public void RemoveExerciseItem(GameObject item)
    {
        TMP_Text nameText = item.transform.Find("name")?.GetComponent<TMP_Text>();
        if (nameText != null)
        {
            string titleToRemove = nameText.text;

            exerciseList.exercises.RemoveAll(e => e.title == titleToRemove);
            SaveExercisesToJson();
        }

        Destroy(item);
    }

    private void CreateExerciseItem(ExerciseData data)
    {
        GameObject newItem = Instantiate(exerciseItem, contentParent);
        newItem.name = "assigned exercise_" + data.title;

        var nameText = newItem.transform.Find("name")?.GetComponent<TMP_Text>();
        if (nameText != null) nameText.text = data.title;

        var contentText = newItem.transform.Find("content")?.GetComponent<TMP_Text>();
        if (contentText != null) contentText.text = data.description;

        var removeButton = newItem.transform.Find("Remove button")?.GetComponent<Button>();
        if (removeButton != null)
        {
            removeButton.onClick.AddListener(() => RemoveExerciseItem(newItem));
        }

        Button editButton = newItem.transform.Find("Edit button")?.GetComponent<Button>();
        if (editButton != null)
        {
            editButton.onClick.AddListener(() =>
            {
            currentExerciseTitle = data.title;
            userSelector.ShowOnly(userSelector.recordDoctorPanel);
            });
        }

    }

    private void SaveExercisesToJson()
    {
        string json = JsonUtility.ToJson(exerciseList, true);
        File.WriteAllText(jsonPath, json);
    }

    private void LoadExercisesFromJson()
    {
        if (File.Exists(jsonPath))
        {
            string json = File.ReadAllText(jsonPath);
            exerciseList = JsonUtility.FromJson<ExerciseList>(json);
        }
        else
        {
            // 2 default samples
            exerciseList.exercises.Add(new ExerciseData
            {
                title = "Bicep curl",
                description = "Bend your elbow to bring your hand toward your shoulder."
            });
            exerciseList.exercises.Add(new ExerciseData
            {
                title = "Shoulder raise",
                description = "Raise your arms straight to shoulder height."
            });
            SaveExercisesToJson();
        }
    }

    private void RenderAllExercises()
    {
        foreach (var exercise in exerciseList.exercises)
        {
            CreateExerciseItem(exercise);
        }
    }

    // public void RecordVideo() { }
    // public void PauseVideo() { }
    // public void StopVideo() { }
    // public void SaveVideo() { }


}
