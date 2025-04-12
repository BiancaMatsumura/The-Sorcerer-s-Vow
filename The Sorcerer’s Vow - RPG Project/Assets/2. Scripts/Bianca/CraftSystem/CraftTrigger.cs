using UnityEngine;

public class CraftTrigger : MonoBehaviour
{
    [SerializeField]
    private GameObject interactObject;
    [SerializeField]
    private GameObject CraftingUI;

    [SerializeField] 
    private KeyCode interactionKey = KeyCode.E;

    private Transform mainCamera;
    private bool isIn = false;
    private bool isActive = false;

    void Start()
    {
        mainCamera = Camera.main.transform;
    }

    void Update()
    {
        if (interactObject.activeSelf)
        {
            interactObject.transform.LookAt(mainCamera);
            
            interactObject.transform.Rotate(0f, 180f, 0f);
        }

        if (Input.GetKeyDown(interactionKey) && isIn)
        {
            if(!isActive)
            {
                ShowCraftingUI();
            }
            else
            {
                HideCraftingUI();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            isIn = true;
            interactObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            isIn = false;
            interactObject.SetActive(false);
        }
    }

    private void ShowCraftingUI()
    {
        CraftingUI.SetActive(true);
        isActive = true;
        CameraController.isCraftingUIOpen = true;
        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    private void HideCraftingUI()
    {
        CraftingUI.SetActive(false);
        isActive = false;
        CameraController.isCraftingUIOpen = false;
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;
        Time.timeScale = 1f;
    }
}
