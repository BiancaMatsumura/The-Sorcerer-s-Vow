using System.Collections.Generic;
using _2._Scripts.Core.Domain.Character.Player;
using UnityEngine;

public enum WeaponOwnerType
{
    Player,
    Enemy,
    Toten,
    Boss
}

public class WeaponDamageTrigger : MonoBehaviour
{
    [SerializeField] private WeaponOwnerType ownerType;

    private AgentWeapon weapon;
    private PlayerCharacter player;
    private Enemy enemy;
    private TotenSpawnEnemy toten;
    private BossCharacter boss;

    [SerializeField] private float damage = 20f;

    // Lista de alvos já atingidos neste ataque
    private HashSet<Collider> hitTargets = new HashSet<Collider>();

    private void Start()
    {
        weapon = GetComponentInParent<AgentWeapon>();

        if (ownerType == WeaponOwnerType.Player)
            player = GetComponentInParent<PlayerCharacter>();
        else if (ownerType == WeaponOwnerType.Enemy)
            enemy = GetComponentInParent<Enemy>();
        else if (ownerType == WeaponOwnerType.Toten)
            toten = GetComponentInParent<TotenSpawnEnemy>();
        else if (ownerType == WeaponOwnerType.Boss)
            boss = GetComponentInParent<BossCharacter>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // evita múltiplos hits no mesmo alvo
        if (hitTargets.Contains(other)) return;
        hitTargets.Add(other);

        if (ownerType == WeaponOwnerType.Player)
        {
            if (other.TryGetComponent(out Enemy detectedEnemy))
            {
                float dano = !weapon.HasWeapon ? player.AttackPower : weapon.GetWeaponDamage();
                detectedEnemy.ReduceHealth(dano);
                if (weapon.HasWeapon) weapon.UseWeapon();
            }
            else if (other.TryGetComponent(out BossCharacter bossTarget))
            {
                float dano = !weapon.HasWeapon ? player.AttackPower : weapon.GetWeaponDamage();
                bossTarget.ReduceHealth(dano);
                if (weapon.HasWeapon) weapon.UseWeapon();
            }
            else
            {
                TotenSpawnEnemy totenTarget = other.GetComponentInParent<TotenSpawnEnemy>();
                if (totenTarget != null)
                {
                    totenTarget.TakeDamage(damage);
                }
            }
        }
        else if (ownerType == WeaponOwnerType.Enemy)
        {
            if (other.TryGetComponent(out PlayerCharacter playerTarget))
            {
                playerTarget.ReduceHealth(enemy.AttackPower);
            }
        }
        else if (ownerType == WeaponOwnerType.Boss)
        {
            if (other.TryGetComponent(out PlayerCharacter playerTarget))
            {
                playerTarget.ReduceHealth(boss.AttackPower);
            }
        }
    }

    // chamado pelo AgentWeapon no início do ataque
    public void ResetHits()
    {
        hitTargets.Clear();
    }
}
