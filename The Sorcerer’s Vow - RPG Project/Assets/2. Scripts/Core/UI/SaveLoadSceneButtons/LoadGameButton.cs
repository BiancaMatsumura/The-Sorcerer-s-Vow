using System;
using _2._Scripts.Core.Engine.Service.SaveLoad;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _2._Scripts.Core.UI.SaveLoadSceneButtons
{
    public class LoadGameButton : MonoBehaviour
    {
        [SerializeField] private int slot = 0;
        [SerializeField] private TMPro.TMP_Text saveName;
        [SerializeField] private TMPro.TMP_Text saveLevel;
        private string sceneName;
        
        private Image panel;

        private void Start()
        {
            panel = GetComponent<Image>();
            Button button = panel.GetComponent<Button>();
            if (button == null)
            {
                button = panel.gameObject.AddComponent<Button>();
            }
            button.onClick.AddListener(OnPanelClick);
        }

        public void SetTextData(string name, int level, string scene)
        {
            saveName.text = name;
            sceneName = scene;
            saveLevel.text = "Level: " + level;
        }
        
        private void OnPanelClick()
        {
            SaveLoadProgressManager.SetCurrentSlot(slot);
            SceneManager.LoadScene(sceneName);
        }
        
    }
}