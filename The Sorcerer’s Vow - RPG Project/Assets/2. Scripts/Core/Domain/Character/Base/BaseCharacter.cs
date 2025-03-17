using System;
using UnityEngine;

namespace _2._Scripts.Core.Domain.Character.Base
{
    public class BaseCharacter : MonoBehaviour
    {
        public string Name { get; set; }
        public int Level { get; set;  }
        public int Speed { get; set;  }

        public virtual void Start()
        {
            Debug.Log("BaseCharacter Start");
        }

        public virtual void Update()
        {
            Debug.Log("BaseCharacter Update");
        }
    }
}