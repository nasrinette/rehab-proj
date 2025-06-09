using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Provides ray-based UI interaction using hand tracking and pinch gestures.
/// Attach one instance to each hand controller/transform.
/// </summary>
public class HandRayInteractor : MonoBehaviour
{
    //[Header("Hand Configuration")]
    //[SerializeField] private OVRHand.Hand handType = OVRHand.Hand.HandLeft;
    //[SerializeField] private OVRHand ovrHand; // Reference to OVRHand component

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

    public Transform rightHand; // Reference to the right hand transform for parenting 
    //public RaycastHit hit;

    [SerializeField] private Canvas targetCanvas; // Assign in Inspector or find at runtime
    private GraphicRaycaster graphicRaycaster;


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

        //// Find OVRHand if not assigned
        //if (ovrHand == null)
        //{
        //    ovrHand = GetComponentInParent<OVRHand>();
        //    if (ovrHand == null)
        //    {
        //        Debug.LogError($"HandRayInteractor on {gameObject.name} couldn't find OVRHand component!");
        //    }
        //}

        // Set ray origin (usually from the hand transform)
        rayOrigin = transform;
        if (targetCanvas == null)
            targetCanvas = FindObjectOfType<Canvas>(); // fallback

        graphicRaycaster = targetCanvas.GetComponent<GraphicRaycaster>();


        StartCoroutine(AssignJointsAfterDelay());
    }

    private IEnumerator AssignJointsAfterDelay()
    {
        yield return new WaitForSeconds(5f);


        if (rightHand == null) rightHand = transform.parent.Find("Joint RightHandIndexProximal");

        // make rightHandLineRenderer child of righthand
        if (gameObject != null && rightHand != null)
        {
            gameObject.transform.SetParent(rightHand);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
        }
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
        hitIndicator.name = $"R_Hand_HitIndicator";
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
        // Show ray when hand is tracked
        lineRenderer.enabled = true;

        // Perform UI raycast
        GameObject hitObject;
        Vector3 hitPoint;
        bool hitSomething = PerformRaycast(out hitObject, out hitPoint);

        // Update ray visualization
        UpdateRayVisualization(hitPoint, hitSomething);

        // Check for pinch
        bool isPinching = IsPinching();

        // Handle interactions
        HandleInteraction(hitObject, hitSomething, isPinching, hitPoint);

        // Update pinch state for next frame
        wasPinching = isPinching;
    }


    //bool PerformRaycast(out RaycastHit hit)
    //{
    //    Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

    //    // First try to hit UI elements in world space
    //    if (Physics.Raycast(ray, out hit, rayLength, uiLayerMask))
    //    {
    //        // Check if hit object has a UI component
    //        if (hit.collider.GetComponent<Graphic>() != null)
    //        {
    //            return true;
    //        }
    //    }

    //    return false;
    //}

    // Returns true if any Button is hit, and outputs the closest Button's RaycastHit
    //bool PerformRaycast(out RaycastHit hit)
    //{
    //    Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
    //    //RaycastHit[] hits = Physics.RaycastAll(ray, rayLength, uiLayerMask);

    //    // add graphic raycaster for ui


    //    float closestDistance = float.MaxValue;
    //    hit = default;
    //    bool foundButton = false;

    //    foreach (var h in hits)
    //    {
    //        Debug.Log($"Hit: {h.collider.name} at {h.point}");
    //        if (h.collider.GetComponent<Button>() != null)
    //        {
    //            float dist = Vector3.Distance(ray.origin, h.point);
    //            if (dist < closestDistance)
    //            {
    //                closestDistance = dist;
    //                hit = h;
    //                foundButton = true;
    //            }
    //        }
    //    }

    //    return foundButton;
    //}

    bool PerformRaycast(out GameObject hitObject, out Vector3 hitPoint)
    {
        hitObject = null;
        hitPoint = Vector3.zero;

        // Create a PointerEventData at the ray's screen position
        Vector3 rayOriginPos = rayOrigin.position;
        Vector3 rayDirection = rayOrigin.forward;

        // Project a point far along the ray and convert to screen space
        Vector3 worldPoint = rayOriginPos + rayDirection * rayLength;
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPoint);

        pointerEventData.position = screenPoint;

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, results);

        foreach (var result in results)
        {
            Debug.Log($"Hit UI Element: {result.gameObject.name} at {result.worldPosition}");
            Button button = result.gameObject.GetComponent<Button>();
            if (button != null)
            {
                hitObject = result.gameObject;
                // Try to get the world position of the hit (approximate)
                RectTransform rect = hitObject.GetComponent<RectTransform>();
                if (rect != null)
                    hitPoint = rect.position;
                return true;
            }
        }
        return false;
    }



    void UpdateRayVisualization(Vector3 hitPoint, bool hitSomething)
    {
        Vector3 startPoint = rayOrigin.position;
        Vector3 endPoint;

        if (hitSomething)
        {
            endPoint = hitPoint;
            hitIndicator.SetActive(true);
            hitIndicator.transform.position = hitPoint;

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
        float triggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        return triggerValue > pinchThreshold;
    }

    void HandleInteraction(GameObject hitObject, bool hitSomething, bool isPinching, Vector3 hitPoint)
    {
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
                pointerEventData.position = Camera.main.WorldToScreenPoint(hitPoint);
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