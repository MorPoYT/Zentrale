using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(3, 10)] 
    public string dialogueText;
    public AudioClip audioClip;
    [Tooltip("0 - NPC, 1 - ГГ")]
    [Range(0, 1)] public int audioTrack = 0; // По умолчанию NPC
    [Tooltip("Время показа этой реплики (секунды). Только для катсценных диалогов")]
    public float displayDuration = 3f;
}

public class DialogueSystem : MonoBehaviour
{
    public TMP_Text nameText; // UI-элемент для имени
    public TMP_Text dialogueText; // UI-элемент для текста
    public AudioSource npcAudioSource; // Аудиодорожка для NPC
    public AudioSource playerAudioSource; // Аудиодорожка для игрока
    public GameObject dialog_obj;

    public List<DialogueLine> dialogueLines; // Список реплик
    public float typingSpeed = 0.05f; // Скорость печатания текста

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    [Tooltip("Если true, диалог будет автоматически переключаться между репликами по времени // НЕ ТРОГАТЬ ЗАСКРИПТОВАНО")]
    public bool isCutsceneDialogue = false;

    private Coroutine autoAdvanceCoroutine;

    // Input System
    private PlayerInput playerInput;
    private InputAction interactAction;

    public delegate void DialogueEndHandler();
    public event DialogueEndHandler OnDialogueEnd;

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
            // Останавливаем все дорожки
            if(npcAudioSource != null) {
                npcAudioSource.Stop();
            }
            if(playerAudioSource != null) {
                playerAudioSource.Stop();
            }
            
            // Воспроизводим на выбранной дорожке
            if (line.audioTrack == 0)
                npcAudioSource.PlayOneShot(line.audioClip);
            else
                playerAudioSource.PlayOneShot(line.audioClip);
        }
        
        // Для катсценного диалога запускаем автоматический переход
        if (isCutsceneDialogue)
        {
            if (autoAdvanceCoroutine != null)
            {
                StopCoroutine(autoAdvanceCoroutine);
            }
            autoAdvanceCoroutine = StartCoroutine(AutoAdvance(line.displayDuration));
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

    IEnumerator AutoAdvance(float delay)
    {
        yield return new WaitForSeconds(delay);
        NextLine();
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
        // Останавливаем авто-переход, если он был
        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }
        
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
        if(npcAudioSource != null) {
            npcAudioSource.Stop();
        }
        if(playerAudioSource != null) {
            playerAudioSource.Stop();
        }
        Debug.Log("Диалог завершён.");
        Invoke("EndHideHint", 1f);
        
        // Вызываем событие завершения диалога
        OnDialogueEnd?.Invoke();
    }

    void EndHideHint() {
        InteractableSystem.SetInteractionAllowed(true);
    }
}