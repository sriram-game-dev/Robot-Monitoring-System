
using System.Collections.Generic;
using UnityEngine;

public class RobotPoolManager : MonoBehaviour
{
    public static RobotPoolManager Instance { get; private set; }

    private GameObject robotPrefab;
    private int initialPoolSize;

    private readonly Queue<GameObject> availableRobots =
        new Queue<GameObject>();

    private readonly HashSet<GameObject> pooledRobots =
        new HashSet<GameObject>();

    private bool initialized;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        if (initialized)
            return;

        if (ResourceManager.instance == null)
        {
            Debug.LogError("RobotPoolManager: ResourceManager not found.");
            return;
        }

        if (MockTelemetrySource.instance == null)
        {
            Debug.LogError("RobotPoolManager: MockTelemetrySource not found.");
            return;
        }

        robotPrefab = ResourceManager.instance.botPrefab;
        initialPoolSize = Mathf.Max(
            1, MockTelemetrySource.instance.robotCount);

        BuildPool();
    }

    // Allows automated tests to supply their own prefab and pool size.
    public void InitializeForTesting(GameObject prefab, int poolSize)
    {
        if (initialized)
        {
            Debug.LogError("RobotPoolManager is already initialized.");
            return;
        }

        if (prefab == null)
        {
            Debug.LogError("RobotPoolManager: Test prefab is missing.");
            return;
        }

        robotPrefab = prefab;
        initialPoolSize = Mathf.Max(1, poolSize);

        BuildPool();
    }

    private void BuildPool()
    {
        if (initialized || robotPrefab == null)
            return;

        initialized = true;

        for (int i = 0; i < initialPoolSize; i++)
            CreateRobot();

        Debug.Log(
            $"Robot pool initialized with {availableRobots.Count} robots.");
    }

    private GameObject CreateRobot()
    {
        GameObject robot = Instantiate(robotPrefab, transform);
        robot.SetActive(false);

        availableRobots.Enqueue(robot);
        pooledRobots.Add(robot);

        return robot;
    }

    public GameObject GetRobot(Vector3 position, Quaternion rotation)
    {
        if (!initialized)
            InitializePool();

        if (!initialized)
        {
            Debug.LogError("RobotPoolManager: Pool is not initialized.");
            return null;
        }

        if (availableRobots.Count == 0)
            CreateRobot();

        GameObject robot = availableRobots.Dequeue();

        robot.transform.SetPositionAndRotation(position, rotation);
        robot.SetActive(true);

        return robot;
    }

    public void ReturnRobot(GameObject robot)
    {
        if (robot == null ||
            !pooledRobots.Contains(robot) ||
            !robot.activeSelf)
        {
            return;
        }

        robot.SetActive(false);
        robot.transform.SetParent(transform);
        availableRobots.Enqueue(robot);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
