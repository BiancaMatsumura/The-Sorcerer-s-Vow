using _2._Scripts.Core.Domain.Character.Player;
using _2._Scripts.Core.Engine.Service.SaveLoad;
using UnityEngine;

namespace _2._Scripts.Core.UI.SaveLoadSceneButtons
{
    public class SaveButtonTest : MonoBehaviour
    {

        private PlayerCharacter playerCharacter;
        
        public void OnClick()
        {
            Debug.Log("Save");
            SaveLoadProgressManager.Save(0, playerCharacter);
        }

        public void SetPlayerCharacter(PlayerCharacter playerCharacter)
        {
            this.playerCharacter = playerCharacter;
        }
        
    }
}