using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DialogBar : MonoBehaviour
{
    private Image barImage;
    
    public float speed = 10f;
    bool open = false;

    private void Awake()
    {
        barImage = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (barImage == null)
        {
            return;
        }

        if (open)
        {
            barImage.fillAmount = Mathf.Lerp(barImage.fillAmount, 1, speed * Time.deltaTime);
        }
        else
        {
            barImage.fillAmount = Mathf.Lerp(barImage.fillAmount, 0, speed * Time.deltaTime);
        }
    }

    public void Enable() 
    {
        barImage.fillAmount = 0;
        open = true;
    }

    public void Disable()
    {
        open = false;
    }

}
