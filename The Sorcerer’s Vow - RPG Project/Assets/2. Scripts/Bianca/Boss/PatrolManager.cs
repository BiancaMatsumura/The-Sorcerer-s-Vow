using UnityEngine;

public class PatrolManager : MonoBehaviour
{
    public static PatrolManager Instance; // Singleton simples para facilitar

    [Header("Pontos de Patrulha")]
    public Transform[] patrolPoints;

    private void Awake()
    {
        Instance = this;
    }

    public Transform[] GetPatrolPoints()
    {
        return patrolPoints;
    }
}
