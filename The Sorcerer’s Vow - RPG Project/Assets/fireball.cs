using UnityEngine;

public class fireball : MonoBehaviour
{
    public float speed;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 frente = Vector3.forward;
        transform.localPosition += frente * speed * Time.deltaTime;
    }
}
