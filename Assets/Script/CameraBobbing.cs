using UnityEngine;
using UnityEngine.InputSystem;

public class CameraBobbing : MonoBehaviour
{
    [Header("Bobbing Settings")]
    [SerializeField] private float walkingBobbingSpeed = 14f;
    [SerializeField] private float runningBobbingSpeed = 20f;
    [SerializeField] private float walkingBobbingAmount = 0.05f;
    [SerializeField] private float runningBobbingAmount = 0.1f;
    [SerializeField] private float midpointOffset = 0f;
    
    [Header("Tilt Settings")]
    [SerializeField] private float runningTiltAmount = 5f;
    [SerializeField] private float tiltSmoothness = 5f;
    [SerializeField] private float tiltReturnSpeed = 8f;
    
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private InputActionReference movementInput;
    [SerializeField] private InputActionReference runInput;
    
    private float timer = 0;
    private float defaultPosY;
    private Quaternion defaultRotation;
    private float currentTilt;
    private Vector2 inputVector;

    void Start()
    {
        if (cameraHolder == null) cameraHolder = transform;
        defaultPosY = cameraHolder.localPosition.y;
        defaultRotation = cameraHolder.localRotation;
        
        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>();
            
        // Initialize input actions
        movementInput.action.Enable();
        runInput.action.Enable();
        
        // Subscribe to input callbacks
        movementInput.action.performed += OnMovement;
        movementInput.action.canceled += OnMovement;
    }

    void OnDestroy()
    {
        // Clean up input callbacks
        movementInput.action.performed -= OnMovement;
        movementInput.action.canceled -= OnMovement;
    }

    private void OnMovement(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>();
    }

    void Update()
    {
        bool isRunning = runInput.action.IsPressed() && playerController.isRunning;
        
        if (characterController.isGrounded && HasMovementInput())
        {
            ApplyBobbingEffect(isRunning);
            if (isRunning)
            {
                ApplyTiltEffect();
            }
            else
            {
                ResetTiltEffect();
            }
        }
        else
        {
            ResetCameraPosition();
            ResetTiltEffect();
        }
    }

    bool HasMovementInput()
    {
        return inputVector.magnitude > 0.1f;
    }

    void ApplyBobbingEffect(bool isRunning)
    {
        float speed = isRunning ? runningBobbingSpeed : walkingBobbingSpeed;
        float amount = isRunning ? runningBobbingAmount : walkingBobbingAmount;

        timer += Time.deltaTime * speed;
        float waveSlice = Mathf.Sin(timer);
        float translateChange = waveSlice * amount;
        
        float inputStrength = Mathf.Clamp(inputVector.magnitude, 0f, 1f);

        Vector3 localPos = cameraHolder.localPosition;
        localPos.y = defaultPosY + translateChange * inputStrength + midpointOffset;
        cameraHolder.localPosition = localPos;
    }

    void ApplyTiltEffect()
    {
        // Tilt based on horizontal movement
        float targetTilt = inputVector.x * runningTiltAmount;
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSmoothness * Time.deltaTime);
        
        cameraHolder.localRotation = defaultRotation * 
                                   Quaternion.Euler(
                                       Mathf.Sin(timer * 0.5f) * 2f,
                                       currentTilt * 0.5f, 
                                       currentTilt);
    }

    void ResetCameraPosition()
    {
        timer = 0;
        Vector3 localPos = cameraHolder.localPosition;
        localPos.y = Mathf.Lerp(localPos.y, defaultPosY + midpointOffset, Time.deltaTime * walkingBobbingSpeed);
        cameraHolder.localPosition = localPos;
    }

    void ResetTiltEffect()
    {
        currentTilt = Mathf.Lerp(currentTilt, 0f, tiltReturnSpeed * Time.deltaTime);
        cameraHolder.localRotation = Quaternion.Lerp(
            cameraHolder.localRotation, 
            defaultRotation, 
            tiltReturnSpeed * Time.deltaTime);
    }
}