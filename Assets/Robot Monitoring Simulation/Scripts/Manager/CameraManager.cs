using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public enum CameraMode
    {
        Free,
        Follow
    }

    [Header("Camera Settings")]
    [SerializeField] private CameraMode currentMode = CameraMode.Free;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float followDistance = 6f;
    [SerializeField] private float followHeight = 4f;
    [SerializeField] private float smoothSpeed = 5f;


    private UIManager selectionUI;

    private float yaw;
    private float pitch = 35f;

    public static CameraManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
    void Reference()
    {
        selectionUI = UIManager.instance;
    }
    private void Start()
    {
        //Reference
        Reference();
        //Camera Settings
        UpdateCameraModeText();
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (currentMode == CameraMode.Free)
            {
                if (selectionUI != null &&
                    selectionUI.SelectedRobot != null)
                {
                    currentMode = CameraMode.Follow;
                }
                else
                {
                    Debug.Log("Select a robot before enabling Follow Camera.");
                }
            }
            else
            {
                currentMode = CameraMode.Free;
            }
        }

        // Continuously synchronize the UI text with the camera mode.
        UpdateCameraModeText();

        if (currentMode == CameraMode.Free)
            HandleFreeCamera();
        else
            HandleFollowCamera();
    }
    private void UpdateCameraModeText()
    {
        if (selectionUI == null)
            return;

        if (currentMode == CameraMode.Free)
            selectionUI.SetCameraModeText("Free");
        else
            selectionUI.SetCameraModeText("Follow");
    }

    private void HandleFreeCamera()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 movement =
            transform.right * x + transform.forward * z;

        transform.position +=
            movement * moveSpeed * Time.deltaTime;

        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -10f, 80f);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }
    public bool IsFollowingRobot(RobotIdentity identity)
    {
        if (identity == null ||
            currentMode != CameraMode.Follow ||
            selectionUI == null ||
            selectionUI.SelectedRobot == null)
        {
            return false;
        }

        RobotIdentity selectedIdentity =
            selectionUI.SelectedRobot.GetComponent<RobotIdentity>();

        return selectedIdentity == identity;
    }

    private void HandleFollowCamera()
    {
        if (selectionUI == null ||
            selectionUI.SelectedRobot == null)
        {
            currentMode = CameraMode.Free;
            return;
        }

        Transform target = selectionUI.SelectedRobot.transform;

        Vector3 desiredPosition =
            target.position
            - target.forward * followDistance
            + Vector3.up * followHeight;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        Vector3 lookPoint = target.position + Vector3.up;
        Vector3 direction = lookPoint - transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion desiredRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                smoothSpeed * Time.deltaTime
            );
        }
    }
}
