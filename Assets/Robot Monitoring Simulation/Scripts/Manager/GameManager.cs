
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private readonly Dictionary<string, RobotController>
        spawnedRobots = new Dictionary<string, RobotController>();

    private MockTelemetrySource telemetrySource;
    private GameObject robotPrefab;

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

    private void Reference()
    {
        telemetrySource = MockTelemetrySource.instance;
        robotPrefab = ResourceManager.instance.botPrefab;
    }

    private void Start()
    {
        Reference();
        RobotSpawnerStartOption();
    }

    private void RobotSpawnerStartOption()
    {
        if (telemetrySource == null || robotPrefab == null)
        {
            Debug.LogError(
                "RobotSpawner: Assign Telemetry Source and Robot Prefab.");
            enabled = false;
            return;
        }

        if (RobotPoolManager.Instance == null)
        {
            Debug.LogError(
                "RobotSpawner: RobotPoolManager not found in the scene.");
            enabled = false;
            return;
        }


        telemetrySource.TelemetryUpdated += OnTelemetryUpdated;

        foreach (RobotTelemetry data in telemetrySource.Robots)
        {
            OnTelemetryUpdated(data);
        }
    }

    private void OnDestroy()
    {
        if (telemetrySource != null)
        {
            telemetrySource.TelemetryUpdated -= OnTelemetryUpdated;
        }

        if (instance == this)
        {
            instance = null;
        }
    }

    private void OnTelemetryUpdated(RobotTelemetry data)
    {
        if (data == null || string.IsNullOrWhiteSpace(data.robotId))
            return;

        if (!spawnedRobots.TryGetValue(
            data.robotId, out RobotController controller))
        {
            GameObject robot = RobotPoolManager.Instance.GetRobot(
                data.position, Quaternion.identity);

            if (robot == null)
                return;

            robot.name = data.robotId;

            RobotIdentity identity =
                robot.GetComponent<RobotIdentity>();

            if (identity == null)
                identity = robot.AddComponent<RobotIdentity>();

            identity.Initialize(data.robotId);

            if (UIManager.instance != null)
            {
                UIManager.instance.UpdateRobotPopup(identity);
            }

            controller = robot.GetComponent<RobotController>();

            if (controller == null)
                controller = robot.AddComponent<RobotController>();

            spawnedRobots.Add(data.robotId, controller);
        }

        controller.SetTelemetry(data);
    }

    public RobotController GetRobot(string robotId)
    {
        spawnedRobots.TryGetValue(
            robotId, out RobotController controller);

        return controller;
    }
}
