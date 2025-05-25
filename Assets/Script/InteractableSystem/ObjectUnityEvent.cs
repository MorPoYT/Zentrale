using UnityEngine;
using UnityEngine.Events;

public class ObjectUnityEvent : MonoBehaviour
{
    [SerializeField] private string _itemName;
    [SerializeField] private string _interactionHint = "Press [E] to use";
    [SerializeField] private UnityEvent _onUse;
    [Tooltip("Если true, то это диалог //для работа нажатий//")]
    [SerializeField] private bool isDialog;
    [Tooltip("Если true, действие можно выполнить только один раз")]
    [SerializeField] private bool _isOneTimeAction = false;
    
    private bool _hasBeenUsed = false;

    public string ItemName => _itemName;
    public string InteractionHint => _interactionHint;

    public void Use()
    {
        if (_isOneTimeAction && _hasBeenUsed) return;
        
        Debug.Log($"Using item: {_itemName}");
        if(isDialog == true) 
        {
            InteractableSystem.SetInteractionAllowed(false);
        }
        _onUse?.Invoke();
        
        _hasBeenUsed = true;
        
        // Опционально: отключаем компонент после использования
        if (_isOneTimeAction)
        {
            this.enabled = false;
        }
    }
}