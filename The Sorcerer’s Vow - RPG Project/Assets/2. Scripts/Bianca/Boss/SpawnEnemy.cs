using System.Collections;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    public bool isSpawned = false;

    [SerializeField] private GameObject enemyPrefab;
    private BoxCollider col;

    public int enemyQuantity = 3;
    public float spawnInterval = 5f;
    public int spawnQuantity = 3;


    void Awake()
    {
        col = GetComponent<BoxCollider>();
    }

    public void Spawn()
    {
        Vector3 point = GetRandomPointInCollider();
        GameObject enemyObj = Instantiate(enemyPrefab, point, Quaternion.identity);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.SetPatrolPoints(PatrolManager.Instance.GetPatrolPoints());
            enemy.target = GameObject.FindWithTag("Player").transform;
        }

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
        if (Input.GetKeyDown(KeyCode.E) && !isSpawned)
        {
            if (!isSpawned)
            {
                StartCoroutine(SpawnRoutine());
                
            }

        }

    }

    IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < spawnQuantity; i++)
        {
            for (int t = 0; t < enemyQuantity; t++)
            {
                Spawn();
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForSeconds(spawnInterval);
        }
        isSpawned = true;
    }
}
