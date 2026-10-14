
using UnityEngine;

public static class TelemetryValidator
{
    public static bool IsValid(RobotTelemetry data)
    {
        if (data == null ||
            string.IsNullOrWhiteSpace(data.robotId))
            return false;

        if (!IsFinite(data.position.x) ||
            !IsFinite(data.position.y) ||
            !IsFinite(data.position.z))
            return false;

        if (!IsFinite(data.speed) || data.speed < 0f)
            return false;

        if (!IsFinite(data.heading))
            return false;

        if (!IsFinite(data.lastUpdateTime))
            return false;

        return true;
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) &&
               !float.IsInfinity(value);
    }
}