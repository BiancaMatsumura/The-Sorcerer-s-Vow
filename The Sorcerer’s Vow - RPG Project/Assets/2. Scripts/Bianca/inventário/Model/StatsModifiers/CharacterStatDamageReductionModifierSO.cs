using _2._Scripts.Core.Domain.Character.Player;
using UnityEngine;

[CreateAssetMenu(menuName = "CharacterStatusModifiers/Damage Reduction")]
public class CharacterStatDamageReductionModifierSO : CharacterStatusModifierSO
{
    public float durationInSeconds = 5f; // Duração do buff
    [Range(0f, 100f)]
    public float damageReductionPercentage = 30f; // Reduz o dano recebido em %

    public override void AffectCharacter(GameObject character, float val)
    {
        PlayerCharacter player = character.GetComponent<PlayerCharacter>();
        if (player != null)
        {
            float totalReduction = damageReductionPercentage + val; // soma extra se quiser configurar pelo val
            player.ApplyDamageReductionTemporarily(totalReduction, durationInSeconds);
        }
        else
        {
            Debug.LogWarning("PlayerCharacter component not found on the GameObject.");
        }
    }
}
