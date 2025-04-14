using System;
using UnityEngine;

namespace _2._Scripts.Core.Domain.Character.Base
{
    [Serializable]
    public class BaseCharacter : MonoBehaviour
    {
        public string Name;
        public int Level;
        public int Speed;

        public virtual void Start()
        {
            Debug.Log("BaseCharacter Start");
        }

        public virtual void Update()
        {
            // Debug.Log("BaseCharacter Update");
        }
    }
}