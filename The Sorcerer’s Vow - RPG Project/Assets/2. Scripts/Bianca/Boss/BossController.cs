using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum BossState
{
    Idle,
    AttackNormal,
    AttackArea,
    SpawnTotem,
    Wind
}

public class BossController : MonoBehaviour
{

    [Header("Referências")]
    //[SerializeField] private Animator anim;
    [SerializeField] private SpawnToten spawnTotem; // script que instancia os totens
    [SerializeField] private BossWindZone windZone; // script do vento

    [Header("Controle manual")]
    public BossState currentState = BossState.Idle;
    public float actionCooldown = 2f;
    private bool isBusy = false;

    private BossCharacter boss;

    void Start()
    {
        
        //if (anim == null) anim = GetComponent<Animator>();
        boss = GetComponent<BossCharacter>();
    }

    void Update()
    {
        if (!isBusy)
        {
            if (Input.GetKeyDown(KeyCode.F1)) SetState(BossState.AttackNormal);
            if (Input.GetKeyDown(KeyCode.F2)) SetState(BossState.AttackArea);
            if (Input.GetKeyDown(KeyCode.F3)) SetState(BossState.SpawnTotem);
            if (Input.GetKeyDown(KeyCode.F4)) SetState(BossState.Wind);

            if (currentState != BossState.Idle)
                StartCoroutine(HandleState());
        }

        if (Input.GetMouseButtonDown(0)) // clique esquerdo
        {
            boss.GetComponent<BossCharacter>().ReduceHealth(5f);
        }
    }

    IEnumerator HandleState()
    {
        isBusy = true;

        switch (currentState)
        {
            case BossState.Idle:
                Debug.Log("Boss parado...");
                break;

            case BossState.AttackNormal:
                Debug.Log("Boss faz ataque normal!");
                //anim?.SetTrigger("AttackNormal");
                break;

            case BossState.AttackArea:
                Debug.Log("Boss faz ataque em área!");
                //anim?.SetTrigger("AttackArea");
                break;

            case BossState.SpawnTotem:
                Debug.Log("Boss invoca totem!");
                spawnTotem?.Spawn();
                break;

            case BossState.Wind:
                Debug.Log("Boss ativa vento!");
                if (windZone != null)
                {
                    windZone.ActivateWindZone();
                    yield return new WaitForSeconds(5f);
                    windZone.isActive = false;
                }
                break;
        }

        yield return new WaitForSeconds(actionCooldown);
        currentState = BossState.Idle; // volta para Idle
        isBusy = false;
    }

    public void SetState(BossState newState)
    {
        currentState = newState;
    }
}
