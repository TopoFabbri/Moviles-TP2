using Game.World.Character;
using Model.Tools.EventSystem;
using TMPro;
using UnityEngine;

namespace Game.World.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI velocityText;

        private const string SpeedString = "Speed: ";

        private void Awake()
        {
            EventSystem.Subscribe<VelocityDebugEvent>(OnVelocityDebugEvent);;
        }

        private void OnDestroy()
        {
            EventSystem.Unsubscribe<VelocityDebugEvent>(OnVelocityDebugEvent);
        }

        private void OnVelocityDebugEvent(VelocityDebugEvent obj)
        {
            velocityText.text = SpeedString + obj.speed.ToString("F2");
        }
    }
}
