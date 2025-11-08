using UnityEngine;

public class DesablePopup : MonoBehaviour
{
    public AudioSource SOM;
    private void SFX() 
    {
        SOM.Play();
    }
    private void HidePickUpUI()
    {
        if (this != null)
        {
                this.gameObject.SetActive(false);  
                return;
        }
    }

}

