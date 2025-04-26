using UnityEngine;
using UnityEngine.UI;
public class EnimyTeste : MonoBehaviour
{
    private float maxhealh;
    public float life;
    public Scrollbar bar;
    public Animator anime;
    public bool isplayer;
    private void Start()
    {
        maxhealh = life;
        bar.size = 1f;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("playerbattle"))
        {
            TakeDamage(10);
        }
    }

    public void TakeDamage(float tanto) 
    {   
        if (life > 0)
        {
            life -= tanto;
            bar.size = life/maxhealh;
        }
        else
        {
            Die();
        }

    }

    void Die() 
    {
        anime.SetInteger("int", 1);
    }
}
