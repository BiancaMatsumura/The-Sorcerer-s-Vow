using _2._Scripts.Core.Domain.Character.Player;
using UnityEngine;

public class WeaponDamageTrigger : MonoBehaviour
{
    private AgentWeapon weapon;
    private PlayerCharacter player;

    private void Start()
    {
        weapon = GetComponentInParent<AgentWeapon>();
        player = GetComponentInParent<PlayerCharacter>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (weapon == null)
        {
            return;
        }

        if (other.TryGetComponent(out EnimyTeste detectedEnemy))
        {
            if (!weapon.HasWeapon)
            {
                float playerDano = player.AttackPower;
                detectedEnemy.TakeDamage(playerDano);
            }
            float dano = weapon.GetWeaponDamage();
            detectedEnemy.TakeDamage(dano);
            weapon.UseWeapon();
        }
    }
}
