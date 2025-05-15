using System;
using _2._Scripts.Core.Domain.Character.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _2._Scripts.Core.Engine.Service.SaveLoad.Structure
{
    [Serializable]
    public class PlayerDataDTO
    {
        public string Name;
        public int Level;
        public int Speed;
        public float CurrentHealth;
        public Scene CurrentScene;
        public Vector3 Position;
        
        public PlayerDataDTO(PlayerCharacter playerCharacter)
        {
            Name = playerCharacter.Name;
            Level = playerCharacter.Level;
            Speed = playerCharacter.Speed;
            CurrentHealth = playerCharacter.currentHealth;
            CurrentScene = playerCharacter.currentScene;
            Position = playerCharacter.transform.position;
        }
    }
}