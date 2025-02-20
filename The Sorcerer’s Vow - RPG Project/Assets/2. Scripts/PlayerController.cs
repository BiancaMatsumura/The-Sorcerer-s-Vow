using UnityEngine;

public class PlayerController : MonoBehaviour
{
   public float speed = 5f; 
    private Rigidbody rb; 

    void Start()
    {
        rb = GetComponent<Rigidbody>(); 
    }

    void FixedUpdate()
    {
        float moveX = Input.GetAxis("Horizontal"); 
        float moveZ = Input.GetAxis("Vertical"); 

        Vector3 move = new Vector3(moveX, 0f, moveZ) * speed;
        Vector3 newPosition = rb.position + move * Time.fixedDeltaTime;

        rb.MovePosition(newPosition); // Move o player suavemente
    }
}
