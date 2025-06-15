using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UserSelector : MonoBehaviour
{
    public GameObject menuPanel;
    public GameObject doctorPanel;
    public GameObject patientPanel;
    public GameObject newExercisePanel;
    public GameObject exercisesDoctorPanel;
    public GameObject recordDoctorPanel;
    public GameObject stopDoctorPanel;
    public GameObject previewDoctorPanel;
    public GameObject feedbackDoctorPanel;
    public GameObject exercisesPatientPanel;
    public GameObject feedbackPatientPanel;
    public GameObject recordPatientPanel;
    public GameObject stopPatientPanel;
    public GameObject previewPatientPanel;
    public GameObject playFeedBackPatientPanel;

    private readonly Stack<GameObject> panelHistory = new Stack<GameObject>();
    private GameObject currentPanel;

    void Start()
    {
        AppState.CurrentUser = UserType.None;
        ShowOnly(menuPanel, remember:false);
    }

    public void OnBack() => GoBack();

    public void OnDoctorSelected()
    {
        AppState.CurrentUser = UserType.Doctor;
        ShowOnly(doctorPanel);
    }

    public void OnPatientSelected()
    {
        AppState.CurrentUser = UserType.Patient;
        ShowOnly(patientPanel);
    }

    public void OnBackToMenu()
    {
        AppState.CurrentUser = UserType.None;
        ShowOnly(menuPanel);
    }

    public void OnDoctorExercises()
    {
        AppState.CurrentUser = UserType.Doctor;
        ShowOnly(exercisesDoctorPanel);
    }

    public void OnDoctorFeedback()
    {
        AppState.CurrentUser = UserType.Doctor;
        ShowOnly(feedbackDoctorPanel);
    }

    public void OnDoctorEdit()
    {
        AppState.CurrentUser = UserType.Doctor;
        ShowOnly(recordDoctorPanel);
    }

    public void OnNewExercises()
    {
        AppState.CurrentUser = UserType.Doctor;
        ShowOnly(newExercisePanel);
    }

    public void OnDoctorContinue()
    {
        AppState.CurrentUser = UserType.Doctor;
        ShowOnly(recordDoctorPanel);
    }

    public void OnDoctorRecord()
    {
        AppState.CurrentUser = UserType.Doctor;
        ShowOnly(stopDoctorPanel);
    }

    public void OnDoctorStop()
    {
        AppState.CurrentUser = UserType.Doctor;
        ShowOnly(previewDoctorPanel);
    }

    public void OnDoctorDone()
    {
        AppState.CurrentUser = UserType.Doctor;
        ShowOnly(exercisesDoctorPanel);
    }

    public void OnDoctorRedo()
    {
        AppState.CurrentUser = UserType.Doctor;
        ShowOnly(recordDoctorPanel);
    }

    public void OnPatientFeedback()
    {
        AppState.CurrentUser = UserType.Patient;
        ShowOnly(feedbackPatientPanel);
    }

    public void OnPatientExercises()
    {
        AppState.CurrentUser = UserType.Patient;
        ShowOnly(exercisesPatientPanel);
    }

    public void OnPatientPerform()
    {
        AppState.CurrentUser = UserType.Patient;
        ShowOnly(recordPatientPanel);
    }

    public void OnPatientRecord()
    {
        AppState.CurrentUser = UserType.Patient;
        ShowOnly(stopPatientPanel);
    }

    public void OnPatientStop()
    {
        AppState.CurrentUser = UserType.Patient;
        ShowOnly(previewPatientPanel);
    }

    public void OnPatientPlay()
    {
        AppState.CurrentUser = UserType.Patient;
        ShowOnly(playFeedBackPatientPanel);
    }

private void ShowOnly(GameObject target, bool remember = true)
    {
        Debug.Log("show only" + target);
        if (target == null) { Debug.Log("null"); return; };
        if (remember && currentPanel != null)
            panelHistory.Push(currentPanel);

        menuPanel.SetActive(false);
        doctorPanel.SetActive(false);
        patientPanel.SetActive(false);
        newExercisePanel.SetActive(false);
        exercisesDoctorPanel.SetActive(false);
        recordDoctorPanel.SetActive(false);
        stopDoctorPanel.SetActive(false);
        previewDoctorPanel.SetActive(false);
        feedbackDoctorPanel.SetActive(false);
        exercisesPatientPanel.SetActive(false);
        feedbackPatientPanel.SetActive(false);
        recordPatientPanel.SetActive(false);
        stopPatientPanel.SetActive(false);
        previewPatientPanel.SetActive(false);
        playFeedBackPatientPanel.SetActive(false);

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
