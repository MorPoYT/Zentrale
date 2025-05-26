using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Events; // Для работы с UnityEvent

[RequireComponent(typeof(PlayableDirector))]
public class CutsceneInitializer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private float delayBeforeStart = 0f;
    [SerializeField] private bool restartOnPlay = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onCutsceneFinished; // Событие, вызываемое по окончании катсцены

    private PlayableDirector playableDirector;

    private void Awake()
    {
        playableDirector = GetComponent<PlayableDirector>();
        
        if (playableDirector == null)
        {
            Debug.LogError("PlayableDirector component is missing!", this);
            return;
        }

        // Подписываемся на событие окончания катсцены
        playableDirector.stopped += OnCutsceneStopped;
    }

    private void OnDestroy()
    {
        // Отписываемся при уничтожении объекта
        if (playableDirector != null)
        {
            playableDirector.stopped -= OnCutsceneStopped;
        }
    }

    private void Start()
    {
        if (playOnStart)
        {
            StartCutscene();
        }
    }

    /// <summary>
    /// Запускает катсцену (Timeline)
    /// </summary>
    public void StartCutscene()
    {
        if (playableDirector == null)
        {
            Debug.LogError("PlayableDirector is not assigned!", this);
            return;
        }

        if (delayBeforeStart > 0)
        {
            Invoke(nameof(ExecuteCutscene), delayBeforeStart);
        }
        else
        {
        	PlayerController.Instance.CloseAllAnimation();
            PlayerController.Instance.isControllUnenabled = true;
            ExecuteCutscene();
        }
    }

    private void ExecuteCutscene()
    {
        if (restartOnPlay || playableDirector.state != PlayState.Playing)
        {
            playableDirector.Stop();
            playableDirector.Play();
        }
    }

    /// <summary>
    /// Проверяет, закончилась ли катсцена
    /// </summary>
    public bool IsCutsceneFinished()
    {
        if (playableDirector == null)
            return true;

        // Катсцена считается завершённой, если она не играет и её время >= длительности
        return playableDirector.state != PlayState.Playing && 
               playableDirector.time >= playableDirector.duration;
    }

    /// <summary>
    /// Останавливает катсцену
    /// </summary>
    public void StopCutscene()
    {
        if (playableDirector != null)
        {
            playableDirector.Stop();
        }
    }

    /// <summary>
    /// Приостанавливает катсцену
    /// </summary>
    public void PauseCutscene()
    {
        if (playableDirector != null)
        {
            playableDirector.Pause();
        }
    }

    /// <summary>
    /// Возобновляет катсцену
    /// </summary>
    public void ResumeCutscene()
    {
        if (playableDirector != null && playableDirector.state == PlayState.Paused)
        {
            playableDirector.Resume();
        }
    }

    // Вызывается при остановке катсцены (вручную или по завершении)
    private void OnCutsceneStopped(PlayableDirector director)
    {
        // Проверяем, что катсцена завершилась естественным путём (а не была остановлена вручную)
        if (director.time >= director.duration)
        {
            onCutsceneFinished?.Invoke(); // Вызываем событие
        }
    }
}