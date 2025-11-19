using System.Collections.Generic;
using Game.Events;
using Tools.EventSystem;
using Tools.ServiceProvider;
using UnityEngine;

namespace Game.World.UI
{
    public class PopupManager : MonoBehaviour
    {
        [SerializeField] private List<GameObject> popups;
        
        private void Awake()
        {
            foreach (GameObject popup in popups)
            {
                popup.SetActive(false);
            }
        }

        public void OnResume()
        {
            ServiceProvider.GetService<EventSystem>().Raise<CancelAction>();
        }
    }
}
