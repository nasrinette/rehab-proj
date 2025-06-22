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
    public GameObject patientFeedbackPrefab;
    public Transform contentFeedbackDoctor;
    public Transform contentFeedbackPatient;
    public Transform contentParentDoctor;
    public Transform contentParentPatient;
    public Transform contentPlayFeedback;
    public TMP_InputField titleInput;
    public TMP_InputField descriptionInput;

    private readonly Dictionary<string, GameObject> doctorDict  = new();
    private readonly Dictionary<string, GameObject> patientDict = new();
    private readonly Dictionary<string, GameObject> doctorFbDict  = new();
    private readonly Dictionary<string, GameObject> patientFbDict = new();
    private ExerciseList exerciseList = new();
    private string jsonPath;
    private string currentExerciseTitle = "";

    //ui connections to implementations
    public JointTracker jointTracker;
    public JointPlayback jointPlayback;
    public FeedbackDrawing feedbackDrawing;
    public RecordAudio recordAudio;

    public MovementRecordingsManager movementRecordingsManager;

    //playback variables
    public float playbackTime = 0f;
    public float maxPlaybackTime = 0f;

    public Slider playbackSlider;
    //public bool isPlayingExercise=false;

    public string patientString;
    public bool isPatient = false;


    void Start()
    {
        jsonPath = Path.Combine(Application.persistentDataPath, "exercises.json");
        //LoadExercisesFromJson();
        LoadExercisesFromFiles(isPatient: false); 
        RenderAllExercises();
    }

    private void Update()
    {
        patientString = isPatient ? "Patient_" : "Doctor_";
        if (currentExerciseTitle.Contains("Patient") || currentExerciseTitle.Contains("Doctor")) patientString = "";

        if (playbackSlider!= null && playbackSlider.IsActive()) moveSlider();
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

        var playBtn = FindButton(doctorGO.transform, "Play button");
        if (playBtn != null)
            playBtn.onClick.AddListener(() =>
            {
                currentExerciseTitle = data.title;
                userSelector.ShowOnly(userSelector.playExerciseDoctorPanel);
            });

        var fbBtn = FindButton(doctorGO.transform, "Feedback button");
        if (fbBtn != null)
        {
            fbBtn.onClick.AddListener(() => SendFeedbackToPatient(data.title, fbBtn));
            // userSelector.ShowOnly(userSelector.drawForFeedbackDoctorPanel);
        }
        else
        {
            Debug.LogWarning("Feedback button not found in doctor exercise prefab.");
        }
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
        {
            btn.onClick.AddListener(() => SendFeedbackToPatient(title, btn));
        }
    }

    private void SendExerciseToDoctor(string title, Button sendBtn)
    {
        // Doctor feedback content synchronized
        if (!doctorFbDict.ContainsKey(title))
        {
            var fbGO = Instantiate(doctorFeedbackPrefab, contentFeedbackDoctor);
            InitItemUI(fbGO, exerciseList.exercises.Find(e => e.title == title));
            BindFeedback(fbGO, title);
            doctorFbDict[title] = fbGO;
        }

        // Send to doctor -> Sent
        sendBtn.interactable = false;
        var txt = sendBtn.GetComponentInChildren<TMP_Text>();
        if (txt) txt.text = "Sent";
    }

    private void SendFeedbackToPatient(string title, Button fbBtn)
    {
        // Patient feedback content synchronized
        if (!patientFbDict.ContainsKey(title))
        {
            var perfGO = Instantiate(patientFeedbackPrefab, contentFeedbackPatient);
            InitItemUI(perfGO, exerciseList.exercises.Find(e => e.title == title));

            BindRemoveFeedbackPatient(perfGO, title); // only remove on Feedback/Patient Panel
            BindPlayFeedbackPatient(perfGO, title); // bind play feedback button
            patientFbDict[title] = perfGO;
        }
        userSelector.ShowOnly(userSelector.drawForFeedbackDoctorPanel);
        currentExerciseTitle = title; 
        Debug.LogWarning($"Giving feedback for exercise: {currentExerciseTitle}");
        fbBtn.interactable = false;
        var txt = fbBtn.GetComponentInChildren<TMP_Text>();
        if (txt) txt.text = "Feedback Sent";
    }



    private void BindRemoveFeedbackPatient(GameObject go, string titleKey)
    {
        var btn = FindButton(go.transform, "Remove patient button");
        if (btn == null) return;

        btn.onClick.AddListener(() =>
        {
            patientFbDict.Remove(titleKey);
            Destroy(go);
        });
    }
    private void BindPlayFeedbackPatient(GameObject go, string titleKey)
    {
        var btn = FindButton(go.transform, "Play button");
        if (btn == null) return;

        btn.onClick.AddListener(() =>
        {
           Debug.LogWarning($"Playing feedback for exercise: {titleKey}");
            currentExerciseTitle = titleKey; 
            userSelector.ShowOnly(userSelector.playFeedBackPatientPanel);
            movementRecordingsManager.generateFeedBackUI(contentPlayFeedback, titleKey); // regenerate feedback UI after stopping playback
        });
    }


    private Button FindButton(Transform root, string btnName)
    {
        foreach (var btn in root.GetComponentsInChildren<Button>(true))
            if (btn.gameObject.name.Equals(btnName))
                return btn;
        return null;
    }


    private static readonly ExerciseData[] defaultSamples =
    {
        // new ExerciseData
        // {
        //     title = "Bicep curl",
        //     description = "Bend your elbow to bring your hand toward your shoulder."
        // },
        // new ExerciseData
        // {
        //     title = "Shoulder raise",
        //     description = "Raise your arms straight to shoulder height."
        // }
    };

    private void LoadExercisesFromFiles(bool isPatient)
    {
        // Use MovementRecordingsManager to retrieve the exercise list
        exerciseList = movementRecordingsManager.MakeExerciseListFromFiles(isPatient);
        RenderAllExercises(); // Render the exercises after loading
    }

    private void SaveExercisesToJson()
    {
        // This method is no longer needed since saving is automatic during recording
        Debug.LogWarning("SaveExercisesToJson is deprecated. Saving is handled automatically during recording.");
    }

    private void RenderAllExercises()
    {
        // Clear existing items
        foreach (Transform child in contentParentDoctor) Destroy(child.gameObject);
        foreach (Transform child in contentParentPatient) Destroy(child.gameObject);

        foreach (var exercise in exerciseList.exercises)
        {
            CreateTwinItems(exercise);
        }
    }

    // add new exercise into ui manager (not added to files here)
    public void OnContinueAddExercise()
    {
        //LoadExercisesFromFiles(isPatient: false);

        string title = titleInput.text.Trim();
        string description = descriptionInput.text.Trim();
        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
        {
            Debug.LogWarning("Please input Title and Description！");
            return;
        }

        var data = new ExerciseData { title = title, description = description };
        CreateTwinItems(data);

        titleInput.text = "";
        descriptionInput.text = "";

        AppState.CurrentUser = UserType.Doctor;
        userSelector.ShowOnly(userSelector.recordDoctorPanel);
        currentExerciseTitle = title; // Set current exercise title for further actions
        Debug.LogWarning($"New exercise added: {currentExerciseTitle}");

    }

    #region old json loading
    //private void SaveExercisesToJson()
    //{
    //    string json = JsonUtility.ToJson(exerciseList, true);
    //    File.WriteAllText(jsonPath, json);
    //}

    //private void LoadExercisesFromJson()
    //{
    //    if (File.Exists(jsonPath))
    //    {
    //        string json = File.ReadAllText(jsonPath);
    //        exerciseList = JsonUtility.FromJson<ExerciseList>(json) ?? new ExerciseList();
    //        //exerciseList = JsonUtility.FromJson<ExerciseList>(File.ReadAllText(jsonPath));
    //    }
    //    else
    //    {
    //        exerciseList = new ExerciseList();
    //    }
    //    foreach (var sample in defaultSamples)
    //    {
    //        bool exists = exerciseList.exercises.Exists(e => e.title == sample.title);
    //        if (!exists)
    //        {
    //            exerciseList.exercises.Insert(0, sample);
    //        }
    //    }
    //    SaveExercisesToJson();
    //}

    //private void RenderAllExercises()
    //{
    //    foreach (var ex in exerciseList.exercises)
    //        CreateTwinItems(ex);
    //}

    //// public ui funcgitons

    //public void OnContinueAddExercise()
    //{
    //    string title = titleInput.text.Trim();
    //    string description = descriptionInput.text.Trim();
    //    if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
    //    {
    //        Debug.LogWarning("Please input Title and Description！");
    //        return;
    //    }

    //    var data = new ExerciseData { title = title, description = description };
    //    exerciseList.exercises.Add(data);
    //    SaveExercisesToJson();

    //    CreateTwinItems(data);
    //    titleInput.text = "";
    //    descriptionInput.text = "";
    //    AppState.CurrentUser = UserType.Doctor;
    //    userSelector.ShowOnly(userSelector.recordDoctorPanel);
    //    currentExerciseTitle = title; // Set current exercise title for further actions
    //    Debug.LogWarning($"New exercise added: {currentExerciseTitle}");
    //}


    #endregion

    public void moveSlider()
    {
        playbackSlider = FindObjectOfType<Slider>();

        if (playbackSlider != null && playbackSlider.IsActive())
        {
            // find active slider in the scene
            playbackSlider = FindObjectOfType<Slider>();
            maxPlaybackTime = movementRecordingsManager.GetMaxPlaybackTime(patientString + currentExerciseTitle);
            playbackSlider.maxValue = maxPlaybackTime;
        }
        if (playbackSlider != null)
        {
            playbackTime = jointPlayback.playbackTime;
            playbackSlider.value = playbackTime;
        }
        else
        {
            Debug.LogError("Playback slider not found in the scene.");
        }
    }
    public void OnSliderValueChanged()
    {
        jointPlayback.Seek(playbackSlider.value); 
    }


    #region ui functions simple
    // movements
    public void onStartPlay(bool isExerciseByPatient) 
    {
        isPatient = isExerciseByPatient;
        patientString = isPatient ? "Patient_" : "Doctor_";
        if (currentExerciseTitle.Contains("Patient") || currentExerciseTitle.Contains("Doctor")) patientString = "";

        jointPlayback.SetRecordingToPlay(patientString + currentExerciseTitle);
        jointPlayback.Play();

        moveSlider();
    }
    public void onStopPlay() => jointPlayback.Stop();
    public void onPausePlay() => jointPlayback.Pause();

    public void onStartRecord(bool isExerciseByPatient) 
    {
        isPatient = isExerciseByPatient;
        jointTracker.NewExercise(currentExerciseTitle, isPatient);
        jointTracker.StartRecording();
    }
    public void onStopRecord() => jointTracker.StopRecording();

    //feedback
    public void onStartRecordFeedback() 
    {
        feedbackDrawing.SetExerciseName(currentExerciseTitle, playbackTime.ToString());
        feedbackDrawing.setRecordingOn();

        recordAudio.StartRecording();
    }
    public void onStopRecordFeedback()
    {
        feedbackDrawing.setRecordingOff();

        recordAudio.StopRecording(currentExerciseTitle, playbackTime.ToString());
    }

    public void onStartPlayFeedback(string exerciseName, string timestamp) // already bound in movementRecordingsManager.cs
    { 
        feedbackDrawing.SetExerciseName(exerciseName, timestamp);
        feedbackDrawing.startPlayback();

        recordAudio.LoadRecording(exerciseName, timestamp);
        recordAudio.StartPlayback();
    }
    public void onStopPlayFeedback()
    {
        feedbackDrawing.StopPlayback();

        recordAudio.StopPlayback();
    }
    #endregion

    #region old ui connection functions

    public void onDoctorExercisePlay() {
        Debug.LogWarning($"Doctor plays his own exercise: {currentExerciseTitle} " );
        onStartPlay(false);
    }


    public void onDoctorExerciseStop()
    {
        Debug.LogWarning($"Doctor Stops his own exercise playing {currentExerciseTitle}");
        onStopPlay();
    }

    public void onDoctorExercisePause()
    {
        Debug.LogWarning($"Doctor pauses his own exercise playing {currentExerciseTitle}");
        onPausePlay();
    }

    public void onDoctorStartRecording()
    {
        Debug.LogWarning($"Doctor starts recording exercise: {currentExerciseTitle}");
        onStartRecord(false);
    }

    public void onDoctorStopRecording()
    {
        Debug.LogWarning($"Doctor stops recording exercise: {currentExerciseTitle} and goes to preview panel");
        onStopRecord();
    }

    public void onDoctorRedoExercise()
    { //do we even need this? we go back to recording panel again and it works any ways
        Debug.LogWarning($"Doctor redoes the exercise recording {currentExerciseTitle}");
        onStartRecord(false);
    }


    //Doctor feedback section now

    public void onDoctorStartFeedback()
    {
        Debug.LogWarning($"Doctor starts giving one feedback");
        onStartRecordFeedback();
    }
    public void onDoctorStopFeedback()
    {
        Debug.LogWarning($"Doctor stops giving that feedback ");
        onStopRecordFeedback();

    }

    public void onDoctorFinishAllFeedback() //TODO what is this
    {
        Debug.LogWarning($"Doctor finishes all feedback for exercise {currentExerciseTitle}");
    }


    //not in use
    public void onDoctorStartsAllFeedback() //TODO what is this
    {
        Debug.LogWarning($"Doctor starts giving feedback for exercise {currentExerciseTitle}");
    }

    public void onDoctorPlayPatientsExercise()
    {
        Debug.LogWarning($"Doctor plays patient's exercise: {currentExerciseTitle} for review and feedback");
        onStartPlay(true); 
    }

    public void onDoctorStopPatientExercise()
    {
        Debug.LogWarning($"Doctor stops patient's exercise playback: {currentExerciseTitle}");
        onStopPlay();
    }

    //Patient panel
    //exercise section
   public void onPatientPlaysDoctorsExercise()
    {
        Debug.LogWarning($"Patient plays doctor's exercise: {currentExerciseTitle} for practice");
        onStartPlay(false);
    }

    public void onPatientStopsDoctorsExercise()
    {
        Debug.LogWarning($"Patient stops doctor's exercise playback: {currentExerciseTitle}");
        onStopPlay();
    }

    public void onPatientPausesDoctorsExercise()
    {
        Debug.LogWarning($"Patient pauses doctor's exercise playback: {currentExerciseTitle}");
        onPausePlay();
    }

    public void onPatientStartRecordingPerform()
    {
        Debug.LogWarning($"Patient starts movement for this {currentExerciseTitle}");
        onStartRecord(true);
    }

    public void onPatientStopRecordingPerform()
    {
        Debug.LogWarning($"Patient stops movement for this {currentExerciseTitle}");
        onStopRecord();
    }

    public void onPatientPlayPreviewExercise()
    {
        Debug.LogWarning($"Patient previews/playback his own exercise before sending it to doctor {currentExerciseTitle}");
        onStartPlay(true); 
    }

    public void onPatientStopPreviewExercise()
    {
        Debug.LogWarning($"Patient stops his own exercise before sending it to doctor {currentExerciseTitle}");
        onStopPlay();
    }

    public void onPatientPausePreviewExercise()
    {
        Debug.LogWarning($"Patient pauses his own exercise before sending it to doctor {currentExerciseTitle}");
        jointPlayback.Pause(); // Pause playback of the patient's exercise
    }

    public void onPatientRedoExercise()
    {
        Debug.LogWarning($"Patient redoes the exercise recording {currentExerciseTitle}");
        onStartRecord(true); 
    }


    //feedback section

    public void onPatientPlayFeedback(string exerciseTitle, string timestamp) // auto added through movementRecordingsManager.cs

    {
        //Debug.LogWarning($"Feedback Playback for  {exerciseTitle} open on timestamp {timestamp}");
        //onStartPlayFeedback(exerciseTitle, timestamp);
        Debug.LogError("method should not be called like this, call onStartPlayFeedback instead");
    }
   
    public void onPatientStopFeedback(string exerciseTitle, string timestamp) //TODO no stop button
    {
        Debug.LogWarning($"Feedback Playback for  {exerciseTitle} closed on timestamp {timestamp}");
        feedbackDrawing.StopPlayback();
    }

    #endregion

}
