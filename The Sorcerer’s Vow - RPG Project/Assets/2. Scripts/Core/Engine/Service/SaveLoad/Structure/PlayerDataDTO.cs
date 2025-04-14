using System;
using _2._Scripts.Core.Domain.Character.Player;
using UnityEngine;

namespace _2._Scripts.Core.Engine.Service.SaveLoad.Structure
{
    [Serializable]
    public class PlayerDataDTO
    {
        public string Name;
        public int Level;
        public int Speed;
        public Vector3 Position;
        
        public PlayerDataDTO(PlayerCharacter playerCharacter)
        {
            Name = playerCharacter.Name;
            Level = playerCharacter.Level;
            Speed = playerCharacter.Speed;
            Position = playerCharacter.transform.position;
        }
    }
}