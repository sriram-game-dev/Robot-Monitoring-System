
using UnityEngine;
using TMPro;

public class RobotNamePopup : MonoBehaviour
{
    [Header("Robot Popup")]
    [SerializeField] private GameObject RobotNamePanel;
    [SerializeField] private TMP_Text robotNameText;

    [SerializeField] private RobotIdentity robotIdentity;

    private void Awake()
    {
        FindRobotIdentity();
    }

    private void Start()
    {
        // Retry after GameManager has initialized the robot.
        FindRobotIdentity();
    }

    private void FindRobotIdentity()
    {
        if (robotIdentity == null)
            robotIdentity = GetComponent<RobotIdentity>();

        if (robotIdentity == null)
            robotIdentity = GetComponentInParent<RobotIdentity>();

        if (robotIdentity == null)
            robotIdentity = GetComponentInChildren<RobotIdentity>(true);

        if (robotIdentity == null)
        {
            Debug.LogWarning(
                "RobotIdentity reference not found for " + name, this);
        }
    }

    private void LateUpdate()
    {
        if (robotIdentity == null ||
            CameraManager.Instance == null ||
            RobotNamePanel == null)
            return;

        bool isFollowing =
            CameraManager.Instance.IsFollowingRobot(robotIdentity);

        if (RobotNamePanel.activeSelf == isFollowing)
            RobotNamePanel.SetActive(!isFollowing);
    }

    public void SetRobotName(string robotName)
    {
        if (robotNameText != null)
            robotNameText.text = robotName;
    }
}
