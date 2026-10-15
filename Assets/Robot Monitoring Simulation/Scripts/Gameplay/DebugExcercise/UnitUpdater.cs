using UnityEngine;
public class UnitUpdater : MonoBehaviour
{
    public Transform target;
    public GameObject marker;
    void Update()
    {
        GameObject unit = GameObject.Find("Robot");
        unit.transform.position = target.position;
        Instantiate(marker, unit.transform.position, Quaternion.identity);
    }
}