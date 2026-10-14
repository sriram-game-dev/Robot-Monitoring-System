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
    [Min(1)]
    public int robotCount = 10;

    [SerializeField, Min(0.1f)]
    private float updateInterval = 0.5f;

    [Header("Warehouse Movement Bounds")]
    [SerializeField]
    private Vector2 xBounds = new Vector2(-8f, 8f);

    [SerializeField]
    private Vector2 zBounds = new Vector2(-8f, 8f);

    [Header("Simulated Telemetry")]
    [SerializeField]
    private Vector2 speedRange = new Vector2(0.5f, 2f);

    [SerializeField, Range(0f, 1f)]
    private float dataLossChance = 0.02f;

    [Header("Robot Spacing")]
    [SerializeField, Min(0.1f)]
    private float minSpawnDistance = 1.5f;

    [Header("Robot Collision Avoidance")]
    [SerializeField, Min(0.1f)]
    private float minRobotDistance = 1.5f;

    [Header("Data Loss and Validation")]
    [SerializeField, Min(0.5f)]
    private float dataLostTimeout = 3f;

    public event Action<RobotTelemetry> TelemetryUpdated;

    private readonly List<RobotTelemetry> robots =
        new List<RobotTelemetry>();

    private float timer;

    public IReadOnlyList<RobotTelemetry> Robots => robots;

    public static MockTelemetrySource instance;

    private static readonly float[] TurnAngles =
    {
        0f, 45f, -45f, 90f, -90f,
        135f, -135f, 180f
    };

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
                position = GetSpawnPosition(),
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
            bool packetReceived =
                UnityEngine.Random.value >= dataLossChance;

            if (packetReceived)
            {
                if (!IsValidTelemetry(data))
                {
                    data.isConnected = false;
                    data.speed = 0f;

                    Debug.LogWarning(
                        $"{data.robotId}: Invalid telemetry rejected.");

                    TelemetryUpdated?.Invoke(data);
                    continue;
                }

                bool wasDisconnected = !data.isConnected;

                if (TryFindSafeMove(
                    data,
                    out Vector3 nextPosition,
                    out float safeHeading))
                {
                    data.position = nextPosition;
                    data.heading = safeHeading;
                }

                // If no safe direction is available, retain position.
                data.isConnected = true;
                data.lastUpdateTime = Time.time;

                if (wasDisconnected)
                {
                    Debug.Log(
                        $"{data.robotId}: Telemetry recovered.");
                }
            }

            // Detect stale telemetry.
            if (Time.time - data.lastUpdateTime > dataLostTimeout)
            {
                if (data.isConnected)
                {
                    Debug.LogWarning(
                        $"{data.robotId}: Data lost. Telemetry timed out.");
                }

                data.isConnected = false;
                data.speed = 0f;
            }

            TelemetryUpdated?.Invoke(data);
        }
    }

    private bool TryFindSafeMove(
        RobotTelemetry robot,
        out Vector3 safePosition,
        out float safeHeading)
    {
        safePosition = robot.position;
        safeHeading = robot.heading;

        foreach (float turn in TurnAngles)
        {
            float candidateHeading =
                (robot.heading + turn + 360f) % 360f;

            Vector3 direction = Quaternion.Euler(
                0f, candidateHeading, 0f) * Vector3.forward;

            Vector3 candidatePosition = robot.position +
                direction * robot.speed * updateInterval;

            // Reject positions outside warehouse bounds.
            if (candidatePosition.x < xBounds.x ||
                candidatePosition.x > xBounds.y ||
                candidatePosition.z < zBounds.x ||
                candidatePosition.z > zBounds.y)
            {
                continue;
            }

            bool blocked = false;

            foreach (RobotTelemetry other in robots)
            {
                if (other == robot)
                    continue;

                // Compare floor-level positions.
                Vector2 candidateXZ = new Vector2(
                    candidatePosition.x,
                    candidatePosition.z);

                Vector2 otherXZ = new Vector2(
                    other.position.x,
                    other.position.z);

                if (Vector2.Distance(candidateXZ, otherXZ)
                    < minRobotDistance)
                {
                    blocked = true;
                    break;
                }
            }

            if (blocked)
                continue;

            safePosition = candidatePosition;
            safeHeading = candidateHeading;
            return true;
        }

        // No safe candidate was found.
        return false;
    }

    private bool IsValidTelemetry(RobotTelemetry data)
    {
        return TelemetryValidator.IsValid(data);
    }
    private bool IsFinite(float value)
    {
        return !float.IsNaN(value) &&
               !float.IsInfinity(value);
    }

    private Vector3 GetSpawnPosition()
    {
        for (int attempt = 0; attempt < 1000; attempt++)
        {
            Vector3 candidate = new Vector3(
                UnityEngine.Random.Range(xBounds.x, xBounds.y),
                0f,
                UnityEngine.Random.Range(zBounds.x, zBounds.y));

            bool tooClose = false;

            foreach (RobotTelemetry robot in robots)
            {
                Vector2 candidateXZ =
                    new Vector2(candidate.x, candidate.z);

                Vector2 existingXZ =
                    new Vector2(robot.position.x, robot.position.z);

                if (Vector2.Distance(candidateXZ, existingXZ)
                    < minSpawnDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
                return candidate;
        }

        Debug.LogWarning(
            "Could not find a sufficiently spaced spawn position. " +
            "Consider reducing robot count or minSpawnDistance.");

        return new Vector3(xBounds.x, 0f, zBounds.x);
    }

    public RobotTelemetry GetTelemetry(string robotId)
    {
        return robots.Find(r => r.robotId == robotId);
    }
}