
using UnityEngine;
using UnityEngine.EventSystems;

public class IslandCameraController : MonoBehaviour
{
    [Header("Island Bounds")]
    [SerializeField] private Transform islandBoundsRoot;
    [SerializeField] private float boundsRefreshInterval = 0.5f;
    [SerializeField] private float boundsPadding = 5f;

    [Header("Camera Target")]
    [SerializeField] private float targetHeight = 0f;
    [SerializeField] private float startingPitch = 45f;
    [SerializeField] private float minimumPitch = 25f;
    [SerializeField] private float maximumPitch = 75f;
    [SerializeField] private IslandTileSelection tileSelection;

    [Header("Zoom")]
    [SerializeField] private float distance = 76f;
    [SerializeField] private float minimumDistance = 18f;
    [SerializeField] private float maximumDistance = 100f;
    [SerializeField] private float zoomSpeed = 8f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 0.2f;

    [Header("Panning")]
    [SerializeField] private float panSpeed = 0.02f;
    [SerializeField] private float panSmoothness = 12f;

    [Header("Camera Smoothness")]
    [SerializeField] private float movementSmoothness = 10f;

    private Vector3 targetPosition;
    private Vector3 desiredTargetPosition;

    private float yaw;
    private float pitch;

    private float targetDistance;
    private float boundsTimer;

    private Bounds islandBounds;
    private bool hasBounds;
    private bool initialViewSet;

    private Vector2 previousTouchPosition;
    private bool singleTouchActive;

    private Camera controlledCamera;

    private void Awake()
    {
        controlledCamera = GetComponent<Camera>();

        if (controlledCamera == null)
        {
            Debug.LogError(
                "ISLAND CAMERA | Camera component missing."
            );
            enabled = false;
            return;
        }

        pitch = startingPitch;
        yaw = transform.eulerAngles.y;

        targetDistance = distance;

        RefreshIslandBounds();

        if (hasBounds)
        {
            targetPosition = islandBounds.center;
            targetPosition.y = targetHeight;
        }
        else
        {
            targetPosition = transform.position +
                transform.forward * distance;
            targetPosition.y = targetHeight;
        }

        desiredTargetPosition = targetPosition;
    }

    private void Update()
    {
        HandleMouseInput();
        HandleTouchInput();

        distance = Mathf.Lerp(
            distance,
            targetDistance,
            Time.deltaTime * movementSmoothness
        );

        targetPosition = Vector3.Lerp(
            targetPosition,
            desiredTargetPosition,
            Time.deltaTime * panSmoothness
        );

        boundsTimer += Time.deltaTime;

        if (boundsTimer >= boundsRefreshInterval)
        {
            boundsTimer = 0f;
            RefreshIslandBounds();
        }
    }

    private void LateUpdate()
    {
        // Wait for IslandTileVisuals to generate the tiles.
        // This runs before the camera renders its first frame.
        if (!initialViewSet)
        {
            RefreshIslandBounds();

            if (!initialViewSet)
                return;
        }

        Quaternion rotation = Quaternion.Euler(
            pitch,
            yaw,
            0f
        );

        Vector3 offset = rotation *
            new Vector3(0f, 0f, -distance);

        transform.position = targetPosition + offset;
        transform.LookAt(targetPosition);
    }

    private void OnEnable()
    {
        if (tileSelection != null)
            tileSelection.TileTransformSelected += FocusOnTile;
    }

    private void OnDisable()
    {
        if (tileSelection != null)
            tileSelection.TileTransformSelected -= FocusOnTile;
    }

    private void FocusOnTile(Transform selectedTile)
    {
        if (selectedTile == null)
        {
            ReturnToIslandCentre();
            return;
        }

        desiredTargetPosition = selectedTile.position;
        desiredTargetPosition.y = targetHeight;

        ClampTargetPosition();
    }

    private void ReturnToIslandCentre()
    {
        if (!hasBounds)
            return;

        Transform centreTile =
            islandBoundsRoot.Find("IslandTile_0_0");

        Vector3 centrePosition = centreTile != null
            ? centreTile.position
            : islandBounds.center;

        desiredTargetPosition = centrePosition;
        desiredTargetPosition.y = targetHeight;

        ClampTargetPosition();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButton(1) &&
            !IsPointerOverUI())
        {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed * 100f;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed * 100f;

            pitch = Mathf.Clamp(
                pitch,
                minimumPitch,
                maximumPitch
            );
        }

        if (Input.GetMouseButton(2) &&
            !IsPointerOverUI())
        {
            Vector3 right = transform.right;
            Vector3 forward = transform.forward;

            right.y = 0f;
            forward.y = 0f;

            right.Normalize();
            forward.Normalize();

            float scale = distance * panSpeed;

            desiredTargetPosition -=
                right * Input.GetAxis("Mouse X") * scale;

            desiredTargetPosition -=
                forward * Input.GetAxis("Mouse Y") * scale;

            ClampTargetPosition();
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (!IsPointerOverUI() && Mathf.Abs(scroll) > 0.001f)
        {
            targetDistance -= scroll * zoomSpeed * 10f;

            targetDistance = Mathf.Clamp(
                targetDistance,
                minimumDistance,
                maximumDistance
            );
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 0)
        {
            singleTouchActive = false;
            return;
        }

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (IsTouchOverUI(touch.fingerId))
            {
                singleTouchActive = false;
                return;
            }

            if (touch.phase == TouchPhase.Began)
            {
                previousTouchPosition = touch.position;
                singleTouchActive = true;
                return;
            }

            if (touch.phase == TouchPhase.Moved &&
                singleTouchActive)
            {
                Vector2 delta =
                    touch.position - previousTouchPosition;

                Pan(delta);

                previousTouchPosition = touch.position;
            }

            return;
        }

