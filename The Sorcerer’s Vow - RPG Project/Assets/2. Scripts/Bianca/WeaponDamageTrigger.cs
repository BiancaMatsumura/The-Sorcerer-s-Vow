using UnityEngine;

public class WeaponDamageTrigger : MonoBehaviour
{
    private AgentWeapon weapon;

    private void Start()
    {
        weapon = GetComponentInParent<AgentWeapon>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (weapon == null) return;

        if (other.TryGetComponent(out EnimyTeste enemy))
        {
            float dano = weapon.GetWeaponDamage();
            enemy.TakeDamage(dano);
            weapon.UseWeapon();
        }
    }
}
