using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableSystem : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float _interactionRange = 2f;
    [SerializeField] private LayerMask _interactableLayer;
    [SerializeField] private float _rayRadius = 0.1f;

    [Header("UI")]
    [SerializeField] private GameObject _hintObject;

    private Camera _playerCamera;
    private ObjectUnityEvent _currentItem;
    private bool _isLookingAtDoor; // Новое поле для проверки двери
    private bool _isInteractionAllowed = true;

    public static bool IsInteractionAllowed => _instance._isInteractionAllowed;
    private static InteractableSystem _instance;

    private void Awake() => _instance = this;

    private void Start()
    {
        _playerCamera = Camera.main;
        UpdateHintVisibility();
    }

    public void OnInteracted(InputAction.CallbackContext context)
    {
        if (_isInteractionAllowed)
        {
            TryUseItem();
        }
    }

    private void Update() => CheckNearbyItems();

    private void CheckNearbyItems()
    {
        _currentItem = null;
        _isLookingAtDoor = false; // Сбрасываем флаг двери

        if (!_isInteractionAllowed) 
        {
            UpdateHintVisibility();
            return;
        }

        Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.SphereCast(ray, _rayRadius, out hit, _interactionRange, _interactableLayer))
        {
            // Проверяем обычные предметы
            if (hit.collider.TryGetComponent(out ObjectUnityEvent item))
            {
                _currentItem = item;
            }
            // Дополнительная проверка на дверь (без вызова методов)
            else // Просто проверяем наличие компонента
            {
                _isLookingAtDoor = true; // Ставим флаг, что смотрим на дверь
            }
        }

        UpdateHintVisibility();
    }

    private void UpdateHintVisibility()
    {
        if (_hintObject != null)
        {
            // Показываем подсказку если смотрим на предмет ИЛИ на дверь
            _hintObject.SetActive((_currentItem != null || _isLookingAtDoor) && _isInteractionAllowed);
        }
    }

    private void TryUseItem()
    {
        if (_currentItem != null && _isInteractionAllowed)
        {
            _currentItem.Use();
        }
        // На двери просто показываем подсказку, взаимодействие через другой скрипт
    }

    public static void SetInteractionAllowed(bool allowed)
    {
        _instance._isInteractionAllowed = allowed;
        _instance.UpdateHintVisibility();
    }

    private void OnDrawGizmosSelected()
    {
        if (_playerCamera == null)
            _playerCamera = Camera.main;

        Gizmos.color = Color.cyan;
        Vector3 rayStart = _playerCamera.transform.position;
        Vector3 rayEnd = rayStart + _playerCamera.transform.forward * _interactionRange;
        Gizmos.DrawLine(rayStart, rayEnd);
        Gizmos.DrawWireSphere(rayEnd, _rayRadius);
    }
}