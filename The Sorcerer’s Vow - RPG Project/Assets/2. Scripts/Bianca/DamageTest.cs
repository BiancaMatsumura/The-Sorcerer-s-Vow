using UnityEngine;

public class DamageTest : MonoBehaviour
{
    public Health health;
    public int damage = 5;
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            health.Reduce(damage);
        }
    }
}
