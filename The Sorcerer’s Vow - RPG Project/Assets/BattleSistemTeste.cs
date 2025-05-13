using Unity.VisualScripting;
using UnityEngine;

public class BattleSistemTeste : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    private Rigidbody rb;
    private bool isGrounded;
    public Animator Anime;
    private float rotationSpeed = 4.0f;
    bool IsStasis = false;
    public BoxCollider collider;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
    }

    void Update()
    {

     

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        if(!IsStasis) { 

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        Vector3 rotate = transform.rotation * move;

           rb.linearVelocity = new Vector3(move.x * moveSpeed, rb.linearVelocity.y, move.z * moveSpeed);

           if (move != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(move, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime));
            }

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                Anime.Play("Jump");
            }

            if (move != Vector3.zero)
            {
                Anime.SetBool("isRunnig", true);
            }

            else { Anime.SetBool("isRunnig", false); }

        }
        if (Anime.IsInTransition(0))
        {
            Anime.SetBool("Kick", false);
        }


        if (Input.GetMouseButtonDown(0) && isGrounded)
        {
            BattleSistem(1);
        }
    }

    // Detecta ch�o
    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
        Anime.SetBool("IsGround", true);
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
        Anime.SetBool("IsGround", false);
    }


    void BattleSistem(int var) 
    {
        switch(var)
        {
            case 1:
                Anime.SetBool("Kick", true);
               
                break;


        }
    }

    void ActiveColider() 
    {
        collider.enabled = true;
    }
    void DesactiveColider()
    {
        collider.enabled = false;
    }
}
