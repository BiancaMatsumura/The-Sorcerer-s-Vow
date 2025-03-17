using System.Collections.Generic;
using System.IO;
using _2._Scripts.Core.Domain.Character.Player;
using UnityEngine;

namespace _2._Scripts.Core.Engine.Service.SaveLoad
{
    public class SaveLoadProgressManager
    {
        private static readonly string FileName = "save-data.json";

        private static List<PlayerCharacter> _saveLoadPlayerCharacters = new List<PlayerCharacter>();

        public static int CurrentSave = 0;

        public static void Save(int slot, PlayerCharacter playerCharacter)
        {
            if (_saveLoadPlayerCharacters.Count == 0)
            {
                _saveLoadPlayerCharacters.Add(playerCharacter);
            }
            else
            {
                _saveLoadPlayerCharacters[slot] = playerCharacter;
            }

            StoreSaveListToFile();
        }

        public static void Save(PlayerCharacter playerCharacter)
        {
            _saveLoadPlayerCharacters.Add(playerCharacter);
            StoreSaveListToFile();
        }


        public static List<PlayerCharacter> SwitchSlots(int from, int to)
        {
            if (from < 0 || from >= _saveLoadPlayerCharacters.Count || to < 0 || to >= _saveLoadPlayerCharacters.Count)
            {
                Debug.LogError("Invalid slot indexes provided for the switch.");
                return _saveLoadPlayerCharacters;
            }
            
            (_saveLoadPlayerCharacters[from], _saveLoadPlayerCharacters[to]) = (_saveLoadPlayerCharacters[to], _saveLoadPlayerCharacters[from]);
            
            StoreSaveListToFile();
            return _saveLoadPlayerCharacters;
        }


        public static List<PlayerCharacter> ListAllSaves()
        {
            return _saveLoadPlayerCharacters;
        }

        public static PlayerCharacter LoadSave(int slot)
        {
            CurrentSave = slot;
            return _saveLoadPlayerCharacters[slot];
        }

        private static void StoreSaveListToFile()
        {
            SaveFileModel.Instance.Saves = _saveLoadPlayerCharacters;
            string initialSaveJson = JsonUtility.ToJson(SaveFileModel.Instance);
            using (FileStream file = new FileStream(FileName, FileMode.Create))
            {
                byte[] dataBytes = System.Text.Encoding.ASCII.GetBytes(initialSaveJson);
                file.Write(dataBytes, 0, dataBytes.Length);
            }
        }

        private static void LoadSaveListFromFile()
        {
            if (!System.IO.File.Exists(FileName))
            {
                _saveLoadPlayerCharacters = new List<PlayerCharacter>();
                StoreSaveListToFile();
            }

            try
            {
                using FileStream file = new FileStream(FileName, FileMode.Open);
                byte[] dataBytes = new byte[file.Length];
                file.Read(dataBytes, 0, dataBytes.Length);

                string encryptedData = System.Text.Encoding.ASCII.GetString(dataBytes);

                SaveFileModel.Instance = JsonUtility.FromJson<SaveFileModel>(encryptedData);
                _saveLoadPlayerCharacters = SaveFileModel.Instance.Saves;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                _saveLoadPlayerCharacters = new List<PlayerCharacter>();
                StoreSaveListToFile();
            }
        }
    }
}