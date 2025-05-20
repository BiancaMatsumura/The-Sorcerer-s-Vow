using _2._Scripts.Core.Domain.Character.Player;
using UnityEngine;


[CreateAssetMenu(menuName = "CharacterStatusModifiers/Damage Extra")]
public class CharacterStatDamageExtraModifierSO : CharacterStatusModifierSO
{
    public float durationInSeconds;
    public override void AffectCharacter(GameObject character, float val)
    {
        PlayerCharacter player = character.GetComponent<PlayerCharacter>();
        if (player != null)
        {
            player.ChangeAttackPowerTemporarily(val, durationInSeconds);
        }
        else
        {
            Debug.LogWarning("PlayerCharacter component not found on the GameObject.");
        }
    }
}
