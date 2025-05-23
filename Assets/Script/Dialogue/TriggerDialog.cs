using UnityEngine;
using UnityEngine.Events;

public class TriggerDialog : MonoBehaviour
{
    [SerializeField] private UnityEvent _onUse;

    void OnTriggerEnter(Collider col) {
    	if(col.gameObject.tag == "Player") {
    		_onUse?.Invoke();
    	}
    }
}
