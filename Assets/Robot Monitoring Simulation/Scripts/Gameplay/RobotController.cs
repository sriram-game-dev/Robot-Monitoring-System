
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speedMultiplier = 1f;
    [SerializeField] private float turnSpeed = 180f;

    public RobotTelemetry CurrentTelemetry { get; private set; }

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool initialized;

    public void SetTelemetry(RobotTelemetry data)
    {
        if (data == null)
            return;

        CurrentTelemetry = data;
        targetPosition = data.position;
        targetRotation = Quaternion.Euler(0f, data.heading, 0f);

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

        float speed = Mathf.Max(
            0f, CurrentTelemetry.speed * speedMultiplier);

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime);
    }
}