        singleTouchActive = false;

        if (Input.touchCount >= 2)
        {
            Touch first = Input.GetTouch(0);
            Touch second = Input.GetTouch(1);

            if (IsTouchOverUI(first.fingerId) ||
                IsTouchOverUI(second.fingerId))
                return;

            Vector2 firstPrevious =
                first.position - first.deltaPosition;

            Vector2 secondPrevious =
                second.position - second.deltaPosition;

            float previousDistance =
                Vector2.Distance(firstPrevious, secondPrevious);

            float currentDistance =
                Vector2.Distance(first.position, second.position);

            float pinchDifference =
                currentDistance - previousDistance;

            targetDistance -= pinchDifference * 0.02f;

            targetDistance = Mathf.Clamp(
                targetDistance,
                minimumDistance,
                maximumDistance
            );

            float previousAngle = Mathf.Atan2(
                secondPrevious.y - firstPrevious.y,
                secondPrevious.x - firstPrevious.x
            ) * Mathf.Rad2Deg;

            float currentAngle = Mathf.Atan2(
                second.position.y - first.position.y,
                second.position.x - first.position.x
            ) * Mathf.Rad2Deg;

            float angleDifference =
                Mathf.DeltaAngle(previousAngle, currentAngle);

            yaw -= angleDifference;
        }
    }

    private void Pan(Vector2 delta)
    {
        Vector3 right = transform.right;
        Vector3 forward = transform.forward;

        right.y = 0f;
        forward.y = 0f;

        right.Normalize();
        forward.Normalize();

        float scale = distance * panSpeed;

        desiredTargetPosition -=
            right * delta.x * scale;

        desiredTargetPosition -=
            forward * delta.y * scale;

        ClampTargetPosition();
    }

    private void RefreshIslandBounds()
    {
        if (islandBoundsRoot == null)
            return;

        Renderer[] renderers =
            islandBoundsRoot.GetComponentsInChildren<Renderer>();

        bool foundRenderer = false;
        Bounds newBounds = new Bounds();

        foreach (Renderer renderer in renderers)
        {
            if (!renderer.enabled)
                continue;

            if (!foundRenderer)
            {
                newBounds = renderer.bounds;
                foundRenderer = true;
            }
            else
            {
                newBounds.Encapsulate(renderer.bounds);
            }
        }

        if (!foundRenderer)
            return;

        newBounds.Expand(
            new Vector3(
                boundsPadding * 2f,
                0f,
                boundsPadding * 2f
            )
        );

        islandBounds = newBounds;
        hasBounds = true;

        if (!initialViewSet)
        {
            Transform centreTile =
                islandBoundsRoot.Find("IslandTile_0_0");

            Vector3 centrePosition = centreTile != null
                ? centreTile.position
                : islandBounds.center;

            targetPosition = centrePosition;
            targetPosition.y = targetHeight;

            desiredTargetPosition = targetPosition;

            // Start at exactly 76 units.
            distance = 76f;
            targetDistance = 76f;

            initialViewSet = true;
        }

        ClampTargetPosition();
    }

    private void ClampTargetPosition()
    {
        if (!hasBounds)
            return;

        desiredTargetPosition.x = Mathf.Clamp(
            desiredTargetPosition.x,
            islandBounds.min.x,
            islandBounds.max.x
        );

        desiredTargetPosition.z = Mathf.Clamp(
            desiredTargetPosition.z,
            islandBounds.min.z,
            islandBounds.max.z
        );

        desiredTargetPosition.y = targetHeight;
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject();
    }

    private bool IsTouchOverUI(int fingerId)
    {
        return EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject(fingerId);
    }

    public void ResetView()
    {
        if (!hasBounds)
            RefreshIslandBounds();

        if (!hasBounds)
            return;

        desiredTargetPosition = islandBounds.center;
        desiredTargetPosition.y = targetHeight;

        targetDistance = CalculateOverviewDistance();

        yaw = 0f;
        pitch = startingPitch;
    }

    private float CalculateOverviewDistance()
    {
        float verticalFov =
            controlledCamera.fieldOfView * Mathf.Deg2Rad;

        float horizontalFov = 2f * Mathf.Atan(
            Mathf.Tan(verticalFov / 2f) *
            controlledCamera.aspect
        );

        float horizontalDistance =
            islandBounds.extents.x /
            Mathf.Tan(horizontalFov / 2f);

        float verticalDistance =
            islandBounds.extents.z /
            Mathf.Tan(verticalFov / 2f);

        return Mathf.Clamp(
            Mathf.Max(horizontalDistance, verticalDistance) * 1.25f,
            minimumDistance,
            maximumDistance
        );
    }
}
