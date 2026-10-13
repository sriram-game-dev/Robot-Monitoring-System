
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RobotTelemetry
{
    public string robotId;
    public Vector3 position;
    public float speed;
    public float heading;
    public bool isConnected;
    public float lastUpdateTime;
}

public class MockTelemetrySource : MonoBehaviour
{
    [Header("Generator Settings")]
    [SerializeField, Min(1)] private int robotCount = 10;
    [SerializeField, Min(0.1f)] private float updateInterval = 0.5f;

    [Header("Warehouse Movement Bounds")]
    [SerializeField] private Vector2 xBounds = new Vector2(-8f, 8f);
    [SerializeField] private Vector2 zBounds = new Vector2(-8f, 8f);

    [Header("Simulated Telemetry")]
    [SerializeField] private Vector2 speedRange = new Vector2(0.5f, 2f);
    [SerializeField, Range(0f, 1f)]
    private float dataLossChance = 0.02f;

    public event Action<RobotTelemetry> TelemetryUpdated;

    private readonly List<RobotTelemetry> robots =
        new List<RobotTelemetry>();

    private float timer;

    public IReadOnlyList<RobotTelemetry> Robots => robots;

    //Reference
    public static MockTelemetrySource instance;

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

    private void Start()
    {
        GenerateInitialTelemetry();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer < updateInterval)
            return;

        timer = 0f;
        UpdateTelemetry();
    }

    private void GenerateInitialTelemetry()
    {
        robots.Clear();

        for (int i = 0; i < robotCount; i++)
        {
            RobotTelemetry data = new RobotTelemetry
            {
                robotId = $"BOT_{i + 1:000}",
                position = new Vector3(
                    UnityEngine.Random.Range(xBounds.x, xBounds.y),
                    0f,
                    UnityEngine.Random.Range(zBounds.x, zBounds.y)
                ),
                speed = UnityEngine.Random.Range(
                    speedRange.x, speedRange.y),
                heading = UnityEngine.Random.Range(0f, 360f),
                isConnected = true,
                lastUpdateTime = Time.time
            };

            robots.Add(data);
            TelemetryUpdated?.Invoke(data);
        }
    }

    private void UpdateTelemetry()
    {
        foreach (RobotTelemetry data in robots)
        {
            // Simulate intermittent connection loss.
            if (data.isConnected &&
                UnityEngine.Random.value < dataLossChance)
            {
                data.isConnected = false;
                data.speed = 0f;
            }
            else if (!data.isConnected &&
                     UnityEngine.Random.value < 0.35f)
            {
                data.isConnected = true;
                data.heading = UnityEngine.Random.Range(0f, 360f);
                data.speed = UnityEngine.Random.Range(
                    speedRange.x, speedRange.y);
            }

            if (data.isConnected)
            {
                data.heading += UnityEngine.Random.Range(-20f, 20f);
                data.heading = (data.heading + 360f) % 360f;

                Vector3 direction = Quaternion.Euler(
                    0f, data.heading, 0f) * Vector3.forward;

                Vector3 nextPosition = data.position +
                    direction * data.speed * updateInterval;
Debug.Log(
    $"{data.robotId} | Current: {data.position} | " +
    $"Next: {nextPosition} | Heading: {data.heading:F1}");
                // Keep robots inside the movement bounds.
                if (nextPosition.x < xBounds.x ||
                    nextPosition.x > xBounds.y ||
                    nextPosition.z < zBounds.x ||
                    nextPosition.z > zBounds.y)
                {
                    data.heading = (data.heading + 180f) % 360f;
                }
                else
                {
                    data.position = nextPosition;
                }

                data.lastUpdateTime = Time.time;
            }

            TelemetryUpdated?.Invoke(data);
            Debug.Log(
    $"{data.robotId} | Position: {data.position} | " +
    $"Speed: {data.speed:F2} | " +
    $"Heading: {data.heading:F1} | " +
    $"Connected: {data.isConnected}"

);
        }
    }

    public RobotTelemetry GetTelemetry(string robotId)
    {
        return robots.Find(r => r.robotId == robotId);
    }
}
