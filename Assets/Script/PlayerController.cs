using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
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

    [Header("Анимация рук")]
    [SerializeField] private Animator animation_PlayerHands;

    // Приватные переменные
    private Vector2 moveInput;
    private Vector2 lookInput;
    public bool isRunning;
    private bool isCrouching;
    private bool isJumping;
    private float verticalVelocity;
    private float currentHeight;
    private Vector3 originalCameraPosition;
    private float xRotation;
    private float yRotation;

    private void Awake()
    {
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
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMovement();
        HandleJumpAndGravity();
        HandleCrouching();
        HandleCameraRotation();
        HandleAnimation();
    }

    private void HandleAnimation() {
    	if(isRunning) {
    		animation_PlayerHands.SetBool("IsRunning", true);
    	} else {
    		animation_PlayerHands.SetBool("IsRunning", false);
    	}
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
        if (characterController.isGrounded)
        {
            verticalVelocity = -1f; // Небольшая сила прижатия к земле
            
            if (isJumping)
            {
                verticalVelocity = jumpForce;
                isJumping = false;
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

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
        if (lookInput == Vector2.zero) return;

        yRotation += lookInput.x * mouseXSensitivity;
        xRotation -= lookInput.y * mouseYSensitivity;
        xRotation = Mathf.Clamp(xRotation, maxLookDownAngle, maxLookUpAngle);

        // Поворот всего тела по горизонтали
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
        
        // Наклон камеры по вертикали
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
	    if (context.performed)
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


    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && characterController.isGrounded && !isCrouching)
        {
            isJumping = true;
        }
    }
}