using _2._Scripts.Core.Engine.Service.SaveLoad;
using UnityEngine;

namespace _2._Scripts.Core.Domain.Character.Player.Inputs.Shurtcuts
{
    public class SaveShortcutInput : IInput
    {
        private PlayerCharacter playerCharacter;

        public void input(string key)
        {
            Debug.Log("Salvando... " + key + " --- " + playerCharacter.ToString());
            if (key == "p" && playerCharacter)
            {
                Debug.Log("Salvando 2...");
                SaveLoadProgressManager.Save(0, playerCharacter);
            }
        }

        public void SetPlayerCharacter(PlayerCharacter playerCharacter)
        {
            this.playerCharacter = playerCharacter;
        }
    }
}