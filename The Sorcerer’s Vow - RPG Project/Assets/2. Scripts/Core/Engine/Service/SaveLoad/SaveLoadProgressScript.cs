using _2._Scripts.Core.Domain.Character.Player;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _2._Scripts.Core.Engine.Service.SaveLoad
{
    public class SaveLoadProgressScript : MonoBehaviour
    {
        public GameObject saveUi;
        
        private void Awake()
        {
            SaveFileDTO fileDto = SaveLoadProgressManager.GetSlotDataFromSaveFile(SaveLoadProgressManager.CurrentSave);
            PlayerCharacter playerCharacter = FindFirstObjectByType<PlayerCharacter>();
            var quest = FindFirstObjectByType<Quest>();
            if (fileDto != null && fileDto.PlayerData.Name != "")
            {
                fileDto.ApplyToPlayerCharacter(playerCharacter);
                if (quest != null)
                {
                    quest.SetCurrentQuestIndex(fileDto.CurrentQuestIndex);
                }
            }
        }

        public void SaveCurrentPlayer()
        {
            PlayerCharacter playerCharacter = FindFirstObjectByType<PlayerCharacter>();
            var quest = FindFirstObjectByType<Quest>();
            int questIndex = quest != null ? quest.GetCurrentQuestIndex() : 0;
            string sceneName = SceneManager.GetActiveScene().name;
            SaveLoadProgressManager.Save(SaveLoadProgressManager.CurrentSave, playerCharacter, questIndex, sceneName);
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