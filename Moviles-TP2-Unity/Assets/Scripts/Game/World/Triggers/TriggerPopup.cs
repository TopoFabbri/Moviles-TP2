using Game.Events;
using Game.World.GameManager;
using Tools.EventSystem;
using Tools.FSM;
using Tools.ServiceProvider;
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
            
            ServiceProvider.GetService<EventSystem>().Subscribe<ChangedStateEvent<States>>(OnStateChanged);
        }

        private void OnDestroy()
        {
            ServiceProvider.GetService<EventSystem>().Unsubscribe<ChangedStateEvent<States>>(OnStateChanged);
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
            ServiceProvider.GetService<EventSystem>().Raise<TriggerPopupEvent>(popup);
            
            if (triggerOnce)
                Destroy(gameObject);
        }
    }
}
