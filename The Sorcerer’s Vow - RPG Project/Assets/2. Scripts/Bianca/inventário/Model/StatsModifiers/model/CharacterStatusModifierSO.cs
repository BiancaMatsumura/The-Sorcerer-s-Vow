using UnityEngine;

public abstract class CharacterStatusModifierSO : ScriptableObject
{
    public abstract void AffectCharacter (GameObject character, float val);
}
