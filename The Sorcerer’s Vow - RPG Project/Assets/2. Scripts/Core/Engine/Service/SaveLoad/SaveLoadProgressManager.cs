using System.Collections.Generic;
using System.IO;
using _2._Scripts.Core.Domain.Character.Player;
using UnityEngine;

namespace _2._Scripts.Core.Engine.Service.SaveLoad
{
    public class SaveLoadProgressManager
    {
        private static readonly string FileName = "save-data.json";

        private static SaveFileDTO slot0;
        private static SaveFileDTO slot1;
        private static SaveFileDTO slot2;

        public static int CurrentSave = 0;

        public static void Save(int slot, PlayerCharacter playerCharacter)
        {
            if (slot == 0)
            {
                slot0 = new SaveFileDTO(0, playerCharacter);
            } else if (slot == 1)
            {
                slot1 = new SaveFileDTO(1, playerCharacter);
            } else if (slot == 2)
            {
                slot2 = new SaveFileDTO(2, playerCharacter);
            }
            else
            {
                Debug.LogError("Invalid slot index provided for the save.");
                throw new System.Exception("Invalid slot index provided for the save.");
            }

            StoreSaveListToFile();
            
            Debug.Log("Save successful.");
        }

        private static void StoreSaveListToFile()
        {
            var saves = new List<SaveFileDTO>();
            saves.Add(slot0);
            saves.Add(slot1);
            saves.Add(slot2);
            
            SaveFileModel.Instance.Saves = saves;
            string initialSaveJson = JsonUtility.ToJson(SaveFileModel.Instance);
            
            Debug.Log(initialSaveJson);
            
            using (FileStream file = new FileStream(FileName, FileMode.Create))
            {
                byte[] dataBytes = System.Text.Encoding.ASCII.GetBytes(initialSaveJson);
                file.Write(dataBytes, 0, dataBytes.Length);
            }
        }
        
        public static void LoadAllSaveDataFromFile()
        {
            if (!File.Exists(FileName))
            {
                StoreSaveListToFile();
            }
            try
            {
                using FileStream file = new FileStream(FileName, FileMode.Open);
                byte[] dataBytes = new byte[file.Length];
                file.Read(dataBytes, 0, dataBytes.Length);
        
                string encryptedData = System.Text.Encoding.ASCII.GetString(dataBytes);
        
                SaveFileModel.Instance = JsonUtility.FromJson<SaveFileModel>(encryptedData);
                
                slot0 = SaveFileModel.Instance.Saves[0];
                slot1 = SaveFileModel.Instance.Saves[1];
                slot2 = SaveFileModel.Instance.Saves[2];
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }
        
        public static SaveFileDTO GetSlotDataFromSaveFile(int slot)
        {
            switch (slot)
            {
                case 0:
                    return slot0;
                case 1:
                    return slot1;
                case 2:
                    return slot2;
                default:
                    return null;
            }
        }
    }
}