using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OS_Folder : MonoBehaviour
{
    [SerializeField] private GameObject panelToOpen; // Ссылка на GameObject, который нужно открыть
    [SerializeField] private float doubleClickTime = 0.3f; // Время между кликами для считывания двойного нажатия

    private Button button;
    private int clickCount = 0;
    private float lastClickTime = 0f;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        clickCount++;

        if (clickCount == 1)
        {
            lastClickTime = Time.time;
            StartCoroutine(WaitForDoubleClick());
        }
        else if (clickCount > 1 && Time.time - lastClickTime <= doubleClickTime)
        {
            // Двойное нажатие зарегистрировано
            clickCount = 0;
            lastClickTime = 0f;
            OpenPanel();
        }
    }

    private IEnumerator WaitForDoubleClick()
    {
        yield return new WaitForSeconds(doubleClickTime);
        
        if (clickCount == 1)
        {
            // Был только один клик
            clickCount = 0;
            lastClickTime = 0f;
        }
    }

    private void OpenPanel()
    {
        if (panelToOpen != null)
        {
            // Активируем панель
            panelToOpen.SetActive(true);
            
            // Получаем Canvas
            Canvas canvas = panelToOpen.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                // Устанавливаем панель в центр Canvas
                RectTransform panelRect = panelToOpen.GetComponent<RectTransform>();
                if (panelRect != null)
                {
                    panelRect.anchoredPosition = Vector2.zero;
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }
}
