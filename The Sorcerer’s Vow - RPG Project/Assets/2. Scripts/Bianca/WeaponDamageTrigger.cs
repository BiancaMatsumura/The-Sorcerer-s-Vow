using _2._Scripts.Core.Domain.Character.Player;
using UnityEngine;

public enum WeaponOwnerType
{
    Player,
    Enemy,
    Toten
}

public class WeaponDamageTrigger : MonoBehaviour
{
    [SerializeField] private WeaponOwnerType ownerType;

    private AgentWeapon weapon;
    private PlayerCharacter player;
    private Enemy enemy;
    private TotenSpawnEnemy toten;
    [SerializeField] private float damage = 20f;

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
        else if (ownerType == WeaponOwnerType.Toten)
        {
            toten = GetComponentInParent<TotenSpawnEnemy>();
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

            TotenSpawnEnemy toten = other.GetComponentInParent<TotenSpawnEnemy>();
            if (toten != null)
            {
                toten.TakeDamage(damage);
                return;
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
