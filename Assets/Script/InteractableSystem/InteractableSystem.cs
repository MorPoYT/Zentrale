using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class InteractableSystem : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float _interactionRange = 2f;
    [SerializeField] private LayerMask _interactableLayer;
    [SerializeField] private float _rayRadius = 0.1f; // Толщина луча (для SphereCast)

    [Header("UI")]
    [SerializeField] private TMP_Text _hintText;

    private Camera _playerCamera; // Камера игрока (для направления луча)
    private ObjectUnityEvent _currentItem;

    private void Start()
    {
        // Получаем камеру игрока автоматически (если скрипт на персонаже)
        _playerCamera = Camera.main;
    }

    // Вызывается через Input System
    public void OnInteracted(InputAction.CallbackContext context)
    {
        TryUseItem();
    }

    private void Update()
    {
        CheckNearbyItems();
    }

    private void CheckNearbyItems()
    {
        // Сбрасываем текущий предмет
        _currentItem = null;

        // Создаем луч из камеры вперед
        Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
        RaycastHit hit;

        // Вариант 1: Обычный Raycast (тонкий луч)
        // if (Physics.Raycast(ray, out hit, _interactionRange, _interactableLayer))

        // Вариант 2: SphereCast (луч с толщиной, чтобы избежать "промахов")
        if (Physics.SphereCast(ray, _rayRadius, out hit, _interactionRange, _interactableLayer))
        {
            if (hit.collider.TryGetComponent(out ObjectUnityEvent item))
            {
                _currentItem = item;
            }
        }

        UpdateHintText();
    }

    private void UpdateHintText()
    {
        if (_hintText != null)
        {
            _hintText.text = _currentItem != null ? _currentItem.InteractionHint : "";
        }
    }

    private void TryUseItem()
    {
        if (_currentItem != null)
        {
            _currentItem.Use();
        }
    }

    // Визуализация луча в редакторе
    private void OnDrawGizmosSelected()
    {
        if (_playerCamera == null)
            _playerCamera = Camera.main;

        if (_playerCamera != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 rayStart = _playerCamera.transform.position;
            Vector3 rayEnd = rayStart + _playerCamera.transform.forward * _interactionRange;
            Gizmos.DrawLine(rayStart, rayEnd);

            // Для SphereCast рисуем "колпак" на конце луча
            Gizmos.DrawWireSphere(rayEnd, _rayRadius);
        }
    }
}