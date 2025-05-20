using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifetime); // Destroi automaticamente após X segundos
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Enemy enemy))
        {
            enemy.ReduceHealth(damage);
            Destroy(gameObject); // Destroi a bola de fogo após causar dano
        }
    }

    public void SetDamage(float dmg) => damage = dmg;
}
