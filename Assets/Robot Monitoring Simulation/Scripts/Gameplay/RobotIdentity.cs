using UnityEngine;

public class RobotIdentity : MonoBehaviour
{
    public string UnitId { get; private set; }

    public void Initialize(string unitId)
    {
        UnitId = unitId;
    }
}