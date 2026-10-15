
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
    [SerializeField] private TMP_Text robotNameText;
    [SerializeField] private TMP_Text positionText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text headingText;
    [SerializeField] private TMP_Text dataAgeText;
    [SerializeField] private TMP_Text statusText;


    [Header("Warning Panel")]
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private TMP_Text warningText;

    [Header("Camera Panel")]
    [SerializeField] private GameObject CameraPanel;
    [SerializeField] private TMP_Text CameraModeText;




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

        // Robot ID
        RobotIdentity identity =
            selectedRobot.GetComponent<RobotIdentity>();

        string robotId = identity != null &&
                         !string.IsNullOrEmpty(identity.UnitId)
            ? identity.UnitId
            : data.robotId;

        if (robotIdText != null)
            robotIdText.text = "ID: " + robotId;
        // Robot Name
        if (robotNameText != null)
        {
            string robotName = GetRobotName(robotId);
            robotNameText.text = "Name: " + robotName;
        }
        // Speed
        if (speedText != null)
            speedText.text = $"Speed: {data.speed:F1} m/s";

        // Heading
        if (headingText != null)
            headingText.text = $"Heading: {data.heading:F0} deg";

        // Position
        if (positionText != null)
            positionText.text =
                $"Position: X {data.position.x:F1} / " +
                $"Z {data.position.z:F1}";

        // Data Age
        float dataAge = Mathf.Max(
            0f, Time.time - data.lastUpdateTime);

        if (dataAgeText != null)
            dataAgeText.text = $"Data Age: {dataAge:F1} sec";

        // Status
        bool dataLost = !data.isConnected || dataAge >= 3f;

        if (statusText != null)
        {
            statusText.text = dataLost
                ? "Status: DATA LOST"
                : "Status: NORMAL";

            statusText.color = dataLost
                ? new Color32(185, 28, 28, 255)   // Red: Data lost
                : new Color32(21, 128, 61, 255);  // Green: Normal
        }
    }
    private string GetRobotName(string robotId)
    {
        if (string.IsNullOrEmpty(robotId) ||
            !robotId.StartsWith("BOT_"))
            return robotId;

        if (!int.TryParse(robotId.Substring(4), out int number) ||
            number < 1)
            return robotId;

        string name = "";

        while (number > 0)
        {
            number--;
            name = (char)('A' + number % 26) + name;
            number /= 26;
        }

        return "ROBOT_" + name;
    }
    public void UpdateRobotPopup(RobotIdentity identity)
    {
        if (identity == null)
            return;

        RobotNamePopup popup =
            identity.GetComponentInChildren<RobotNamePopup>(true);

        if (popup != null)
        {
            popup.SetRobotName(GetRobotName(identity.UnitId));
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

        if (warningText == null)
            return;

        if (!hasWarnings)
        {
            warningText.text = "";
            return;
        }

        int maxVisibleWarnings = 5;
        List<string> formattedMessages = new List<string>();

        int count = 0;

        foreach (var warning in activeWarnings)
        {
            if (count >= maxVisibleWarnings)
                break;

            Color color = GetWarningColor(warning.Key);
            string hexColor = ColorUtility.ToHtmlStringRGB(color);

            formattedMessages.Add(
                $"<color=#{hexColor}>{warning.Value}</color>");

            count++;
        }

        warningText.text =
            "<color=#1D4ED8><b>ACTIVE WARNINGS</b></color>\n\n" +
            string.Join("\n\n", formattedMessages);
    }

    //--------------------------------------------------//
    //-----------------Robot Warning UI Visual-----------------//
    //-------------------------------------------------//

    private Color GetWarningColor(string warningKey)
    {
        if (string.IsNullOrEmpty(warningKey))
            return new Color32(202, 138, 4, 255);

        if (warningKey.IndexOf(
            "proximity", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return new Color32(234, 88, 12, 255); // Orange

        if (warningKey.IndexOf(
            "restricted", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return new Color32(220, 38, 38, 255); // Red

        if (warningKey.IndexOf(
            "data", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return new Color32(185, 28, 28, 255); // Dark red

        return new Color32(202, 138, 4, 255); // Amber
    }
    //--------------------------------------------------//
    //-----------------Camera UI Visual-----------------//
    //-------------------------------------------------//

    public void SetCameraModeText(string mode)
    {
        if (CameraModeText != null)
            CameraModeText.text = mode;
    }

}
