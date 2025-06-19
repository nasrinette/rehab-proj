using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using static Oculus.Avatar2.OvrAvatarEntity;

public class UserSelector : MonoBehaviour
{
    [Header("Avatar presets")]
    [SerializeField] private SampleAvatarEntity mainAvatar;    // drag �MainAvatar� here
    [SerializeField] private SampleAvatarEntity mirrorAvatar;  // drag �MirrorAvatar� here
    [SerializeField] private int doctorPreset = 0;             // Style-1 presets 0-6
    [SerializeField] private int patientPreset = 2;             // pick any other index
    // [SerializeField] private GameObject mainAvatarGO;    // drag MainAvatar here
    // [SerializeField] private GameObject mirrorAvatarGO;  // drag MirrorAvatar here

    [SerializeField] private List<GameObject> allPanels;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject doctorPanel;
    [SerializeField] private GameObject patientPanel;
    [SerializeField] private GameObject newExercisePanel;
    [SerializeField] private GameObject playExerciseDoctorPanel;
    [SerializeField] private GameObject exercisesDoctorPanel;
    [SerializeField] public GameObject recordDoctorPanel;
    [SerializeField] private GameObject stopDoctorPanel;
    [SerializeField] private GameObject previewDoctorPanel;
    [SerializeField] private GameObject feedbackDoctorPanel;
    [SerializeField] private GameObject drawForFeedbackDoctorPanel;
    [SerializeField] private GameObject exercisesPatientPanel;
    [SerializeField] private GameObject feedbackPatientPanel;
    [SerializeField] public GameObject recordPatientPanel;
    [SerializeField] private GameObject stopPatientPanel;
    [SerializeField] private GameObject previewPatientPanel;
    [SerializeField] private GameObject playFeedBackPatientPanel;

    private readonly Stack<GameObject> panelHistory = new Stack<GameObject>();
    private GameObject currentPanel;

    void Start()
    {
        AppState.CurrentUser = UserType.None;
        // mainAvatarGO.SetActive(false);
        // mirrorAvatarGO.SetActive(false);
        ShowOnly(menuPanel, remember:false);
    }

    public void OnBack() => GoBack();

    //public void OnDoctorSelected()
    //{
    //    AppState.CurrentUser = UserType.Doctor;
    //    ShowOnly(doctorPanel);
    //}

    //public void OnPatientSelected()
    //{
    //    AppState.CurrentUser = UserType.Patient;
    //    ShowOnly(patientPanel);
    //}

    private void SwapPresetOnBoth(int preset)
    {
        // Convert int -> "0" / "6" etc.  Zip source = built-in preset.
        string path = preset.ToString();
        const AssetSource src = AssetSource.Zip;

        if (mainAvatar != null) mainAvatar.ReloadAvatarManually(path, src);
        if (mirrorAvatar != null) mirrorAvatar.ReloadAvatarManually(path, src);
    }
    public void OnDoctorSelected()
    {
        AppState.CurrentUser = UserType.Doctor;

        // bring avatars back
        // mainAvatarGO.SetActive(true);
        // mirrorAvatarGO.SetActive(true);

        SwapPresetOnBoth(doctorPreset);   // your helper from earlier
        ShowOnly(doctorPanel);
    }

    public void OnPatientSelected()
    {
        AppState.CurrentUser = UserType.Patient;
        // mainAvatarGO.SetActive(true);
        // mirrorAvatarGO.SetActive(true);
        SwapPresetOnBoth(patientPreset);
        ShowOnly(patientPanel);
    }

    public void OnBackToMenu()
    {
        AppState.CurrentUser = UserType.None;
        // mainAvatarGO.SetActive(false);
        // mirrorAvatarGO.SetActive(false);
        ShowOnly(menuPanel);
    }
    
    public void OnBackToDoctor() => ShowOnly(doctorPanel);
    public void OnDoctorExercises() => ShowOnly(exercisesDoctorPanel);
    public void OnDoctorFeedback() => ShowOnly(feedbackDoctorPanel);
    public void OnDoctorPlay() => ShowOnly(playExerciseDoctorPanel);
    public void OnNewExercises() => ShowOnly(newExercisePanel);
    public void OnDoctorContinue() => ShowOnly(recordDoctorPanel);
    public void OnDoctorRecord() => ShowOnly(stopDoctorPanel);
    public void OnDoctorStop() => ShowOnly(previewDoctorPanel);
    public void OnDoctorDone() => ShowOnly(exercisesDoctorPanel);
    public void OnDoctorRedo() => ShowOnly(recordDoctorPanel);
    public void OnDoctorGiveFeedback() => ShowOnly(drawForFeedbackDoctorPanel);
    public void OnPatientFeedback() => ShowOnly(feedbackPatientPanel);
    public void OnPatientExercises() => ShowOnly(exercisesPatientPanel);
    public void OnPatientPerform() => ShowOnly(recordPatientPanel);
    public void OnPatientRecord() => ShowOnly(stopPatientPanel);
    public void OnPatientStop() => ShowOnly(previewPatientPanel);
    public void OnPatientPlay() => ShowOnly(playFeedBackPatientPanel);

public void ShowOnly(GameObject target, bool remember = true)
    {
        Debug.Log("show only" + target);
        if (target == null) { Debug.Log("null"); return; }
        if (remember && currentPanel != null)
            panelHistory.Push(currentPanel);

        foreach (GameObject panel in allPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }

        Debug.Log("setting" + target);
        target.SetActive(true);
        currentPanel = target;
    }

    private void GoBack()
    {
        if (panelHistory.Count > 0)
        {
            GameObject previous = panelHistory.Pop();
            ShowOnly(previous, remember:false);
        }
        else
        {
            ShowOnly(menuPanel, remember:false);
            AppState.CurrentUser = UserType.None;
        }
    }

}
