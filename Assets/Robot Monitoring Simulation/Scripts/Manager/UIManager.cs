
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [Header("Robot Panel")]
    [SerializeField] private GameObject detailsPanel;
    [SerializeField] private TMP_Text robotIdText;
    [SerializeField] private TMP_Text positionText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text headingText;
    [SerializeField] private TMP_Text statusText;

    [Header("Warning UI")]
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private TMP_Text warningText;

    private readonly Dictionary<string, string> activeWarnings =
    new Dictionary<string, string>();
    private Camera mainCamera;

    private RobotController selectedRobot;

    public RobotController SelectedRobot => selectedRobot;
    public static UIManager instance;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Reference()
    {
        mainCamera = ResourceManager.instance.mainCamera;
    }

    private void Start()
    {

        Reference();
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (detailsPanel != null)
            detailsPanel.SetActive(false);
        //---------Warning------------
        RefreshWarningUI();
    }

    private void Update()
    {
        HandleRobotSelection();
        RefreshDetails();
    }
    //--------------------------------------------------//
    //-----------------Robot Detail UI-----------------//
    //-------------------------------------------------//
    private void HandleRobotSelection()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (mainCamera == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        RobotIdentity identity =
            hit.collider.GetComponentInParent<RobotIdentity>();

        if (identity == null)
            return;

        selectedRobot = identity.GetComponent<RobotController>();

        if (selectedRobot != null && detailsPanel != null)
            detailsPanel.SetActive(true);
    }

    private void RefreshDetails()
    {
        if (selectedRobot == null ||
            selectedRobot.CurrentTelemetry == null)
            return;

        RobotTelemetry data = selectedRobot.CurrentTelemetry;

        if (robotIdText != null)
            robotIdText.text = "Robot ID: " + data.robotId;

        if (positionText != null)
            positionText.text =
                $"Position: ({data.position.x:F2}, " +
                $"{data.position.y:F2}, {data.position.z:F2})";

        if (speedText != null)
            speedText.text = $"Speed: {data.speed:F2}";

        if (headingText != null)
            headingText.text = $"Heading: {data.heading:F1}°";

        if (statusText != null)
        {
            statusText.text = data.isConnected
                ? "Status: CONNECTED"
                : "Status: DATA LOST";

            statusText.color = data.isConnected
                ? Color.green
                : Color.red;
        }
    }
    //--------------------------------------------------//
    //-------------------------------------------------//

    //--------------------------------------------------//
    //-----------------Robot Warning UI-----------------//
    //-------------------------------------------------//
    public void ShowWarning(string warningKey, string message)
    {
        activeWarnings[warningKey] = message;
        RefreshWarningUI();
    }

    public void ClearWarning(string warningKey)
    {
        activeWarnings.Remove(warningKey);
        RefreshWarningUI();
    }

    private void RefreshWarningUI()
    {
        bool hasWarnings = activeWarnings.Count > 0;

        if (warningPanel != null)
            warningPanel.SetActive(hasWarnings);

        if (warningText != null)
        {
            int maxVisibleWarnings = 5;

            List<string> messages = new List<string>(
                activeWarnings.Values);

            int visibleCount = Mathf.Min(
                messages.Count, maxVisibleWarnings);

            messages = messages.GetRange(0, visibleCount);

            warningText.text = hasWarnings
                ? " ACTIVE WARNINGS \n\n"
                    + string.Join("\n\n", messages)
                : "";
        }
    }

}
