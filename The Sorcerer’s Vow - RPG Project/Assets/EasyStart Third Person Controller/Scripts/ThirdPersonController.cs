using _2._Scripts.Core.Domain.Character.Player;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    public float sprintAdittion = 3.5f;
    public float jumpForce = 18f;
    public float jumpTime = 0.85f;
    public float gravity = 9.8f;
    public float airKickMomentumMultiplier = 1.2f; // Multiplicador de momento para o chute no ar

    float jumpElapsedTime = 0;

    bool isStasis = false;
    bool isJumping = false;
    bool isSprinting = false;
    bool isCrouching = false;
    bool isAirKicking = false; // Nova variável para controlar o chute no ar

    float inputHorizontal;
    float inputVertical;
    bool inputJump;
    bool inputCrouch;
    bool inputSprint;

    float lastHorizontalInput;
    float lastVerticalInput;

    public GameObject BOLADEFOGO;
    public Transform spawnTarget;
    public BoxCollider[] collider;

    private int currentAttackType = 0; // 1 = chute, 2 = soco, 3 = bola de fogo

    Animator animator;
    public CharacterController cc;
    [SerializeField]
    private PlayerCharacter playerCharacter;

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
        // Capturar inputs
        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");
        inputJump = Input.GetAxis("Jump") == 1f;
        inputSprint = Input.GetAxis("Fire3") == 1f;
        inputCrouch = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.JoystickButton1);

        // Sempre armazenar o último input válido para uso durante ataques
        if (inputHorizontal != 0)
            lastHorizontalInput = inputHorizontal;
        if (inputVertical != 0)
            lastVerticalInput = inputVertical;

        // Verificar se deve aplicar stasis de movimento
        bool shouldApplyStasis = isStasis && !(isAirKicking || (currentAttackType == 1 && !cc.isGrounded));

        // Se estiver em stasis sem ser chute no ar, zerar inputs
        if (shouldApplyStasis)
        {
            inputHorizontal = 0;
            inputVertical = 0;
        }

        // Lógica de agachamento
        if (!shouldApplyStasis)
        {
            if (inputCrouch)
                isCrouching = !isCrouching;
        }

        // Atualizar animações baseadas no estado do personagem
        if (cc.isGrounded && animator != null)
        {
            animator.SetBool("crouch", isCrouching);

            float minimumSpeed = 0.9f;
            animator.SetBool("run", cc.velocity.magnitude > minimumSpeed);
            isSprinting = cc.velocity.magnitude > minimumSpeed && inputSprint;
            animator.SetBool("sprint", isSprinting);
        }

        if (animator != null)
        {
            animator.SetBool("air", !cc.isGrounded);

            float horizontalSpeed = new Vector3(cc.velocity.x, 0, cc.velocity.z).magnitude;
            animator.SetFloat("Speed", horizontalSpeed);
        }

        // Lógica de pulo
        if (inputJump && cc.isGrounded && !shouldApplyStasis)
        {
            isJumping = true;
        }

        HeadHittingDetect();

        // Ataques
        if (Input.GetMouseButtonDown(0) && !isCrouching)
        {
            BattleSistem(1); // Chute - agora permitido no ar
        }
        if (Input.GetMouseButtonDown(1) && !isJumping && !isCrouching)
        {
            BattleSistem(2); // Soco
        }
        if (Input.GetKey(KeyCode.F) && !isCrouching)
        {
            BattleSistem(3); // Bola de fogo
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

        // Se estiver executando um chute no ar, use o último input válido multiplicado pelo multiplicador de momento
        float effectiveHorizontal = inputHorizontal;
        float effectiveVertical = inputVertical;

        if (isAirKicking)
        {
            effectiveHorizontal = lastHorizontalInput * airKickMomentumMultiplier;
            effectiveVertical = lastVerticalInput * airKickMomentumMultiplier;
        }

        float directionX = effectiveHorizontal * (baseSpeed + velocityAdittion) * Time.deltaTime;
        float directionZ = effectiveVertical * (baseSpeed + velocityAdittion) * Time.deltaTime;
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
        if (isStasis && !(var == 1 && !cc.isGrounded))
            return;

        currentAttackType = var;

        // Para chute no ar, configurar a variável específica
        if (var == 1 && !cc.isGrounded)
        {
            isAirKicking = true;
            // Para chute no ar, não entramos em stasis completo
        }
        else
        {
            isStasis = true;
            isAirKicking = false;
        }

        switch (var)
        {
            case 1: // Chute
                animator.SetTrigger("Kick");
                break;
            case 2: // Soco
                animator.SetTrigger("Punch");
                break;
            case 3: // Bola de fogo
                animator.SetTrigger("Power");
                break;
        }
    }

    void ActiveCollider()
    {
        foreach (var col in collider)
            col.enabled = true;
    }

    void DesactiveCollider()
    {
        foreach (var col in collider)
            col.enabled = false;
    }

    void FireActive()
    {
        GameObject ball = Instantiate(BOLADEFOGO, spawnTarget.position, Quaternion.identity);

        float ballSpeed = 4.0f;
        Vector3 direction = transform.forward;
        direction.y = 0;
        direction.Normalize();

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * ballSpeed;
        }

        var proj = ball.GetComponent<ProjectileDamage>();
        if (proj != null)
        {
            proj.SetDamage(playerCharacter.AttackPower);
        }
    }

    public void EndAttack()
    {
        isStasis = false;
        isAirKicking = false;
    }
}