using UnityEngine;
using System.Collections;

public class LightSwitch : MonoBehaviour
{
    [SerializeField] private Light[] lightsToToggle;       // Массив источников света
    [SerializeField] private float flipDuration = 0.3f;     // Длительность поворота
    [SerializeField] private float lockDelay = 0.1f;        // Задержка блокировки

    private bool isOn = true;                               // Текущее состояние
    private bool isLocked = false;                          // Блокировка повторного вызова

    public void ToggleLights()
    {
        if (isLocked)
            return;

        isLocked = true;

        // Переключаем состояние
        isOn = !isOn;

        foreach (var light in lightsToToggle)
        {
            if (light != null)
                light.enabled = isOn;
        }

        // Запускаем анимацию поворота
        StartCoroutine(FlipSwitch());

        // Разблокируем через задержку
        StartCoroutine(UnlockAfterDelay());
    }

    private IEnumerator FlipSwitch()
    {
        Quaternion startRotation = transform.localRotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 0, 180f);

        float elapsed = 0f;

        while (elapsed < flipDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flipDuration);
            transform.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }

        // Точное позиционирование
        transform.localRotation = endRotation;
    }

    private IEnumerator UnlockAfterDelay()
    {
        yield return new WaitForSeconds(lockDelay);
        isLocked = false;
    }
}