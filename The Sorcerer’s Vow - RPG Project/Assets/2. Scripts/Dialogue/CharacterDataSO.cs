using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDataSO", menuName = "Scriptable Objects/CharacterData")]
public class CharacterDataSO : ScriptableObject
{
    public string CharacterName;
    public Sprite Sprite;
}
