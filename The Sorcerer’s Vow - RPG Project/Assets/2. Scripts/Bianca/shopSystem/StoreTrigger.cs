using Inventory.Model;
using ShopSystem;
using UnityEngine;

public class StoreTrigger : MonoBehaviour
{
    [SerializeField]
    private GameObject interactObject;

    [SerializeField]
    private KeyCode interactionKey = KeyCode.E;

    [SerializeField] private UIShopPage uiShopPage;
    [SerializeField] private Shop shop;

    [SerializeField] private InventoryController inventoryUI;

    [SerializeField] private UIManager uiManager;


    private Transform mainCamera;
    private bool isIn = false;
    private bool isStoreOpen = false;

    public bool IsStoreOpen => isStoreOpen;

    public void ForceCloseStore()
    {
        if (isStoreOpen)
            CloseStore();
    }


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

        if (Input.GetKeyDown(KeyCode.E) && isIn)
        {
            uiManager.ToggleShop(shop);
        }

    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Jogador entrou no trigger!");
            isIn = true;
            interactObject.SetActive(true);
        }
    }


    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isIn = false;
            interactObject.SetActive(false);
        }
    }


    private void ActivateStore()
    {

        uiShopPage.gameObject.SetActive(true);
        uiShopPage.InitializeShopUI(shop);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        CameraController.isInventoryOpen = true;

        isStoreOpen = true;
    }

    private void CloseStore()
    {
        uiShopPage.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
        CameraController.isInventoryOpen = false;

        isStoreOpen = false;
    }




}
