using _2._Scripts.Core.Domain.Character.Player;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bianca.QuestSystem;
using QuestSystem;

namespace _2._Scripts.Core.Engine.Service.SaveLoad
{
    public class SaveLoadProgressScript : MonoBehaviour
    {
        public GameObject saveUi;
        
        private void Awake()
        {
            SaveFileDTO fileDto = SaveLoadProgressManager.GetSlotDataFromSaveFile(SaveLoadProgressManager.CurrentSave);
            PlayerCharacter playerCharacter = FindFirstObjectByType<PlayerCharacter>();
            Quest quest = null; // QuestSystem.Quest is not a UnityEngine.Object so it cannot be found via FindFirstObjectByType; replace with your own retrieval if needed
            if (fileDto != null && fileDto.PlayerData.Name != "")
            {
                fileDto.ApplyToPlayerCharacter(playerCharacter);
                if (quest != null)
                {
                    //quest.SetCurrentQuestIndex(fileDto.CurrentQuestIndex);
                }
            }
        }

        public void SaveCurrentPlayer()
        {
            PlayerCharacter playerCharacter = FindFirstObjectByType<PlayerCharacter>();
            Quest quest = null; // QuestSystem.Quest is not a UnityEngine.Object so it cannot be found via FindFirstObjectByType; replace with your own retrieval if needed
            //int questIndex = quest != null ? quest.GetCurrentQuestIndex() : 0;
            string sceneName = SceneManager.GetActiveScene().name;
            SaveLoadProgressManager.Save(SaveLoadProgressManager.CurrentSave, playerCharacter, /*questIndex*/ 0, sceneName);
            StartCoroutine(ShowSaveUiCoroutine());
        }

        private IEnumerator ShowSaveUiCoroutine()
        {
            saveUi.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            saveUi.SetActive(false);
        }
    }
}