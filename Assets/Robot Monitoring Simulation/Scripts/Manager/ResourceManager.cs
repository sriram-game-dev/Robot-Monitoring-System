using UnityEngine;
using UnityEngine.EventSystems;
public class ResourceManager : MonoBehaviour
{

    //Robot
    [Header("Player")]
    public GameObject botPrefab;

    //Camera
    [Header("Camera")]
    public Camera mainCamera;

    //Reference
    public static ResourceManager instance;



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



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
