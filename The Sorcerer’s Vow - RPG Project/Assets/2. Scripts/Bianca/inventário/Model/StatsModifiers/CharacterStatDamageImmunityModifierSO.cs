using _2._Scripts.Core.Domain.Character.Player;
using UnityEngine;

[CreateAssetMenu(menuName = "CharacterStatusModifiers/Damage Immunity")]
public class CharacterStatDamageImmunityModifierSO : CharacterStatusModifierSO
{

    public override void AffectCharacter(GameObject character, float val)
    {
        PlayerCharacter player = character.GetComponent<PlayerCharacter>();
        if (player != null)
        {
            player.ApplyDamageImmunityTemporarily(val);
        }
        else
        {
            Debug.LogWarning("PlayerCharacter component not found on the GameObject.");
        }
    }
}
