using _2._Scripts.Core.Domain.Character.Player;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    public float sprintAdittion = 3.5f;
    public float jumpForce = 18f;
    public float jumpTime = 0.85f;
    public float gravity = 9.8f;
    public float airKickMomentumMultiplier = 1.2f;

    [Header("Audios")]
    public AudioSource punchSound;

    float jumpElapsedTime = 0;

    bool isStasis = false;
    bool isJumping = false;
    bool isSprinting = false;
    bool isCrouching = false;
    bool isAirKicking = false;

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

    private int currentAttackType = 0;

    Animator animator;
    public CharacterController cc;
    [SerializeField]
    private PlayerCharacter playerCharacter;
    [SerializeField]
    private DialogueManager dialogueManager;

    // --- FORÇA EXTERNA (vento/campo magnético) ---
    private Vector3 externalForce = Vector3.zero;
    [SerializeField] private float externalForceDecay = 2f; // quanto mais alto, mais rápido a força some

    

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
        bool dialogueActive = dialogueManager != null && dialogueManager.IsDialogueActive();

        // Se o diálogo estiver ativo ou qualquer UI (inventário/loja), bloqueia entrada
        if (dialogueActive || CameraController.isInventoryOpen)
        {
            inputHorizontal = 0;
            inputVertical = 0;
            inputJump = false;
            inputSprint = false;
            inputCrouch = false;
            return;
        }

        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");
        inputJump = Input.GetAxis("Jump") == 1f;
        inputSprint = Input.GetAxis("Fire3") == 1f;
        inputCrouch = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.JoystickButton1);

        if (inputHorizontal != 0)
            lastHorizontalInput = inputHorizontal;
        if (inputVertical != 0)
            lastVerticalInput = inputVertical;

        bool shouldApplyStasis = isStasis && !(isAirKicking || (currentAttackType == 1 && !cc.isGrounded));

        if (shouldApplyStasis)
        {
            inputHorizontal = 0;
            inputVertical = 0;
        }

        if (!shouldApplyStasis)
        {
            if (inputCrouch)
                isCrouching = !isCrouching;
        }

        if (cc.isGrounded && animator != null)
        {
            animator.SetBool("crouch", isCrouching);

            float minimumSpeed = 0.9f;
            bool isMoving = cc.velocity.magnitude > minimumSpeed;

            animator.SetBool("run", isMoving);
            animator.SetBool("sprint", isSprinting);
        }

        if (animator != null)
        {
            animator.SetBool("air", !cc.isGrounded);
            float horizontalSpeed = new Vector3(cc.velocity.x, 0, cc.velocity.z).magnitude;
            animator.SetFloat("Speed", horizontalSpeed);
        }

        if (inputJump && cc.isGrounded && !shouldApplyStasis)
        {
            isJumping = true;
        }

        HeadHittingDetect();

        // ✅ BLOQUEIO DE ATAQUE CASO ALGUMA UI ESTEJA ABERTA
        if (CameraController.isInventoryOpen)
            return;

        if (Input.GetMouseButtonDown(0) && !isCrouching)
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
        bool dialogueActive = dialogueManager != null && dialogueManager.IsDialogueActive();
        if (dialogueActive) return;

        float baseSpeed = playerCharacter.Speed;
        float velocityAdittion = 0;

        // Sprint
        isSprinting = inputSprint && cc.velocity.magnitude > 0.9f && playerCharacter.currentEnergy > 0;
        if (isSprinting) velocityAdittion = sprintAdittion;
        if (isCrouching) velocityAdittion = -(baseSpeed * 0.50f);

        // Ajuste de input para air kick
        float effectiveHorizontal = inputHorizontal;
        float effectiveVertical = inputVertical;
        if (isAirKicking)
        {
            effectiveHorizontal = lastHorizontalInput * airKickMomentumMultiplier;
            effectiveVertical = lastVerticalInput * airKickMomentumMultiplier;
        }

        // Direção horizontal
        float directionX = effectiveHorizontal * (baseSpeed + velocityAdittion) * Time.deltaTime;
        float directionZ = effectiveVertical * (baseSpeed + velocityAdittion) * Time.deltaTime;

        // Direção vertical (jump + gravidade)
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

        // Direção baseada na câmera
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        forward *= directionZ;
        right *= directionX;

        // Rotação do player
        if (directionX != 0 || directionZ != 0)
        {
            float angle = Mathf.Atan2(forward.x + right.x, forward.z + right.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.15f);
        }

        // Movimento final
        Vector3 verticalDirection = Vector3.up * directionY;
        Vector3 horizontalDirection = forward + right;

        // --- SOMAR FORÇA EXTERNA ---
        horizontalDirection += externalForce;

        Vector3 moviment = verticalDirection + horizontalDirection;

        cc.Move(moviment);

        // --- REDUZIR FORÇA EXTERNA GRADUALMENTE ---
        externalForce = Vector3.Lerp(externalForce, Vector3.zero, Time.deltaTime * externalForceDecay);

        // Energia
        if (isSprinting && cc.isGrounded && playerCharacter.currentEnergy > 0)
        {
            float energyDrainPerSecond = playerCharacter.energyReductionRate;
            playerCharacter.ReduceEnergy(energyDrainPerSecond * Time.deltaTime);
        }
        if (!isSprinting && playerCharacter.currentEnergy < playerCharacter.maxEnergy)
        {
            float energyRecoveryPerSecond = playerCharacter.energyRecoveryRate;
            playerCharacter.RecoverEnergy(energyRecoveryPerSecond * Time.deltaTime);
        }
        if (playerCharacter.currentEnergy <= 0) isSprinting = false;
    }
    public void AddExternalForce(Vector3 force)
    {
        externalForce += force;
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
        if (playerCharacter.currentEnergy <= 0)
            return;

        if (isStasis && !cc.isGrounded)
            return;

        currentAttackType = var;

        if (var == 1 && !cc.isGrounded)
        {
            isAirKicking = true;
        }
        else
        {
            isStasis = true;
            isAirKicking = false;
        }

        switch (var)
        {
            case 1:
                animator.SetTrigger("Kick");
                playerCharacter.ReduceEnergy(playerCharacter.energyReductionRate * 2);
                break;
            case 2:
                animator.SetTrigger("Punch");
                if (punchSound != null)
                    punchSound.Play();
                playerCharacter.ReduceEnergy(playerCharacter.energyReductionRate);
                break;
            case 3:
                animator.SetTrigger("Power");
                playerCharacter.ReduceEnergy(playerCharacter.energyReductionRate * 4);
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

    public bool IsSprinting()
    {
        return isSprinting;
    }

    public void EndAttack()
    {
        isStasis = false;
        isAirKicking = false;
    }
}
