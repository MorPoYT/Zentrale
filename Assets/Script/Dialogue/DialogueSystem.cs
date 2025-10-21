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
    [Range(0, 1)] public int audioTrack = 0;
    [Tooltip("Время показа этой реплики (секунды). Только для катсценных диалогов")]
    public float displayDuration = 3f;
}

public class DialogueSystem : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public AudioSource npcAudioSource;
    public AudioSource playerAudioSource;
    public GameObject dialog_obj;

    public List<DialogueLine> dialogueLines;
    public float typingSpeed = 0.05f;

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
        if (interactAction != null)
        {
            interactAction.performed -= OnInteractPerformed;
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!dialog_obj.activeSelf) return;
        
        if (isTyping && isCutsceneDialogue == false)
        {
            SkipTyping();
        }
        else if(!isTyping && isCutsceneDialogue == false)
        {
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
            if(npcAudioSource != null) {
                npcAudioSource.Stop();
            }
            if(playerAudioSource != null) {
                playerAudioSource.Stop();
            }
            
            if (line.audioTrack == 0)
                npcAudioSource.PlayOneShot(line.audioClip);
            else
                playerAudioSource.PlayOneShot(line.audioClip);
        }
        
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
        
        OnDialogueEnd?.Invoke();
    }

    void EndHideHint() {
        InteractableSystem.SetInteractionAllowed(true);
    }
}