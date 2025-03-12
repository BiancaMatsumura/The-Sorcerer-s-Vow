using UnityEngine;

public class SimpleNPC : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            transform.GetComponent<Quest>().CheckQuest();
        }
    }
}
