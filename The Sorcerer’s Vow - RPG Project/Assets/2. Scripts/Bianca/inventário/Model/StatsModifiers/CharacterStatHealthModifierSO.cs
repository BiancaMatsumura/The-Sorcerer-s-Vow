using _2._Scripts.Core.Domain.Character.Player;
using UnityEngine;

[CreateAssetMenu(menuName = "CharacterStatusModifiers/Health Modifier")]
public class CharacterStatHealthModifierSO : CharacterStatusModifierSO
{
    public override void AffectCharacter(GameObject character, float val)
    {
        PlayerCharacter player = character.GetComponent<PlayerCharacter>();
        if (player != null)
        {
            player.AddHealth((int)val);
        }
        else
        {
            Debug.LogWarning("PlayerCharacter component not found on the GameObject.");
        }
    }
}
