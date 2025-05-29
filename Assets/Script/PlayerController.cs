using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    [Header("Компоненты")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera playerCamera;

    [Header("Настройки движения")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = -25f;
    
    [Header("Настройки приседания")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchSpeed = 10f;
    [SerializeField] private float cameraCrouchYOffset = -0.5f;
    [SerializeField] private LayerMask obstacleMask;
    
    [Header("Настройки камеры")]
    [SerializeField] private float mouseXSensitivity = 2f;
    [SerializeField] private float mouseYSensitivity = 2f;
    [SerializeField] private float maxLookUpAngle = 80f;
    [SerializeField] private float maxLookDownAngle = -80f;

    [Header("Настройки плавного прыжка")]
    [SerializeField] private float jumpHeight = 1.5f;  // Высота прыжка (в метрах)
    [SerializeField] private float jumpApexTime = 0.4f; // Время до вершины прыжка
    [SerializeField] private AnimationCurve jumpCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 1f),
        new Keyframe(1f, 0f)
    ); // Кривая плавности

    private float jumpProgress; // Таймер прыжка (0..1)
    private bool isJumping; // В процессе прыжка

    [Header("Анимация рук")]
    [SerializeField] private Animator AnimationPlayer;
    public bool isOpenMenu;

    // Приватные переменные
    private Vector2 moveInput;
    private Vector2 lookInput;
    public bool isRunning;
    private bool isCrouching;
    private float verticalVelocity;
    private float currentHeight;
    private Vector3 originalCameraPosition;
    private float xRotation;
    private float yRotation;

    //ОЧЕНЬ ВАЖНО [*****]
    [HideInInspector]
    [SerializeField] public bool isControllUnenabled;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            return;
        }
        // Инициализация компонентов
        characterController = GetComponent<CharacterController>();
        playerCamera = cameraPivot.GetComponentInChildren<Camera>();
        
        // Начальные значения
        currentHeight = standHeight;
        characterController.height = standHeight;
        
        if (playerCamera != null)
        {
            originalCameraPosition = playerCamera.transform.localPosition;
        }
        
         // Cursor.lockState = CursorLockMode.Locked;
         // Cursor.visible = false;
    }

    public void CloseAllAnimation() {
        AnimationPlayer.Rebind();
        AnimationPlayer.Update(0f);
    }

    private void Update()
    {
        HandleMovement();
        HandleJumpAndGravity();
        HandleCrouching();
        HandleCameraRotation();
    }

    private bool IsGrounded()
    {
        float rayLength = 2f; // Проверяем чуть ниже ног
        Vector3 rayStart = transform.position + Vector3.up * 0.1f; // Старт луча чуть выше ступней
        bool hitGround = Physics.Raycast(rayStart, Vector3.down, rayLength, obstacleMask);
        return hitGround || characterController.isGrounded; // Двойная проверка
    }

    private void HandleMovement()
    {
        // Движение относительно взгляда камеры
        Vector3 moveDirection = transform.TransformDirection(
            new Vector3(moveInput.x, 0, moveInput.y)
        );
        
        float speed = isRunning ? runSpeed : walkSpeed;
        characterController.Move(moveDirection * speed * Time.deltaTime);
    }

    private void HandleJumpAndGravity()
    {
        // Прыжок
        if (isJumping)
        {
            jumpProgress += Time.deltaTime / jumpApexTime;
            if (jumpProgress >= 1f)
            {
                isJumping = false;
                jumpProgress = 0f;
            }

            // Расчёт высоты по кривой
            float jumpPower = jumpCurve.Evaluate(jumpProgress) * jumpHeight;
            verticalVelocity = jumpPower / Time.deltaTime; // Компенсация Move()
        }
        // Гравитация (только если не в прыжке)
        else if (!characterController.isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        // Прижатие к земле
        else
        {
            verticalVelocity = -0.1f;
        }

        // Применяем движение
        characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    private void HandleCrouching()
	{
	    // Плавное изменение высоты
	    float targetHeight = isCrouching ? crouchHeight : standHeight;
	    currentHeight = Mathf.Lerp(currentHeight, targetHeight, crouchSpeed * Time.deltaTime);
	    
	    // Сохраняем положение "ног" персонажа
	    float lastHeight = characterController.height;
	    characterController.height = currentHeight;
	    
	    // Корректируем позицию при изменении высоты
	    if (Mathf.Abs(currentHeight - lastHeight) > 0.01f)
	    {
	        float heightDifference = (currentHeight - lastHeight) / 2;
	        transform.position += Vector3.up * heightDifference;
	    }
	    
	    // Двигаем камеру
	    if (playerCamera != null)
	    {
	        Vector3 targetCameraPos = originalCameraPosition + 
	            (isCrouching ? Vector3.up * cameraCrouchYOffset : Vector3.zero);
	            
	        playerCamera.transform.localPosition = Vector3.Lerp(
	            playerCamera.transform.localPosition,
	            targetCameraPos,
	            crouchSpeed * Time.deltaTime
	        );
	    }
	}

    private bool CanStandUp()
	{
	    float rayLength = standHeight - crouchHeight;
	    Vector3 rayStart = transform.position + Vector3.up * crouchHeight;
	    bool hitSomething = Physics.Raycast(rayStart, Vector3.up, rayLength, obstacleMask);
	    
	    // Возвращаем true если НЕ наткнулись на препятствие
	    return !hitSomething;
	}

    private void HandleCameraRotation()
    {
        if (lookInput == Vector2.zero || isControllUnenabled) return;

        // Горизонтальное вращение (влево/вправо)
        yRotation += lookInput.x * mouseXSensitivity;
        
        // Вертикальное вращение (вверх/вниз)
        xRotation -= lookInput.y * mouseYSensitivity;
        xRotation = Mathf.Clamp(xRotation, maxLookDownAngle, maxLookUpAngle);

        // Поворачиваем весь объект игрока только по горизонтали
        transform.rotation = Quaternion.Euler(0, yRotation, 0);

        // Наклоняем только камеру по вертикали
        if (cameraPivot != null)
        {
            cameraPivot.localRotation = Quaternion.Euler(xRotation, 0, 0);
        }
    }


    // Input System обработчики
    public void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext context) => lookInput = context.ReadValue<Vector2>();
    public void OnRun(InputAction.CallbackContext context) => isRunning = context.performed;
    
    public void OnCrouch(InputAction.CallbackContext context)
	{
	    if (context.performed  && isControllUnenabled == false)
	    {
	        // Переключаем состояние приседания
	        isCrouching = !isCrouching;
	        
	        // Если пытаемся встать - проверяем, есть ли место
	        if (!isCrouching && !CanStandUp())
	        {
	            // Если места нет - остаемся в приседе
	            isCrouching = true;
	        }
	    }
	}

    public void OnMenu(InputAction.CallbackContext context)
    {
        if (context.performed && isControllUnenabled == false)
        {
            isOpenMenu = !isOpenMenu;

            if(AnimationPlayer != null) {
                AnimationPlayer.SetBool("openNotebook", isOpenMenu);
            }
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && IsGrounded() && !isCrouching && isControllUnenabled == false)
        {
            isJumping = true;
            jumpProgress = 0f;
        }
    }
}