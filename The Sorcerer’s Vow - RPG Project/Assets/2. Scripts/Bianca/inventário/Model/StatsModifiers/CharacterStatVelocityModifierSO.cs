using _2._Scripts.Core.Domain.Character.Player;
using UnityEngine;

[CreateAssetMenu(menuName = "CharacterStatusModifiers/Velocity Modifier")]
public class CharacterStatVelocityModifierSO : CharacterStatusModifierSO
{
    public float durationInSeconds = 5f; // configure no Inspector

    public override void AffectCharacter(GameObject character, float val)
    {
        PlayerCharacter player = character.GetComponent<PlayerCharacter>();
        if (player != null)
        {
            int atualSpeed = player.Speed;
            int newSpeed =  atualSpeed + (int)val;
            player.ChangeVelocityTemporarily(newSpeed, durationInSeconds);
        }
        else
        {
            Debug.LogWarning("PlayerCharacter component not found on the GameObject.");
        }
    }
}
