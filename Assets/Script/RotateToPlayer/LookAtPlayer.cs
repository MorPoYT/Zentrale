using UnityEngine;
using TMPro;

public class LookAtPlayer : MonoBehaviour
{
    private Camera _mainCamera;
    private TMP_Text _textMeshPro;

    private void Awake()
    {
        _textMeshPro = GetComponent<TMP_Text>();
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (_mainCamera != null)
        {
            // Поворачиваем текст лицом к камере
            transform.LookAt(transform.position + _mainCamera.transform.rotation * Vector3.forward,
                            _mainCamera.transform.rotation * Vector3.up);
            
            // Альтернативный вариант - текст всегда полностью перпендикулярен направлению к камере
            // transform.LookAt(_mainCamera.transform);
            // transform.Rotate(0, 180, 0); // Разворот на 180 градусов, если текст отображается задом наперед
        }
    }
}