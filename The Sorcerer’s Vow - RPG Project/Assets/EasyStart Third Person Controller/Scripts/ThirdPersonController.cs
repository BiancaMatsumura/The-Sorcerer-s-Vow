using _2._Scripts.Core.Domain.Character.Player;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    public float sprintAdittion = 3.5f;
    public float jumpForce = 18f;
    public float jumpTime = 0.85f;
    public float gravity = 9.8f;

    float jumpElapsedTime = 0;

    bool isStasis = false;
    bool isJumping = false;
    bool isSprinting = false;
    bool isCrouching = false;

    float inputHorizontal;
    float inputVertical;
    bool inputJump;
    bool inputCrouch;
    bool inputSprint;
    public GameObject BOLADEFOGO;
    Animator animator;
    public CharacterController cc;
    [SerializeField]
    private PlayerCharacter playerCharacter;
    public  Transform spawnTarget;

    public BoxCollider[] collider;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerCharacter = GetComponent<PlayerCharacter>();


        if (animator == null)
            Debug.LogWarning("Hey buddy, you don't have the Animator component in your player. Without it, the animations won't work.");
    }

    void Update()
    {
        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");
        inputJump = Input.GetAxis("Jump") == 1f;
        inputSprint = Input.GetAxis("Fire3") == 1f;
        inputCrouch = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.JoystickButton1);
        if (!isStasis) { 
            if (inputCrouch)
                isCrouching = !isCrouching;

            if (cc.isGrounded && animator != null)
            {
                animator.SetBool("crouch", isCrouching);

                float minimumSpeed = 0.9f;
                animator.SetBool("run", cc.velocity.magnitude > minimumSpeed);
                isSprinting = cc.velocity.magnitude > minimumSpeed && inputSprint;
                animator.SetBool("sprint", isSprinting);
            }
        }
        if (animator != null)
            animator.SetBool("air", !cc.isGrounded);

        if (inputJump && cc.isGrounded)
        {
            isJumping = true;
        }

        HeadHittingDetect();

        if (animator.IsInTransition(0))
        {   //qnd estiver em transição impede que a animação se repita
            isStasis = false;
            animator.SetBool("Kick", false);
            animator.SetBool("Punch", false);
        }

        if (Input.GetMouseButtonDown(0) && !isJumping && !isCrouching)
        {
            BattleSistem(1);
            
        }
        if (Input.GetMouseButtonDown(1) && !isJumping && !isCrouching)
        {
            BattleSistem(2);
            
        }
        if (Input.GetKey(KeyCode.F) && !isCrouching)
        {
            BattleSistem(3);
        }
    }

    private void FixedUpdate()
    {
        float baseSpeed = playerCharacter.Speed;
        float velocityAdittion = 0;

        if (isSprinting)
            velocityAdittion = sprintAdittion;
        if (isCrouching)
            velocityAdittion = -(baseSpeed * 0.50f);

        float directionX = inputHorizontal * (baseSpeed + velocityAdittion) * Time.deltaTime;
        float directionZ = inputVertical * (baseSpeed + velocityAdittion) * Time.deltaTime;
        float directionY = 0;

        if (isJumping)
        {
            directionY = Mathf.SmoothStep(jumpForce, jumpForce * 0.30f, jumpElapsedTime / jumpTime) * Time.deltaTime;
            jumpElapsedTime += Time.deltaTime;
            if (jumpElapsedTime >= jumpTime)
            {
                isJumping = false;
                jumpElapsedTime = 0;
            }
        }

        directionY -= gravity * Time.deltaTime;

        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        forward *= directionZ;
        right *= directionX;

        if (directionX != 0 || directionZ != 0)
        {
            float angle = Mathf.Atan2(forward.x + right.x, forward.z + right.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.15f);
        }

        Vector3 verticalDirection = Vector3.up * directionY;
        Vector3 horizontalDirection = forward + right;
        Vector3 moviment = verticalDirection + horizontalDirection;

        cc.Move(moviment);
    }

    void HeadHittingDetect()
    {
        float headHitDistance = 1.1f;
        Vector3 ccCenter = transform.TransformPoint(cc.center);
        float hitCalc = cc.height / 2f * headHitDistance;

        if (Physics.Raycast(ccCenter, Vector3.up, hitCalc))
        {
            jumpElapsedTime = 0;
            isJumping = false;
        }
    }
    void BattleSistem(int var)
    {
        //batle sistem 
        switch (var)
        {
            case 1:
                animator.SetBool("Kick", true);
                isStasis = true;
                break;
            case 2:
                animator.SetBool("Punch", true);
                isStasis = true;
                break;
            case 3:
                animator.Play("Power");
      
                break; 
        }
    }

    void ActiveCollider()
    {
        int qnt = collider.Length;
        for (int i = 0; i < qnt; i++)
        {
            collider[i].enabled = true;
        }
    }

    void DesactiveCollider()
    {
        int qnt = collider.Length;
        for (int i = qnt - 1; i >= 0; i--)
        {
            collider[i].enabled = false;
        }

    
    }
    void FireACtive() 
    {
        GameObject ball = Instantiate(BOLADEFOGO, spawnTarget.position, Quaternion.identity);

        float ballSpeed = 4.0f;
        // Pega a direção que o player está olhando (apenas no plano XZ)
        Vector3 direction = transform.forward;
        direction.y = 0;
        direction.Normalize();

        // Faz a bola andar nessa direção
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * ballSpeed;
        }
    }
}