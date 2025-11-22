using _2._Scripts.Core.Domain.Character.Player;
using UnityEngine;

public class GameOver_River : MonoBehaviour
{
    [SerializeField] private PlayerCharacter playerCharacter;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerCharacter.isDead = true;
        }
    }
}
