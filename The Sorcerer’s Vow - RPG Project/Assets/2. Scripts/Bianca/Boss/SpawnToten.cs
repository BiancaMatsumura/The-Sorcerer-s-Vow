using UnityEngine;

public class SpawnToten : MonoBehaviour
{
    public bool isSpawned = false;

    [SerializeField] private GameObject totenPrefab;
    private BoxCollider col;


    void Awake()
    {
        col = GetComponent<BoxCollider>();
        
    }

    public void Spawn()
    {
        Vector3 point = GetRandomPointInCollider();
        Instantiate(totenPrefab, point, Quaternion.identity);
    }

    Vector3 GetRandomPointInCollider()
    {
        Vector3 bounds = col.size / 2f;

        float x = Random.Range(-bounds.x, bounds.x);
        float y = 0;
        float z = Random.Range(-bounds.z, bounds.z);

        return col.transform.position + col.transform.rotation * new Vector3(x, y, z);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider>().size);
    }

    void Start()
    {

    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && !isSpawned)
        {
            if (!isSpawned)
            {
                Spawn();
                //isSpawned = true;
            }

        }
        

    }
}
