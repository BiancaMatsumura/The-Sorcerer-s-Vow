using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _2._Scripts.Core.UI.SaveLoadSceneButtons
{
    public class LoadGameButton : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text saveName;
        [SerializeField] private TMPro.TMP_Text saveLevel;

        public void SetTextData(string name, int level)
        {
            saveName.text = name;
            saveLevel.text = "Level: " + level.ToString();
        }
        
    }
}