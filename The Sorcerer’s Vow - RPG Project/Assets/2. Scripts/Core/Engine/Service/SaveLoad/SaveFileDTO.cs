using System;
using _2._Scripts.Core.Domain.Character.Player;
using _2._Scripts.Core.Engine.Service.SaveLoad.Structure;
using Bianca.QuestSystem;

namespace _2._Scripts.Core.Engine.Service.SaveLoad
{
    [Serializable]
    public class SaveFileDTO
    {
        public int SlotIndex = 0;
        public PlayerDataDTO PlayerData;
        public InventoryDTO InventoryData;
        public int CurrentQuestIndex = 0;
        public string SceneName = "";
        
        public SaveFileDTO(int slot, PlayerCharacter playerCharacter, int currentQuestIndex = 0, string sceneName = "")
        {
            SlotIndex = slot;
            CurrentQuestIndex = currentQuestIndex;
            SceneName = sceneName;
            if (playerCharacter != null)
            {
                PlayerData = new PlayerDataDTO(playerCharacter);
                InventoryData = new InventoryDTO(playerCharacter);
            }
        }

        public void ApplyToPlayerCharacter(PlayerCharacter playerCharacter, Quest quest = null)
        {
            if (PlayerData == null || playerCharacter == null) return;
            playerCharacter.Name = PlayerData.Name;
            playerCharacter.Level = PlayerData.Level;
            playerCharacter.Speed = PlayerData.Speed;
            playerCharacter.SetInitialPosition(PlayerData.Position);
            if (quest != null)
            {
                quest.SetCurrentQuestIndex(CurrentQuestIndex);
            }
        }
    }
}