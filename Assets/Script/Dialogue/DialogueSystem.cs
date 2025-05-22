using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[System.Serializable]
public class DialogueLine
{
    public string speakerName; // Имя говорящего
    [TextArea(3, 10)] 
    public string dialogueText; // Текст реплики
    public AudioClip audioClip; // Аудио для этой реплики
}

public class DialogueSystem : MonoBehaviour
{
    public TMP_Text nameText; // UI-элемент для имени
    public TMP_Text dialogueText; // UI-элемент для текста
    public AudioSource audioSource; // Источник звука
    public GameObject dialog_obj;

    public List<DialogueLine> dialogueLines; // Список реплик
    public float typingSpeed = 0.05f; // Скорость печатания текста

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    
    // Input System
    private PlayerInput playerInput;
    private InputAction interactAction;

    private void Awake()
    {
        // Инициализация Input System
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            playerInput = gameObject.AddComponent<PlayerInput>();
        }
        
        // Настройка действия для взаимодействия
        var actions = new InputActionMap();
        interactAction = actions.AddAction("Interact", binding: "<Keyboard>/e");
        interactAction.performed += OnInteractPerformed;
        
        // Активируем действия
        actions.Enable();
    }

    private void OnDestroy()
    {
        // Отписываемся от событий при уничтожении объекта
        if (interactAction != null)
        {
            interactAction.performed -= OnInteractPerformed;
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!dialog_obj.activeSelf) return;
        
        if (isTyping)
        {
            // Пропустить анимацию печатания
            SkipTyping();
        }
        else
        {
            // Перейти к следующей реплике
            NextLine();
        }
    }

    public void ActivateDialog() 
    {
        if (dialogueLines.Count > 0)
        {
            dialog_obj.SetActive(true);
            StartDialogue();
        }
    }

    public void StartDialogue()
    {
        currentLineIndex = 0;
        DisplayLine(dialogueLines[currentLineIndex]);
    }

    void DisplayLine(DialogueLine line)
    {
        nameText.text = line.speakerName;
        
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(line.dialogueText));

        if (line.audioClip != null)
        {
            audioSource.Stop();
            audioSource.clip = line.audioClip;
            audioSource.Play();
        }
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";
        
        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        isTyping = false;
    }

    void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        dialogueText.text = dialogueLines[currentLineIndex].dialogueText;
        isTyping = false;
    }

    void NextLine()
    {
        currentLineIndex++;
        
        if (currentLineIndex < dialogueLines.Count)
        {
            DisplayLine(dialogueLines[currentLineIndex]);
        }
        else
        {
            // Диалог закончен
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        dialog_obj.SetActive(false);
        nameText.text = "";
        dialogueText.text = "";
        audioSource.Stop();
        Debug.Log("Диалог завершён.");
    }
}