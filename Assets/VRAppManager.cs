using UnityEngine;
using Oculus.Avatar2;
using Oculus.Interaction;

public class VRAppManager : MonoBehaviour
{
    [Header("UI Setup")]
    public Transform uiParent;
    public Material buttonMaterial;
    public Material textMaterial;

    [Header("Avatar Setup")]
    public OvrAvatarEntity patientAvatarPrefab;
    public OvrAvatarEntity doctorAvatarPrefab;

    private GameObject userSelectionUI;
    private GameObject patientUI;
    private GameObject doctorUI;
    private OvrAvatarEntity currentAvatar;

    public enum UserType { None, Patient, Doctor }
    private UserType selectedUserType = UserType.None;

    void Start()
    {
        CreateUserSelectionUI();
    }

    void CreateUserSelectionUI()
    {
        // Create main UI panel programmatically
        userSelectionUI = CreateUIPanel("UserSelection", Vector3.forward * 3f);

        // Title
        CreateText(userSelectionUI.transform, "Select User Type", Vector3.up * 0.5f, 0.1f);

        // Patient Button
        var patientBtn = CreateButton(userSelectionUI.transform, "Patient", Vector3.left * 0.5f,
            () => SelectUserType(UserType.Patient));

        // Doctor Button
        var doctorBtn = CreateButton(userSelectionUI.transform, "Doctor", Vector3.right * 0.5f,
            () => SelectUserType(UserType.Doctor));
    }

    GameObject CreateUIPanel(string name, Vector3 position)
    {
        var panel = new GameObject(name);
        panel.transform.position = position;
        panel.transform.parent = uiParent;
        return panel;
    }

    GameObject CreateButton(Transform parent, string text, Vector3 localPos, System.Action onClick)
    {
        // Create button cube
        var button = GameObject.CreatePrimitive(PrimitiveType.Cube);
        button.transform.parent = parent;
        button.transform.localPosition = localPos;
        button.transform.localScale = Vector3.one * 0.3f;
        button.GetComponent<Renderer>().material = buttonMaterial;

        // Add interactable component
        var interactable = button.AddComponent<PokeInteractable>();
        var selectableInteractable = button.AddComponent<RayInteractable>();

        // Add button logic
        var buttonComponent = button.AddComponent<InteractableUnityEventWrapper>();
        buttonComponent.WhenSelect.AddListener(() => onClick?.Invoke());

        // Add text
        CreateText(button.transform, text, Vector3.forward * 0.2f, 0.05f);

        return button;
    }

    void CreateText(Transform parent, string text, Vector3 localPos, float scale)
    {
        var textObj = new GameObject("Text");
        textObj.transform.parent = parent;
        textObj.transform.localPosition = localPos;
        textObj.transform.localScale = Vector3.one * scale;

        var textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.fontSize = 100;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.GetComponent<Renderer>().material = textMaterial;
    }

    void SelectUserType(UserType userType)
    {
        selectedUserType = userType;
        userSelectionUI.SetActive(false);

        // Load appropriate avatar
        LoadAvatar(userType);

        // Create user-specific UI
        CreateUserSpecificUI(userType);
    }

    void LoadAvatar(UserType userType)
    {
        var avatarPrefab = userType == UserType.Patient ? patientAvatarPrefab : doctorAvatarPrefab;
        currentAvatar = Instantiate(avatarPrefab);

        // Configure avatar for local user
        currentAvatar.SetIsLocal(true);

        // The avatar will automatically initialize when set as local
        // You can also set user ID if needed:
        // currentAvatar.SetUserId(0); // Use appropriate user ID
    }

    void CreateUserSpecificUI(UserType userType)
    {
        if (userType == UserType.Patient)
        {
            CreatePatientUI();
        }
        else
        {
            CreateDoctorUI();
        }
    }

    void CreatePatientUI()
    {
        patientUI = CreateUIPanel("PatientUI", Vector3.forward * 2f + Vector3.up * 0.5f);

        CreateText(patientUI.transform, "Patient Interface", Vector3.up * 0.3f, 0.08f);

        // Patient-specific buttons
        CreateButton(patientUI.transform, "Health Status", Vector3.left * 0.4f,
            () => Debug.Log("Show Health Status"));

        CreateButton(patientUI.transform, "Symptoms", Vector3.right * 0.4f,
            () => Debug.Log("Report Symptoms"));
    }

    void CreateDoctorUI()
    {
        doctorUI = CreateUIPanel("DoctorUI", Vector3.forward * 2f + Vector3.up * 0.5f);

        CreateText(doctorUI.transform, "Doctor Interface", Vector3.up * 0.3f, 0.08f);

        // Doctor-specific buttons
        CreateButton(doctorUI.transform, "Patient Data", Vector3.left * 0.6f,
            () => Debug.Log("View Patient Data"));

        CreateButton(doctorUI.transform, "Diagnosis", Vector3.zero,
            () => Debug.Log("Create Diagnosis"));

        CreateButton(doctorUI.transform, "Treatment", Vector3.right * 0.6f,
            () => Debug.Log("Prescribe Treatment"));
    }
}

// Ray Interaction Setup Component
public class RayInteractionSetup : MonoBehaviour
{
    void Start()
    {
        // Ensure ray interactor is enabled on controllers
        var rayInteractors = FindObjectsOfType<RayInteractor>();
        foreach (var ray in rayInteractors)
        {
            ray.enabled = true;
        }
    }
}