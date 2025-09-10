using System.Collections.Generic;
using System.IO;
using _2._Scripts.Core.Domain.Character.Player;
using UnityEngine;

namespace _2._Scripts.Core.Engine.Service.SaveLoad
{
    public class SaveLoadProgressManager
    {
        private static readonly string FileName = Path.Combine(Application.persistentDataPath, "save-data.json");

        private static SaveFileDTO[] _slots = new SaveFileDTO[3];

        public static int CurrentSave { get; private set; }

        public static void Save(int slot, PlayerCharacter playerCharacter, int currentQuestIndex = 0, string sceneName = "")
        {
            if (slot < 0 || slot > 2)
            {
                Debug.LogError($"Invalid slot index {slot} provided for the save.");
                throw new System.Exception("Invalid slot index provided for the save.");
            }
            _slots[slot] = new SaveFileDTO(slot, playerCharacter, currentQuestIndex, sceneName);
            StoreSaveListToFile();
            CurrentSave = slot;
            Debug.Log($"Save successful in slot {slot}.");
        }

        private static void StoreSaveListToFile()
        {
            var saves = new List<SaveFileDTO>(_slots);
            SaveFileModel.Instance.Saves = saves;
            string initialSaveJson = JsonUtility.ToJson(SaveFileModel.Instance);
            Debug.Log($"Saving data: {initialSaveJson}");
            File.WriteAllText(FileName, initialSaveJson);
        }

        public static void LoadAllSaveDataFromFile()
        {
            if (!File.Exists(FileName))
            {
                _slots = new SaveFileDTO[3];
                StoreSaveListToFile();
            }
            try
            {
                string encryptedData = File.ReadAllText(FileName);
                SaveFileModel.Instance = JsonUtility.FromJson<SaveFileModel>(encryptedData);
                for (int i = 0; i < 3; i++)
                {
                    _slots[i] = (SaveFileModel.Instance.Saves != null && SaveFileModel.Instance.Saves.Count > i) ? SaveFileModel.Instance.Saves[i] : null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load save data: {e}");
            }
        }

        public static SaveFileDTO GetSlotDataFromSaveFile(int slot)
        {
            if (slot < 0 || slot > 2) return null;
            return _slots[slot];
        }

        public static void SetCurrentSlot(int slot)
        {
            if (slot < 0 || slot > 2) return;
            CurrentSave = slot;
        }
    }
}