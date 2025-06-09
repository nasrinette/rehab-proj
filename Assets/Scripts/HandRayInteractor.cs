using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Provides ray-based UI interaction using hand tracking and pinch gestures.
/// Attach one instance to each hand controller/transform.
/// </summary>
public class HandRayInteractor : MonoBehaviour
{
    [Header("Hand Configuration")]
    [SerializeField] private OVRHand.Hand handType = OVRHand.Hand.HandLeft;
    [SerializeField] private OVRHand ovrHand; // Reference to OVRHand component

    [Header("Ray Configuration")]
    [SerializeField] private float rayLength = 10f;
    [SerializeField] private float rayWidth = 0.01f;
    [SerializeField] private Color rayDefaultColor = Color.white;
    [SerializeField] private Color rayHoverColor = Color.green;
    [SerializeField] private Color rayPinchColor = Color.yellow;

    [Header("Interaction")]
    [SerializeField] private float pinchThreshold = 0.8f; // How close fingers need to be for pinch
    [SerializeField] private LayerMask uiLayerMask = -1; // Which layers can be interacted with

    // Components
    private LineRenderer lineRenderer;
    private GameObject hitIndicator;
    private Transform rayOrigin;

    // State tracking
    private bool wasPinching = false;
    private GameObject currentHoveredObject;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;

    void Start()
    {
        // Set up the ray visualization
        SetupRayVisualization();

        // Get or create event system
        eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystem = eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
        }

        pointerEventData = new PointerEventData(eventSystem);

        // Find OVRHand if not assigned
        if (ovrHand == null)
        {
            ovrHand = GetComponentInParent<OVRHand>();
            if (ovrHand == null)
            {
                Debug.LogError($"HandRayInteractor on {gameObject.name} couldn't find OVRHand component!");
            }
        }

        // Set ray origin (usually from the hand transform)
        rayOrigin = transform;
    }

    void SetupRayVisualization()
    {
        // Create line renderer for the ray
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = rayWidth;
        lineRenderer.endWidth = rayWidth * 0.5f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.positionCount = 2;

        // Create hit indicator (small sphere at ray hit point)
        hitIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hitIndicator.name = $"{handType}Hand_HitIndicator";
        hitIndicator.transform.localScale = Vector3.one * 0.02f;
        hitIndicator.GetComponent<Collider>().enabled = false;

        // Make hit indicator unlit and set initial color
        Renderer indicatorRenderer = hitIndicator.GetComponent<Renderer>();
        indicatorRenderer.material = new Material(Shader.Find("Sprites/Default"));
        indicatorRenderer.material.color = rayDefaultColor;

        hitIndicator.SetActive(false);
    }

    void Update()
    {
        if (ovrHand == null || !ovrHand.IsTracked)
        {
            // Hide ray when hand isn't tracked
            lineRenderer.enabled = false;
            hitIndicator.SetActive(false);
            return;
        }

        // Show ray when hand is tracked
        lineRenderer.enabled = true;

        // Perform raycast
        bool hitSomething = PerformRaycast(out RaycastHit hit);

        // Update ray visualization
        UpdateRayVisualization(hit, hitSomething);

        // Check for pinch
        bool isPinching = IsPinching();

        // Handle interactions
        HandleInteraction(hit, hitSomething, isPinching);

        // Update pinch state for next frame
        wasPinching = isPinching;
    }

    bool PerformRaycast(out RaycastHit hit)
    {
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        // First try to hit UI elements in world space
        if (Physics.Raycast(ray, out hit, rayLength, uiLayerMask))
        {
            // Check if hit object has a UI component
            if (hit.collider.GetComponent<Graphic>() != null)
            {
                return true;
            }
        }

        return false;
    }

    void UpdateRayVisualization(RaycastHit hit, bool hitSomething)
    {
        Vector3 startPoint = rayOrigin.position;
        Vector3 endPoint;

        if (hitSomething)
        {
            endPoint = hit.point;
            hitIndicator.SetActive(true);
            hitIndicator.transform.position = hit.point;

            // Update colors based on state
            Color currentColor = rayDefaultColor;
            if (IsPinching())
            {
                currentColor = rayPinchColor;
            }
            else if (currentHoveredObject != null)
            {
                currentColor = rayHoverColor;
            }

            lineRenderer.startColor = currentColor;
            lineRenderer.endColor = currentColor;
            hitIndicator.GetComponent<Renderer>().material.color = currentColor;
        }
        else
        {
            endPoint = startPoint + rayOrigin.forward * rayLength;
            hitIndicator.SetActive(false);

            lineRenderer.startColor = rayDefaultColor;
            lineRenderer.endColor = rayDefaultColor * 0.3f; // Fade at end
        }

        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
    }

    bool IsPinching()
    {
        if (ovrHand == null) return false;

        // Get pinch strength from OVRHand
        float pinchStrength = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        return pinchStrength > pinchThreshold;
    }

    void HandleInteraction(RaycastHit hit, bool hitSomething, bool isPinching)
    {
        GameObject hitObject = hitSomething ? hit.collider.gameObject : null;

        // Handle hover enter/exit
        if (hitObject != currentHoveredObject)
        {
            // Exit previous hover
            if (currentHoveredObject != null)
            {
                ExecuteEvents.Execute(currentHoveredObject, pointerEventData, ExecuteEvents.pointerExitHandler);
            }

            // Enter new hover
            if (hitObject != null)
            {
                ExecuteEvents.Execute(hitObject, pointerEventData, ExecuteEvents.pointerEnterHandler);
            }

            currentHoveredObject = hitObject;
        }

        // Handle pinch interactions
        if (hitObject != null)
        {
            // Update pointer position for UI system
            if (hitSomething)
            {
                pointerEventData.position = Camera.main.WorldToScreenPoint(hit.point);
            }

            // Pinch started
            if (isPinching && !wasPinching)
            {
                ExecuteEvents.Execute(hitObject, pointerEventData, ExecuteEvents.pointerDownHandler);

                // For buttons, also trigger click immediately
                Button button = hitObject.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.Invoke();
                }
            }
            // Pinch released
            else if (!isPinching && wasPinching)
            {
                ExecuteEvents.Execute(hitObject, pointerEventData, ExecuteEvents.pointerUpHandler);
                ExecuteEvents.Execute(hitObject, pointerEventData, ExecuteEvents.pointerClickHandler);
            }
        }
    }

    void OnDestroy()
    {
        if (hitIndicator != null)
        {
            Destroy(hitIndicator);
        }
    }
}