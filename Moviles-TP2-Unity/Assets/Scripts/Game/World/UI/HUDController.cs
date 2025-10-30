using System;
using Game.Events;
using TMPro;
using Tools.EventSystem;
using Tools.Time;
using UnityEngine;

namespace Game.World.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private GameObject androidControls;
        [SerializeField] private TextMeshProUGUI velocityText;
        [SerializeField] private TextMeshProUGUI timerTxt;
        
        private const string SpeedString = "Speed: ";
        private Timer timer;

        private void Awake()
        {
            EventSystem.Subscribe<VelocityDebugEvent>(OnVelocityDebugEvent);
            androidControls.SetActive(Application.platform == RuntimePlatform.Android || Application.isEditor);
            timer = new Timer();
        }

        private void OnDestroy()
        {
            EventSystem.Unsubscribe<VelocityDebugEvent>(OnVelocityDebugEvent);
        }

        private void Update()
        {
            float seconds = timer.TimeElapsed;
            int minutes = (int)(seconds / 60f);
            seconds -= minutes * 60f;
            seconds %= 60f;
            
            timerTxt.text = $"{minutes:00}:{seconds:00}";
        }

        private void OnVelocityDebugEvent(VelocityDebugEvent obj)
        {
            velocityText.text = SpeedString + obj.speed.ToString("F2");
        }
    }
}
