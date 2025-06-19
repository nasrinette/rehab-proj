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
    public List<ExerciseData> exercises = new();
}

public class UIManager : MonoBehaviour
{
    public UserSelector userSelector;
    public GameObject doctorExercisePrefab;
    public GameObject patientExercisePrefab;
    public Transform contentParentDoctor;
    public Transform contentParentPatient;
    public TMP_InputField titleInput;
    public TMP_InputField descriptionInput;

    private readonly Dictionary<string, GameObject> doctorDict  = new();
    private readonly Dictionary<string, GameObject> patientDict = new();
    private ExerciseList exerciseList = new();
    private string jsonPath;
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

        var data = new ExerciseData { title = title, description = description };
        exerciseList.exercises.Add(data);
        SaveExercisesToJson();

        CreateTwinItems(data);
        titleInput.text = "";
        descriptionInput.text = "";
    }

    private void CreateTwinItems(ExerciseData data)
    {
        var doctorGO = Instantiate(doctorExercisePrefab, contentParentDoctor);
        InitItemUI(doctorGO, data);
        doctorDict[data.title] = doctorGO;

        var patientGO = Instantiate(patientExercisePrefab, contentParentPatient);
        InitItemUI(patientGO, data);
        patientDict[data.title] = patientGO;

        BindRemove(doctorGO, data.title);
        BindRemove(patientGO, data.title);
        BindPerform(patientGO, data.title);

        var editBtn = FindButton(doctorGO.transform, "Edit button");
        if (editBtn != null)
            editBtn.onClick.AddListener(() =>
            {
                currentExerciseTitle = data.title;
                userSelector.ShowOnly(userSelector.recordDoctorPanel); // playDoctorPanel needed
            });
    }

    private void RemoveByTitle(string title)
    {
        if (doctorDict.TryGetValue(title, out var doctor)) Destroy(doctor);
        if (patientDict.TryGetValue(title, out var patient)) Destroy(patient);
        doctorDict.Remove(title);
        patientDict.Remove(title);

        exerciseList.exercises.RemoveAll(exercise => exercise.title == title);
        SaveExercisesToJson();
    }

    private void InitItemUI(GameObject go, ExerciseData data)
    {
        go.name = "exercise_" + data.title;
        go.transform.Find("name")?.GetComponent<TMP_Text>().SetText(data.title);
        go.transform.Find("content")?.GetComponent<TMP_Text>().SetText(data.description);
    }

    private void BindRemove(GameObject go, string key)
    {
        var btn = FindButton(go.transform, "Remove button");
        if (btn != null)
            btn.onClick.AddListener(() => RemoveByTitle(key));
    }

    private void BindPerform(GameObject go, string titleKey)
    {
        var btn = FindButton(go.transform, "Perform button");
        if (btn != null)
            btn.onClick.AddListener(() =>
            {
                userSelector.ShowOnly(userSelector.recordPatientPanel);
            });
    }

    private Button FindButton(Transform root, string btnName)
    {
        foreach (var btn in root.GetComponentsInChildren<Button>(true))
            if (btn.gameObject.name.Equals(btnName))
                return btn;
        return null;
    }

    private void SaveExercisesToJson()
    {
        string json = JsonUtility.ToJson(exerciseList, true);
        File.WriteAllText(jsonPath, json);
    }

    private static readonly ExerciseData[] defaultSamples =
    {
        new ExerciseData
        {
            title = "Bicep curl",
            description = "Bend your elbow to bring your hand toward your shoulder."
        },
        new ExerciseData
        {
            title = "Shoulder raise",
            description = "Raise your arms straight to shoulder height."
        }
    };

    private void LoadExercisesFromJson()
    {
        if (File.Exists(jsonPath))
        {
            string json = File.ReadAllText(jsonPath);
            exerciseList = JsonUtility.FromJson<ExerciseList>(json) ?? new ExerciseList();
            //exerciseList = JsonUtility.FromJson<ExerciseList>(File.ReadAllText(jsonPath));
        }
        else
        {
            exerciseList = new ExerciseList();
        }
        foreach (var sample in defaultSamples)
        {
            bool exists = exerciseList.exercises.Exists(e => e.title == sample.title);
            if (!exists)
            {
                exerciseList.exercises.Insert(0, sample);
            }
        }
        SaveExercisesToJson();
    }

    private void RenderAllExercises()
    {
        foreach (var ex in exerciseList.exercises)
            CreateTwinItems(ex);
    }
public void onDoctorExercisePlay()
    {
        Debug.LogWarning("Doctor plays his own exercise");
    }

    public void onDoctorNewExerciseContinue()
    {
        Debug.LogWarning("Doctor continues to add a new exercise");
    }

    public void onDoctorStartRecording(string exerciseTitle)
    {
        Debug.LogWarning($"Doctor starts recording exercise: {exerciseTitle}");
      

    }

    public void onDoctorStopRecording(string exerciseTitle)
    {
        Debug.LogWarning($"Doctor stops recording exercise: {exerciseTitle} and goes to preview panel");

    }

  public void onDoctorPreviewExercise(string exerciseTitle)
    {
        Debug.LogWarning($"Doctor previews his own exercise before saving: {exerciseTitle}");
    

    }
    public void onDoctorRedoExercise(string exerciseTitle)
    {
        Debug.LogWarning("Doctor redoes the exercise recording");
    }


    //Doctor feedback section now

    public void onDoctorGiveFeedback(string exerciseTitle)
    {
        Debug.LogWarning("Doctor plays the recording ");
        //todo more
    }


    //Patient panel
    //exercise section
    public void onPatientPerform(string exerciseTitle)
    {//patient watches the recording of doctor 
        Debug.LogWarning($"Playback of doctor's performance with controls {exerciseTitle}");
       
    }
    public void onPatientStartRecordingPerform(string exerciseTitle)
    {
        Debug.LogWarning($"Patient starts movement for this {exerciseTitle}");

    }
    public void onPatientStoptRecordingPerform(string exerciseTitle)
    {
        Debug.LogWarning($"Patient stops movement for this {exerciseTitle}");

    }

    public void onPatientPreviewExercise(string exerciseTitle)
    {
        Debug.LogWarning($"Patient previews/playback his own exercise before sending it to doctor");


    }
    public void onPatientRedoExercise(string exerciseTitle)
    {
        Debug.LogWarning("Patient redoes the exercise recording");
    }


    //feedback section

    public void onPatientPlayFeedback(string exerciseTitle)
    {
        Debug.LogWarning($"Feedback Playback for  {exerciseTitle} opens");
    }
    //todo more


    // public void RecordVideo() { }
    // public void PauseVideo() { }
    // public void StopVideo() { }
    // public void SaveVideo() { }

}
