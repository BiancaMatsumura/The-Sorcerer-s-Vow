using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TotenSpawnEnemy : MonoBehaviour
{

    [Header("Vida")]
    [SerializeField] private float maxHealth = 50f;
    private float currentHealth;

    [Header("UI")]
    [SerializeField] private Slider sliderLife;

    private Animator anim;
    private bool isDead = false;

    public bool isSpawned = false;

    [SerializeField] private GameObject enemyPrefab;
    private BoxCollider col;

    public int enemyQuantity = 3;
    public float spawnInterval = 5f;
    public int spawnQuantity = 3;

    private Transform mainCamera;


    void Awake()
    {
        col = GetComponent<BoxCollider>();
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
        isDead = false;
        sliderLife.maxValue = maxHealth;
        sliderLife.value = currentHealth;
    }


    void Start()
    {
        mainCamera = Camera.main.transform;
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;

        if (sliderLife != null)
        {
            sliderLife.maxValue = maxHealth;
            sliderLife.value = currentHealth;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entrou no totem");

        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        if (sliderLife != null)
            sliderLife.value = currentHealth;

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{name} morreu!");
        if (anim != null)
            anim.Play("destroyTotem01");

        StartCoroutine(DestroyAfterSeconds(5f));
    }

    private IEnumerator DestroyAfterSeconds(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
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


    void Update()
    {

        if (sliderLife != null && sliderLife.gameObject.activeSelf)
        {
            sliderLife.transform.LookAt(mainCamera);
            sliderLife.transform.Rotate(0f, 180f, 0f);
        }

        sliderLife.value = currentHealth;

        if (Input.GetKeyDown(KeyCode.E) && !isSpawned)
        {
            if (!isSpawned)
            {
                StartCoroutine(SpawnRoutine());

            }

        }
        if (Input.GetKeyDown(KeyCode.L) && !isSpawned)
        {
            anim.Play("destroyTotem01");

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
