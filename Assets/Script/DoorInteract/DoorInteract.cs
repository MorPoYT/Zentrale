using UnityEngine;
using UnityEngine.InputSystem;

public class DoorInteract : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z }

    [Header("Input")]
    public InputActionReference interactAction;
    public InputActionReference lookAction;

    [Header("Door Settings")]
    public Transform doorPivot;
    public RotationAxis rotationAxis = RotationAxis.Z;
    public float maxOpenAngle = 90f; // +90 для левой двери, -90 для правой
    public float mouseSensitivity = 0.5f;
    public float rotationSmoothing = 5f;
    public bool autoInvertDirection = true;
    public bool isLeftToRight = false; // Направление открывания: false - справа налево, true - слева направо

    [Header("Direction")]
    public bool canOpenInward = true;
    public bool canOpenOutward = true;

    private bool isRotating;
    private float targetAngle;
    private float currentAngle;
    private float mouseDeltaSum;
    private Vector3 initialRotation;
    private Transform playerTransform;
    private bool shouldInvertDirection;

    private void Start()
    {
        currentAngle = 0f;
        targetAngle = 0f;
        initialRotation = doorPivot.localRotation.eulerAngles;
        playerTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        interactAction.action.Enable();
        lookAction.action.Enable();
        interactAction.action.started += StartRotate;
        interactAction.action.canceled += StopRotate;
    }

    private void OnDisable()
    {
        interactAction.action.Disable();
        lookAction.action.Disable();
        interactAction.action.started -= StartRotate;
        interactAction.action.canceled -= StopRotate;
    }

    private void StartRotate(InputAction.CallbackContext ctx)
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, 3f) && hit.transform.IsChildOf(doorPivot))
        {
            isRotating = true;
            mouseDeltaSum = 0f;

            if (autoInvertDirection)
            {
                Vector3 toPlayer = playerTransform.position - doorPivot.position;
                Vector3 doorNormal = doorPivot.TransformDirection(Vector3.forward);
                shouldInvertDirection = (Vector3.Dot(toPlayer, doorNormal) < 0);
            }
        }
    }

    private void StopRotate(InputAction.CallbackContext ctx)
    {
        isRotating = false;
    }

    private void Update()
    {
        if (isRotating)
        {
            float mouseX = lookAction.action.ReadValue<Vector2>().x;
            float mouseDelta = mouseX * mouseSensitivity;
            
            // Применяем направление открывания в зависимости от isLeftToRight
            if (isLeftToRight)
            {
                // Слева направо: мышь вправо → дверь вправо
                mouseDelta *= -1f;
            }
            else
            {
                // Справа налево: мышь влево → дверь вправо
                mouseDelta *= 1f;
            }

            mouseDeltaSum += mouseDelta;

            float anglePercentage = Mathf.Clamp(mouseDeltaSum / 1000f, -1f, 1f);
            targetAngle = anglePercentage * Mathf.Abs(maxOpenAngle);

            // Применяем инверсию, если игрок с "обратной" стороны
            if (shouldInvertDirection)
                targetAngle *= -1;

            // Ограничения
            if (!canOpenInward) 
                targetAngle = Mathf.Clamp(targetAngle, 0, Mathf.Abs(maxOpenAngle));
            if (!canOpenOutward) 
                targetAngle = Mathf.Clamp(targetAngle, -Mathf.Abs(maxOpenAngle), 0);

            // Восстанавливаем знак для правой двери
            targetAngle *= Mathf.Sign(maxOpenAngle);
        }

        currentAngle = Mathf.Lerp(currentAngle, targetAngle, rotationSmoothing * Time.deltaTime);
        ApplyRotation();
    }

    private void ApplyRotation()
    {
        Vector3 newRotation = initialRotation;
        
        switch (rotationAxis)
        {
            case RotationAxis.X:
                newRotation.x += currentAngle;
                break;
            case RotationAxis.Y:
                newRotation.y += currentAngle;
                break;
            case RotationAxis.Z:
                newRotation.z += currentAngle;
                break;
        }

        doorPivot.localRotation = Quaternion.Euler(newRotation);
    }

    public void ResetDoor()
    {
        targetAngle = 0f;
        currentAngle = 0f;
        mouseDeltaSum = 0f;
        doorPivot.localRotation = Quaternion.Euler(initialRotation);
    }
}