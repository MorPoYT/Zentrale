using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class DialogueInitial : MonoBehaviour
{
    [SerializeField] private DialogueSystem dialogueSystem;
    public List<DialogueLine> newDialogueLines;

    [Header("Dialogue Features")]
    [Tooltip("Если true, диалог сработает только один раз")]
    public bool isDisposable = false;
    private bool hasBeenTriggered = false;

    [Tooltip("Если true, диалог будет автоматически переключаться между репликами по времени")]
    public bool isCutsceneDialogue = false;

    [Tooltip("Если true, катсцена с диалогом проиграется только один раз")]
    public bool playCutsceneOnlyOnce = false;
    private bool hasCutsceneBeenPlayed = false;

    [Header("Audio Sources Settings")]
    [Tooltip("Если true, автоматически найдет AudioSource игрока по тегу 'Player'")]
    public bool autoFindPlayerAudio = true;

    [Tooltip("Если true, автоматически найдет AudioSource NPC через GetComponent")]
    public bool isAudioSourceOnObject;

    [Header("Audio Sources (Optional)")]
    public AudioSource npcAudioSource;
    public AudioSource playerAudioSource;

    [Header("End Action Settings")]
    [Tooltip("Если true, UnityEvent сработает в конце диалога")]
    public bool EndAction = false;

    [Tooltip("Событие, которое сработает в конце диалога, если EndAction = true")]
    public UnityEvent onDialogueEndEvent;

    // --- Новое поле ---
    private List<DialogueLine> currentActiveDialogueLines = null;
    // ------------------

    private void Start()
    {
        if (autoFindPlayerAudio && playerAudioSource == null)
        {
            FindPlayerAudioSource();
        }
        if (isAudioSourceOnObject && npcAudioSource == null)
        {
            npcAudioSource = GetComponent<AudioSource>();
        }
    }

    public void TriggerDialogue()
    {
        if (isDisposable && hasBeenTriggered)
        {
            return;
        }

        if (playCutsceneOnlyOnce && hasCutsceneBeenPlayed && isCutsceneDialogue)
        {
            return;
        }

        if (dialogueSystem == null)
        {
            Debug.LogError("DialogueSystem не назначен в инспекторе!");
            return;
        }

        if (autoFindPlayerAudio && playerAudioSource == null)
        {
            FindPlayerAudioSource();
        }

        if (npcAudioSource != null) dialogueSystem.npcAudioSource = npcAudioSource;
        if (playerAudioSource != null) dialogueSystem.playerAudioSource = playerAudioSource;

        dialogueSystem.dialogueLines = new List<DialogueLine>(newDialogueLines);
        // --- Сохраняем активные строки ---
        currentActiveDialogueLines = new List<DialogueLine>(newDialogueLines);
        // ---------------------------------
        dialogueSystem.isCutsceneDialogue = isCutsceneDialogue;
        dialogueSystem.ActivateDialog();

        if (isDisposable)
        {
            hasBeenTriggered = true;
        }

        if (isCutsceneDialogue && playCutsceneOnlyOnce)
        {
            hasCutsceneBeenPlayed = true;
        }
    }

    public void OnInteractTrigger()
    {
        TriggerDialogue();
    }

    private void OnEnable()
    {
        if (dialogueSystem != null)
        {
            dialogueSystem.OnDialogueEnd += HandleDialogueEnd;
        }
    }

    private void OnDisable()
    {
        if (dialogueSystem != null)
        {
            dialogueSystem.OnDialogueEnd -= HandleDialogueEnd;
        }
    }

    private void HandleDialogueEnd()
    {
        // Проверяем, есть ли активный диалог
        if (currentActiveDialogueLines != null && currentActiveDialogueLines.Count > 0)
        {
            ClearDialogueLines();

            if (EndAction)
            {
                onDialogueEndEvent?.Invoke();
                Debug.Log("Dialogue ended and event invoked.");
            }

            // Очистка состояния
            currentActiveDialogueLines.Clear();
            currentActiveDialogueLines = null;
        }
    }

    private void ClearDialogueLines()
    {
        dialogueSystem.dialogueLines.Clear();
    }

    private void FindPlayerAudioSource()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerAudioSource = player.GetComponent<AudioSource>();
            if (playerAudioSource == null)
            {
                Debug.LogWarning("У игрока не найден компонент AudioSource!");
            }
        }
        else
        {
            Debug.LogWarning("Не найден объект с тегом 'Player'!");
        }
    }

    public void ResetDialogueState()
    {
        hasBeenTriggered = false;
        hasCutsceneBeenPlayed = false;
        currentActiveDialogueLines = null;
    }
}