using Inventory.Model;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UI;
using QuestSystem;
public class UIPopup : MonoBehaviour
{   
    private PickUpSystem _pickupSystem;
    private List<ItemSO> inventory;
    private Item3D item;
    public AudioSource itenSong;
    private Sprite ItemImage;
    private string ItemName;
    private string Mensage;
    public GameObject BASE ;
    void Start()
    {
        item.GetComponent<Item3D>();
        BASE.SetActive(false);

    }

    //ao destruir ser� adiconado um item deste gameobject a lista, assim impedindo que o popup seja mostrado varias vezes pelo mesmo item 
    private void OnDestroy()
    {
        //if (!IsCollected(inventory, item.InventoryItem)) // Se o jogador ja tiver colletado descontinua o codigo
        //{
            
            itenSong.Play();
            ItemSO VITEM = item.InventoryItem;
            inventory.Add(VITEM);
            ShowPopup();
        //}
        
    }

    //Indentifica se o jogador ja coletou um item
    bool IsCollected(List<ItemSO> Lista, ItemSO item2) 
    {
        foreach (ItemSO item3 in Lista)
        {
            if (item3 == item2) return true;

            else { return false; }
        }
        Debug.LogError("N�o foi detectado nenhum item");
        return false;
    }

    //Mostra o popup e troca as informa��es pro novo item
    public void ShowPopup() 
    {   
        TextMesh TXT = BASE.GetComponent<TextMesh>();
        Image IMG = BASE.GetComponentInChildren<Image>();
        ItemName = item.InventoryItem.Name;
        ItemImage = item.InventoryItem.ItemImage;
        IMG.sprite = ItemImage;
        TXT.text = ItemName;   
        BASE.SetActive(true);


    }
}
