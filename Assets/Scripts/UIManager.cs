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
    public GameObject doctorFeedbackPrefab;
    public GameObject patientPerformedPrefab;
    public Transform contentFeedbackDoctor;
    public Transform contentFeedbackPatient;
    public Transform contentParentDoctor;
    public Transform contentParentPatient;
    public Transform contentFeedback;
    public TMP_InputField titleInput;
    public TMP_InputField descriptionInput;

    private readonly Dictionary<string, GameObject> doctorDict  = new();
    private readonly Dictionary<string, GameObject> patientDict = new();
    private readonly Dictionary<string, GameObject> doctorFbDict  = new();
    private readonly Dictionary<string, GameObject> patientFbDict = new();
    private ExerciseList exerciseList = new();
    private string jsonPath;
    private string currentExerciseTitle = "";


    void Start()
    {
        jsonPath = Path.Combine(Application.persistentDataPath, "exercises.json");
        LoadExercisesFromJson();
        RenderAllExercises();
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
        BindSend(patientGO, data.title);

        var editBtn = FindButton(doctorGO.transform, "Play button");
        if (editBtn != null)
            editBtn.onClick.AddListener(() =>
            {
                currentExerciseTitle = data.title;
                userSelector.ShowOnly(userSelector.playExerciseDoctorPanel);
            });

        var fbBtn = FindButton(doctorGO.transform, "Give Feedback button");
        if (fbBtn != null)
            fbBtn.onClick.AddListener(() => SendFeedbackToPatient(data.title, fbBtn));
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
                Debug.LogWarning($"Performing exercise: {titleKey}");
                currentExerciseTitle = titleKey; 
                userSelector.ShowOnly(userSelector.recordPatientPanel);
            });
    }

    private void BindSend(GameObject go, string title)
    {
        var btn = FindButton(go.transform, "Send button");
        if (btn != null)
            btn.onClick.AddListener(() => SendExerciseToDoctor(title, btn));
    }

    private void BindFeedback(GameObject go, string title)
    {
        var btn = FindButton(go.transform, "Feedback button");
        if (btn != null)
            btn.onClick.AddListener(() => SendFeedbackToPatient(title, btn));
    }

    private void BindPerformFeedbackPatient(GameObject go, string title)
    {
        // Patient 侧“Performed”条目如果还有播放按钮可在此绑定
    }

    private void SendExerciseToDoctor(string title, Button sendBtn)
    {
        // 克隆到 Doctor Feedback 列表
        if (!doctorFbDict.ContainsKey(title))
        {
            var fbGO = Instantiate(doctorFeedbackPrefab, contentFeedbackDoctor);
            InitItemUI(fbGO, exerciseList.exercises.Find(e => e.title == title));
            BindFeedback(fbGO, title);
            doctorFbDict[title] = fbGO;
        }

        // 修改 Patient 侧条目外观
        sendBtn.interactable = false;
        var txt = sendBtn.GetComponentInChildren<TMP_Text>();
        if (txt) txt.text = "Performed";
    }

    /* ---------- 6. Feedback 逻辑 ---------- */
    private void SendFeedbackToPatient(string title, Button fbBtn)
    {
        if (!patientFbDict.ContainsKey(title))
        {
            var perfGO = Instantiate(patientPerformedPrefab, contentFeedbackPatient);
            InitItemUI(perfGO, exerciseList.exercises.Find(e => e.title == title));
            patientFbDict[title] = perfGO;
        }

        fbBtn.interactable = false;
        var txt = fbBtn.GetComponentInChildren<TMP_Text>();
        if (txt) txt.text = "Feedback Sent";
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

    // public ui funcgitons

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
        AppState.CurrentUser = UserType.Doctor;
        userSelector.ShowOnly(userSelector.recordDoctorPanel);
        currentExerciseTitle = title; // Set current exercise title for further actions
        Debug.LogWarning($"New exercise added: {currentExerciseTitle}");
    }

    public void onDoctorExercisePlay() {
        Debug.LogWarning($"Doctor plays his own exercise: {currentExerciseTitle} " );
       
    }


    public void onDoctorExerciseStop()
    {
        Debug.LogWarning($"Doctor Stops his own exercise playing {currentExerciseTitle}");

    }

    public void onDoctorStartRecording()
    {
        Debug.LogWarning($"Doctor starts recording exercise: {currentExerciseTitle}");
      

    }

    public void onDoctorStopRecording()
    {
        Debug.LogWarning($"Doctor stops recording exercise: {currentExerciseTitle} and goes to preview panel");

    }


    public void onDoctorRedoExercise()
    { //do we even need this? we go back to recording panel again and it works any ways
        Debug.LogWarning($"Doctor redoes the exercise recording {currentExerciseTitle}");
    }


    //Doctor feedback section now

    public void onDoctorStartFeedback(string feedbackID)
    {
        Debug.LogWarning($"Doctor starts giving one feedback {feedbackID}");
        
    }
    public void onDoctorStopFeedback(string feedbackID)
    {
        Debug.LogWarning($"Doctor stops giving that feedback {feedbackID}");
       
    }

    public void onDoctorFinishAllFeedback()
    {
        Debug.LogWarning($"Doctor finishes all feedback for exercise {currentExerciseTitle}");
    }

    public void onDoctorStartsAllFeedback()
    {
        Debug.LogWarning($"Doctor starts giving feedback for exercise {currentExerciseTitle}");
    }

    public void onDoctorPlayPatientsExercise()
    {
        Debug.LogWarning($"Doctor plays patient's exercise: {currentExerciseTitle} for review and feedback");
    }
    public void onDoctorStopPatientExercise()
    {
        Debug.LogWarning($"Doctor stops patient's exercise playback: {currentExerciseTitle}");
    }

    //Patient panel
    //exercise section
   public void onPatientPlaysDoctorsExercise()
    {
        Debug.LogWarning($"Patient plays doctor's exercise: {currentExerciseTitle} for practice");
    }
    public void onPatientStopsDoctorsExercise()
    {
        Debug.LogWarning($"Patient stops doctor's exercise playback: {currentExerciseTitle}");
    }
    public void onPatientStartRecordingPerform()
    {
        Debug.LogWarning($"Patient starts movement for this {currentExerciseTitle}");

    }
    public void onPatientStoptRecordingPerform()
    {
        Debug.LogWarning($"Patient stops movement for this {currentExerciseTitle}");

    }

    public void onPatientPlayPreviewExercise()
    {
        Debug.LogWarning($"Patient previews/playback his own exercise before sending it to doctor {currentExerciseTitle}");


    }
    public void onPatientStopPreviewExercise()
    {
        Debug.LogWarning($"Patient stops his own exercise before sending it to doctor {currentExerciseTitle}");


    }
    public void onPatientRedoExercise()
    {
        Debug.LogWarning($"Patient redoes the exercise recording {currentExerciseTitle}");
    }


    //feedback section

    public void onPatientPlayFeedback(string exerciseTitle, string timestamp) 
   
    {
   
        Debug.LogWarning($"Feedback Playback for  {exerciseTitle} open on timestamp {timestamp}");
    }
   
    public void onPatientStopFeedback(string exerciseTitle, string timestamp)
    {
        Debug.LogWarning($"Feedback Playback for  {exerciseTitle} closed on timestamp {timestamp}");
    }


    // public void RecordVideo() { }
    // public void PauseVideo() { }
    // public void StopVideo() { }
    // public void SaveVideo() { }

}
