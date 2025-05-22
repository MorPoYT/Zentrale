using UnityEngine;
using UnityEngine.Events;

public class ObjectUnityEvent : MonoBehaviour
{
    [SerializeField] private string _itemName;
    [SerializeField] private string _interactionHint = "Press [E] to use";
    [SerializeField] private UnityEvent _onUse;

    public string ItemName => _itemName;
    public string InteractionHint => _interactionHint;

    public void Use()
    {
        Debug.Log($"Using item: {_itemName}");
        _onUse?.Invoke();
    }
}
