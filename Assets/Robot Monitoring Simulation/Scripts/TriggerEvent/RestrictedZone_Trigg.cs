using UnityEngine;

public class RestrictedZone : MonoBehaviour
{
    [SerializeField] private string zoneName = "Restricted Zone";

    private void OnTriggerEnter(Collider other)
    {
        RobotController robot =
            other.GetComponentInParent<RobotController>();

        if (robot == null)
            return;

        string robotId = GetRobotId(robot);
        string warningKey = robotId + "_" + zoneName;
        string message = robotId + " entered " + zoneName;

        if (UIManager.instance != null)
        {
            UIManager.instance.ShowWarning(
                warningKey, message);
        }

        Debug.LogWarning(message);
    }

    private void OnTriggerExit(Collider other)
    {
        RobotController robot =
            other.GetComponentInParent<RobotController>();

        if (robot == null)
            return;

        string robotId = GetRobotId(robot);
        string warningKey = robotId + "_" + zoneName;

        if (UIManager.instance != null)
        {
            UIManager.instance.ClearWarning(warningKey);
        }

        Debug.Log(robotId + " exited " + zoneName);
    }

    private string GetRobotId(RobotController robot)
    {
        if (robot.CurrentTelemetry != null)
            return robot.CurrentTelemetry.robotId;

        return robot.gameObject.name;
    }
}
