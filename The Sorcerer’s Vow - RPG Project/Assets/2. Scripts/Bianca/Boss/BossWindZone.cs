using UnityEngine;

public class BossWindZone : MonoBehaviour
{
    public float forceStrength = 10f;
    public float radius = 5f;
    public bool attract = false; // true = puxa, false = empurra
    public Transform bossCenter; // ponto central do efeito

    void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector3.Distance(player.transform.position, bossCenter.position);
        if (distance < radius)
        {
            Vector3 dir = (player.transform.position - bossCenter.position).normalized;
            if (attract) dir = -dir;

            player.GetComponent<ThirdPersonController>().AddExternalForce(dir * forceStrength);
        }
    }
}
