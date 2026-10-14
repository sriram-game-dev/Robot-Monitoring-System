
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Movement")]
    // [SerializeField] private float speedMultiplier = 1f;
    [SerializeField] private float turnSpeed = 180f;

    public RobotTelemetry CurrentTelemetry { get; private set; }

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 previousPosition;
    private float interpolationDuration = 0.5f;
    private float interpolationTimer;
    private bool initialized;

    public void SetTelemetry(RobotTelemetry data)
    {
        if (data == null)
            return;

        // Capture the previous target before accepting the new one.
        if (initialized)
            previousPosition = transform.position;
        else
            previousPosition = data.position;

        CurrentTelemetry = data;
        targetPosition = data.position;
        targetRotation = Quaternion.Euler(0f, data.heading, 0f);
        interpolationTimer = 0f;

        if (!initialized)
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
            initialized = true;
        }
    }

    private void Update()
    {
        if (!initialized || CurrentTelemetry == null)
            return;

        if (!CurrentTelemetry.isConnected)
            return;

        interpolationTimer += Time.deltaTime;

        float t = Mathf.Clamp01(
            interpolationTimer / interpolationDuration);

        transform.position = Vector3.Lerp(
            previousPosition,
            targetPosition,
            t);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime);
    }
}
