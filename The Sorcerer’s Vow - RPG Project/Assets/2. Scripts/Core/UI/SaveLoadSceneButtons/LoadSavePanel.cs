using _2._Scripts.Core.Engine.Service.SaveLoad;
using UnityEngine;

namespace _2._Scripts.Core.UI.SaveLoadSceneButtons
{
    public class LoadSavePanel : MonoBehaviour
    {
        private SaveFileDTO slot0;
        private SaveFileDTO slot1;
        private SaveFileDTO slot2;

        [SerializeField] private LoadGameButton panelLoad0;
        [SerializeField] private LoadGameButton panelLoad1;
        [SerializeField] private LoadGameButton panelLoad2;
        
        [SerializeField] private NewGameButton panelNewGame0;
        [SerializeField] private NewGameButton panelNewGame1;
        [SerializeField] private NewGameButton panelNewGame2;
        
        private void Start()
        {
            RefreshScreen();
        }

        public void RefreshScreen()
        {
            SaveLoadProgressManager.LoadAllSaveDataFromFile();
            slot0 = SaveLoadProgressManager.GetSlotDataFromSaveFile(0);
            slot1 = SaveLoadProgressManager.GetSlotDataFromSaveFile(1);
            slot2 = SaveLoadProgressManager.GetSlotDataFromSaveFile(2);

            panelLoad0.gameObject.SetActive(false);
            panelLoad1.gameObject.SetActive(false);
            panelLoad2.gameObject.SetActive(false);
            panelNewGame0.gameObject.SetActive(false);
            panelNewGame1.gameObject.SetActive(false);
            panelNewGame2.gameObject.SetActive(false);
            
            if (slot0.PlayerData.Name != "")
            {
                panelLoad0.SetTextData(slot0.PlayerData.Name, slot0.PlayerData.Level, slot0.SceneName);
                panelLoad0.gameObject.SetActive(true);
            }
            else
            {
                panelNewGame0.gameObject.SetActive(true);
            }
            
            if (slot1.PlayerData.Name != "")
            {
                panelLoad1.SetTextData(slot1.PlayerData.Name, slot1.PlayerData.Level, slot0.SceneName);
                panelLoad1.gameObject.SetActive(true);
            }
            else
            {
                panelNewGame1.gameObject.SetActive(true);
            }
            
            if (slot2.PlayerData.Name != "")
            {
                panelLoad2.SetTextData(slot2.PlayerData.Name, slot2.PlayerData.Level, slot0.SceneName);
                panelLoad2.gameObject.SetActive(true);
            }
            else
            {
                panelNewGame2.gameObject.SetActive(true);
            }
        }
    }
}