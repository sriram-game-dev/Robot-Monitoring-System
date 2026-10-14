using System.Collections.Generic;
using UnityEngine;

public class ProximityManager : MonoBehaviour
{
    [Header("Proximity Settings")]
    [SerializeField] private float warningDistance = 2f;
    [SerializeField] private float checkInterval = 0.25f;

    private readonly HashSet<string> activePairs =
        new HashSet<string>();

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer < checkInterval)
            return;

        timer = 0f;
        CheckProximity();
    }

    private void CheckProximity()
    {
        RobotController[] robots =
            FindObjectsByType<RobotController>(
                FindObjectsSortMode.None);

        HashSet<string> currentPairs =
            new HashSet<string>();

        for (int i = 0; i < robots.Length; i++)
        {
            if (robots[i] == null ||
                robots[i].CurrentTelemetry == null ||
                !robots[i].CurrentTelemetry.isConnected)
                continue;

            for (int j = i + 1; j < robots.Length; j++)
            {
                if (robots[j] == null ||
                    robots[j].CurrentTelemetry == null ||
                    !robots[j].CurrentTelemetry.isConnected)
                    continue;

                float distance = Vector3.Distance(
                    robots[i].transform.position,
                    robots[j].transform.position);

                if (distance <= warningDistance)
                {
                    string idA =
                        robots[i].CurrentTelemetry.robotId;
                    string idB =
                        robots[j].CurrentTelemetry.robotId;

                    // Keep the pair key consistent.
                    string pairKey = string.CompareOrdinal(
                        idA, idB) < 0
                        ? idA + "_" + idB
                        : idB + "_" + idA;

                    currentPairs.Add(pairKey);

                    if (!activePairs.Contains(pairKey))
                    {
                        string message =
                            "PROXIMITY ALERT: " + idA +
                            " and " + idB +
                            " are " + distance.ToString("F1") +
                            " m apart!";

                        if (UIManager.instance != null)
                        {
                            UIManager.instance.ShowWarning(
                                "PROXIMITY_" + pairKey,
                                message);
                        }

                        Debug.LogWarning(message);
                    }
                }
            }
        }

        // Clear warnings for pairs that are no longer close.
        foreach (string oldPair in activePairs)
        {
            if (!currentPairs.Contains(oldPair) &&
                UIManager.instance != null)
            {
                UIManager.instance.ClearWarning(
                    "PROXIMITY_" + oldPair);
            }
        }

        activePairs.Clear();

        foreach (string pair in currentPairs)
            activePairs.Add(pair);
    }
}
