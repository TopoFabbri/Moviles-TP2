using Game.Events;
using Game.World.GameManager;
using Tools.EventSystem;
using Tools.FSM;
using UnityEngine;

namespace Game.World.Triggers
{
    internal sealed class TriggerPopup : MonoBehaviour
    {
        [SerializeField] private bool triggerOnce = true;
        [SerializeField] private GameObject popup;
        
        private new Collider collider;
        private bool triggered;

        private void Awake()
        {
            collider = GetComponent<Collider>();
            
            collider.isTrigger = true;
            collider.enabled = false;
            
            EventSystem.Subscribe<ChangedStateEvent<States>>(OnStateChanged);
        }

        private void OnDestroy()
        {
            EventSystem.Unsubscribe<ChangedStateEvent<States>>(OnStateChanged);
        }

        private void OnStateChanged(ChangedStateEvent<States> obj)
        {
            if (obj.state == States.Play)
                collider.enabled = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (triggered) return;
            
            triggered = true;
            
            popup.SetActive(true);
            EventSystem.Raise<TriggerPopupEvent>(popup);
            
            if (triggerOnce)
                Destroy(gameObject);
        }
    }
}
