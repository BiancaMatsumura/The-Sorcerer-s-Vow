using System;
using _2._Scripts.Core.Domain.Character.Player.Inputs;
using UnityEngine;

namespace _2._Scripts.Core.Domain.Character.Player.Inputs
{
    public class PlayerInputController : MonoBehaviour
    {
        
        private IInput saveShortcutInput;

        private void Start()
        {
            //saveShortcutInput = new SaveShortcutInput();
        }
        
        private void Update()
        {
            if (Input.anyKeyDown)
            {
                string pressedKey = Input.inputString;
                if (!string.IsNullOrEmpty(pressedKey))
                {
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    PlayerCharacter playerCharacter = player.GetComponent<PlayerCharacter>();
                    //(saveShortcutInput as SaveShortcutInput).SetPlayerCharacter(playerCharacter);
                    saveShortcutInput.input(pressedKey);
                }
            }

        }
    }
}