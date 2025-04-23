using _2._Scripts.Core.Domain.Character.Player;
using UnityEngine;

public class DamageTest : MonoBehaviour
{
    [SerializeField] private PlayerCharacter health; 
    public int damage = 5;
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            health.ReduceHealth(damage);
        }
    }
}
