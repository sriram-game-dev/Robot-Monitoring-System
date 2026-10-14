
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class RobotPoolManagerTests
{
    private GameObject managerObject;
    private GameObject prefabObject;
    private RobotPoolManager pool;


    [UnitySetUp]
    public IEnumerator SetUp()
    {
        prefabObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        prefabObject.name = "TestRobotPrefab";

        // Create the manager inactive so Start cannot run prematurely.
        managerObject = new GameObject("TestRobotPool");
        managerObject.SetActive(false);

        pool = managerObject.AddComponent<RobotPoolManager>();

        managerObject.SetActive(true);

        // Initialize directly without ResourceManager or telemetry source.
        pool.InitializeForTesting(prefabObject, 1);

        yield return null;
    }


    [UnityTest]
    public IEnumerator ReturnedRobot_IsReused()
    {
        Vector3 firstPosition = new Vector3(1f, 0f, 2f);

        GameObject firstRobot = pool.GetRobot(
            firstPosition, Quaternion.identity);

        Assert.IsNotNull(firstRobot);
        Assert.IsTrue(firstRobot.activeSelf);
        Assert.AreEqual(firstPosition, firstRobot.transform.position);

        pool.ReturnRobot(firstRobot);

        Assert.IsFalse(firstRobot.activeSelf);

        Vector3 secondPosition = new Vector3(5f, 0f, 6f);

        GameObject secondRobot = pool.GetRobot(
            secondPosition, Quaternion.identity);

        Assert.IsNotNull(secondRobot);
        Assert.IsTrue(secondRobot.activeSelf);

        // The same GameObject must be reused.
        Assert.AreSame(firstRobot, secondRobot);

        Assert.AreEqual(
            secondPosition, secondRobot.transform.position);

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (managerObject != null)
            Object.Destroy(managerObject);

        if (prefabObject != null)
            Object.Destroy(prefabObject);

        yield return null;
    }
}
