using System.Collections;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    private void Start()
    {
        StartCoroutine(SetSpawn());
    }

    private IEnumerator SetSpawn()
    {
        // garante que nada mova o Player no mesmo frame
        GetComponent<ThirdPersonController>().SendMessage("FreezeNextFrame");

        yield return new WaitForEndOfFrame();

        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
    }
}
