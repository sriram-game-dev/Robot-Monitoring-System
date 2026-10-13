using UnityEngine;
using System.Collections.Generic;
public class GameManager : MonoBehaviour
{


    //Reference
    public static GameManager instance;

  private readonly Dictionary<string, RobotController>
        spawnedRobots =
            new Dictionary<string, RobotController>();

    //Reference
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

    void Reference()
    {
        //Scripts
        telemetrySource = MockTelemetrySource.instance;

        //Object
        robotPrefab = ResourceManager.instance.botPrefab;
    }

    private void Start()
    {
        //Reference
        Reference();

        //Robot Spawner Logic At Start()
        RobotSpawnerStartOption();
        

    }

//-------------------------------------------------
//-------------Robot Spawner Logic------------------
//-------------------------------------------------

    void RobotSpawnerStartOption()
    {
        if (telemetrySource == null || robotPrefab == null)
        {
            Debug.LogError(
                "RobotSpawner: Assign Telemetry Source and Robot Prefab.");
            enabled = false;
            return;
        }

        // Subscribe before reading existing telemetry.
        telemetrySource.TelemetryUpdated += OnTelemetryUpdated;

        // Handles either script starting first.
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
    }

    private void OnTelemetryUpdated(RobotTelemetry data)
    {
        if (data == null)
            return;

        if (!spawnedRobots.TryGetValue(
                data.robotId, out RobotController controller))
        {
            GameObject robot = Instantiate(
                robotPrefab, data.position, Quaternion.identity);

            robot.name = data.robotId;

            RobotIdentity identity =
                robot.GetComponent<RobotIdentity>();

            if (identity == null)
                identity = robot.AddComponent<RobotIdentity>();

            identity.Initialize(data.robotId);

            controller =
                robot.GetComponent<RobotController>();

            if (controller == null)
                controller =
                    robot.AddComponent<RobotController>();

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

//-------------------------------------------------
//-------------Robot Spawner Logic------------------
//-------------------------------------------------


}
