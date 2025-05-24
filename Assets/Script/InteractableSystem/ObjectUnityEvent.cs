using UnityEngine;
using UnityEngine.Events;

public class ObjectUnityEvent : MonoBehaviour
{
    [SerializeField] private string _itemName;
    [SerializeField] private string _interactionHint = "Press [E] to use";
    [SerializeField] private UnityEvent _onUse;
    [Tooltip("Если true, то это диалог //для работа нажатий//")]
    [SerializeField] private bool isDialog;

    public string ItemName => _itemName;
    public string InteractionHint => _interactionHint;

    public void Use()
    {
        Debug.Log($"Using item: {_itemName}");
        if(isDialog == true) {
            InteractableSystem.SetInteractionAllowed(false);
        }
        _onUse?.Invoke();
    }
}
