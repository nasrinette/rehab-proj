using UnityEngine;
using UnityEngine.SceneManagement;

public class UserSelector : MonoBehaviour
{
    public GameObject menuPanel;
    public GameObject doctorPanel;
    public GameObject patientPanel;

    public void OnDoctorSelected()
    {
        AppState.CurrentUser = UserType.Doctor;
        menuPanel.SetActive(false);
        doctorPanel.SetActive(true);
    }

    public void OnPatientSelected()
    {
        AppState.CurrentUser = UserType.Patient;
        menuPanel.SetActive(false);
        patientPanel.SetActive(true);
    }
}
