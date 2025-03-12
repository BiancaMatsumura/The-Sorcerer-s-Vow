using UnityEngine;

public class QuestSystem : MonoBehaviour
{
    public string [] nameQuests;
    public string [] questDependence;
    public bool [] checkQuest; 


    public bool CheckQuests()
    {
        for (int i = 0; i < checkQuest.Length; i++)
        {
            if(checkQuest[i] == false)
            {
                return false;
            }
        }
        return true;
    }

    public string [] EmptyQuest()
    {
        int countEmpty = 0;

        for (int i = 0; i < checkQuest.Length; i++)
        {
            if(checkQuest[i] == false)
            {
                countEmpty++;
            }
        }

        string[] Empty = new string [countEmpty];

        int j=0;
        for (int i = 0; i < checkQuest.Length; i++)
        {
            if(checkQuest[i] == false)
            {
                Empty[j] = nameQuests[i];
                j++;
            }
        }
        return Empty;
    }

    public bool CheckQuest(string nameQuest)
    {
        for (int i = 0; i < nameQuests.Length; i++)
        {
            if(nameQuests[i] == nameQuest)
            {
                checkQuest[i] = true;
                return true;
            }
        }
        return false;
    }

}
