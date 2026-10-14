using NUnit.Framework;
using UnityEngine;

public class TelemetryValidatorTests
{
    private RobotTelemetry CreateValidTelemetry()
    {
        return new RobotTelemetry
        {
            robotId = "BOT_001",
            position = new Vector3(1f, 0f, 2f),
            speed = 1.5f,
            heading = 90f,
            isConnected = true,
            lastUpdateTime = 1f
        };
    }

    [Test]
    public void ValidTelemetry_IsAccepted()
    {
        Assert.IsTrue(
            TelemetryValidator.IsValid(
                CreateValidTelemetry()));
    }

    [Test]
    public void MissingRobotId_IsRejected()
    {
        RobotTelemetry data = CreateValidTelemetry();
        data.robotId = "";

        Assert.IsFalse(TelemetryValidator.IsValid(data));
    }

    [Test]
    public void NegativeSpeed_IsRejected()
    {
        RobotTelemetry data = CreateValidTelemetry();
        data.speed = -1f;

        Assert.IsFalse(TelemetryValidator.IsValid(data));
    }

    [Test]
    public void NaNPosition_IsRejected()
    {
        RobotTelemetry data = CreateValidTelemetry();
        data.position.x = float.NaN;

        Assert.IsFalse(TelemetryValidator.IsValid(data));
    }

    [Test]
    public void InfiniteHeading_IsRejected()
    {
        RobotTelemetry data = CreateValidTelemetry();
        data.heading = float.PositiveInfinity;

        Assert.IsFalse(TelemetryValidator.IsValid(data));
    }
}
