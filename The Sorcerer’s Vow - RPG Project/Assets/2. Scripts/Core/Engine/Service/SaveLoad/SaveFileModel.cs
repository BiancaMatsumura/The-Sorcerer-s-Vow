using System.Collections.Generic;
using _2._Scripts.Core.Domain.Character.Player;

namespace _2._Scripts.Core.Engine.Service.SaveLoad
{
    [System.Serializable]
    public class SaveFileModel
    {
        public List<PlayerCharacter> Saves;
        public static SaveFileModel Instance = new SaveFileModel();
    }
}