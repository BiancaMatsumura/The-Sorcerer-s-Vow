using _2._Scripts.Core.Domain.Character.Player;
using UnityEngine;

namespace _2._Scripts.Core.Engine.Service.SaveLoad
{
    public class SaveLoadProgressScript : MonoBehaviour
    {
        private void Awake()
        {
            SaveFileDTO fileDto = SaveLoadProgressManager.GetSlotDataFromSaveFile(SaveLoadProgressManager.CurrentSave);
            PlayerCharacter playerCharacter = FindFirstObjectByType<PlayerCharacter>();
            if (fileDto != null && fileDto.playerData.Name != "")
            {
                fileDto.SetPlayerCharacterSaveData(playerCharacter);
            }
            SaveLoadProgressManager.Save(SaveLoadProgressManager.CurrentSave, playerCharacter);
        }
    }
}