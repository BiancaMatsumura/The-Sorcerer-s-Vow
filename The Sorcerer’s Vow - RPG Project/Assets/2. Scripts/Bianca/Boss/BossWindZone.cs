using UnityEngine;

public class BossWindZone : MonoBehaviour
{
    public float forceStrength = 0.5f;
    public bool attract = false; // true = puxa, false = empurra
    public Transform bossCenter;
    public float oscillationAmplitude = 60f;
    public float oscillationSpeed = 1.5f;
    public bool isActive = false;

    private void Start()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isActive)
        {
            Vector3 dir = (other.transform.position - bossCenter.position).normalized;
            if (attract) dir = -dir;

            other.GetComponent<ThirdPersonController>().AddExternalForce(dir * forceStrength);
        }
    }

    private void Update()
    {
        if (isActive)
        RotateWindZone();

    }

    private void RotateWindZone()
    {
        float angle = Mathf.Sin(Time.time * oscillationSpeed) * oscillationAmplitude;
        transform.localRotation = Quaternion.Euler(0, angle, 0);
    }
    
    public void ActivateWindZone()
    {
        isActive = true;
    }
}
