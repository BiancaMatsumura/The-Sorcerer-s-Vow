using System;
using _2._Scripts.Core.Domain.Character.Player;

namespace _2._Scripts.Core.Engine.Service.SaveLoad.Structure
{
    [Serializable]
    public class PlayerDataDTO
    {
        public string Name;
        public int Level;
        public int Speed;

        public PlayerDataDTO(PlayerCharacter playerCharacter)
        {
            Name = playerCharacter.Name;
            Level = playerCharacter.Level;
            Speed = playerCharacter.Speed;
        }
    }
}