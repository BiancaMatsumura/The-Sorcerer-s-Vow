using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueSO", menuName = "Scriptable Objects/Dialogue")]
public class DialogueDataSO : ScriptableObject
{
    public List<DialogSentence> Sentences;

}

[Serializable]
public class DialogSentence
{
    public CharacterDataSO ActorData;
    [TextArea(3, 5)]
    public string Content;
}
