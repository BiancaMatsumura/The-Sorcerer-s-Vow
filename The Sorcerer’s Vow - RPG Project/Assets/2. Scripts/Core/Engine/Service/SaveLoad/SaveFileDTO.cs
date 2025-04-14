using System;
using _2._Scripts.Core.Domain.Character.Player;
using _2._Scripts.Core.Engine.Service.SaveLoad.Structure;

namespace _2._Scripts.Core.Engine.Service.SaveLoad
{
    [Serializable]
    public class SaveFileDTO
    {
        public int slot = 0;
        public PlayerDataDTO playerData;

        public SaveFileDTO(int slot, PlayerCharacter playerCharacter)
        {
            this.slot = slot;
            if (playerCharacter)
            {
                playerData = new PlayerDataDTO(playerCharacter);
            }
        }
    }
}