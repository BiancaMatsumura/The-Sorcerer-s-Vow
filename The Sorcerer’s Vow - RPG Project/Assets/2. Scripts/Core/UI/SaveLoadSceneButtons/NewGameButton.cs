using System;
using _2._Scripts.Core.Domain.Character.Player;
using _2._Scripts.Core.Engine.Service.SaveLoad;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace _2._Scripts.Core.UI.SaveLoadSceneButtons
{
    public class NewGameButton : MonoBehaviour
    {
        [SerializeField] private int slot = 0;
        private Image panel;
        private TMPro.TMP_Text text;

        private void Start()
        {
            panel = GetComponent<Image>();
            
            text = GetComponentInChildren<TMPro.TMP_Text>();
            
            Button button = panel.GetComponent<Button>();
            if (button == null)
            {
                button = panel.gameObject.AddComponent<Button>();
            }
            button.onClick.AddListener(OnPanelClick);

            EventTrigger trigger = panel.gameObject.AddComponent<EventTrigger>();

            var entryHover = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entryHover.callback.AddListener((data) => OnMouseEnter());
            trigger.triggers.Add(entryHover);

            var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            entryExit.callback.AddListener((data) => OnMouseExit());
            trigger.triggers.Add(entryExit);
        }

        private void OnMouseEnter()
        {
            text.color = Color.black;
        }

        private void OnMouseExit()
        {
            text.color = Color.white;
        }

        private void OnPanelClick()
        {
            PlayerCharacter playerCharacter;
            GameObject playerObject = new GameObject("PlayerCharacter");
            playerCharacter = playerObject.AddComponent<PlayerCharacter>();

            playerCharacter.Level = 1;
            playerCharacter.Name = "Player " + (slot + 1);
            playerCharacter.Speed = 30;

            SaveLoadProgressManager.Save(slot, playerCharacter);
        }
    }
}