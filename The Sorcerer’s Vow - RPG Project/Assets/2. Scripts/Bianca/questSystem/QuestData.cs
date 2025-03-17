using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quests/Quest")]
public class QuestData : ScriptableObject
{
    public string questName;
    public List<QuestData> questDependencies; // Lista de dependências
    public bool isCompleted;
}
