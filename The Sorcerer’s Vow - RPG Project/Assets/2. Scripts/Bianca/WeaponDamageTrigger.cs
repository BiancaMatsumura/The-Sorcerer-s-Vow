using _2._Scripts.Core.Domain.Character.Player;
using UnityEngine;

public enum WeaponOwnerType
{
    Player,
    Enemy
}

public class WeaponDamageTrigger : MonoBehaviour
{
    [SerializeField] private WeaponOwnerType ownerType;

    private AgentWeapon weapon;
    private PlayerCharacter player;
    private Enemy enemy;

    private void Start()
    {
        weapon = GetComponentInParent<AgentWeapon>();

        if (ownerType == WeaponOwnerType.Player)
        {
            player = GetComponentInParent<PlayerCharacter>();
        }
        else if (ownerType == WeaponOwnerType.Enemy)
        {
            enemy = GetComponentInParent<Enemy>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (ownerType == WeaponOwnerType.Player)
        {
            if (other.TryGetComponent(out Enemy detectedEnemy))
            {
                if (!weapon.HasWeapon)
                {
                    float playerDano = player.AttackPower;
                    detectedEnemy.ReduceHealth(playerDano);
                }

                float dano = weapon.GetWeaponDamage();
                detectedEnemy.ReduceHealth(dano);
                weapon.UseWeapon();
            }
        }
        else if (ownerType == WeaponOwnerType.Enemy)
        {
            if (other.TryGetComponent(out PlayerCharacter playerTarget))
            {
                float dano = enemy.AttackPower;
                playerTarget.ReduceHealth(dano);
                //weapon.UseWeapon();
            }
        }
    }
}
