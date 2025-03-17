using _2._Scripts.Core.Domain.Character.Base;
using UnityEngine;

namespace _2._Scripts.Core.Domain.Character.Player
{
    [System.Serializable]
    public class PlayerCharacter : BaseCharacter
    {
        public override void Start()
        {
            base.Start();
            Debug.Log("PlayerCharacter Start");
        }

        public override void Update()
        {
            base.Update();
            Debug.Log("PlayerCharacter Update");
        }
    }
}