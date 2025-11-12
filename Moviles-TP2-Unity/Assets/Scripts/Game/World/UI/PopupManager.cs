using Game.Events;
using Tools.EventSystem;
using UnityEngine;

namespace Game.World.UI
{
    public class PopupManager : MonoBehaviour
    {
        public void OnResume()
        {
            EventSystem.Raise<CancelAction>();
        }
    }
}
