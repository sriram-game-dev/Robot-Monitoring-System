
using UnityEngine;
using TMPro;

public class RestrictedAreaLabel : MonoBehaviour
{


    [SerializeField] private TMP_Text labelText;
    [SerializeField] private string areaName = "RESTRICTED AREA A";

    private Camera mainCamera;


    void Reference()
    {
        mainCamera = ResourceManager.instance.mainCamera;
    }

    private void Awake()
    {
        if (labelText != null)
            labelText.text = areaName;

        // Keep the label enabled.
        gameObject.SetActive(true);
    }

    private void Start()
    {
        Reference();
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
            return;

        transform.forward = mainCamera.transform.forward;
    }
}
