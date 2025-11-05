using Game.Events;
using Game.World.GameManager;
using TMPro;
using Tools.EventSystem;
using Tools.FSM;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Timer = Tools.Time.Timer;

namespace Game.World.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private GameObject androidControls;
        [SerializeField] private TextMeshProUGUI velocityText;
        [SerializeField] private TextMeshProUGUI timerTxt;
        [SerializeField] private Image speedBar;
        [SerializeField] private int speedBarSegments = 32;

        private Timer timer;

        private void Awake()
        {
            EventSystem.Subscribe<VelocityDebugEvent>(OnVelocityDebugEvent);
            EventSystem.Subscribe<ChangedStateEvent<States>>(OnStateChanged);
            EventSystem.Subscribe<MoveAction>(OnMove);
            
            androidControls.SetActive(Application.platform == RuntimePlatform.Android || Application.isEditor);
            timer = new Timer();
        }

        private void OnMove(MoveAction obj)
        {
            timer.Start();
        }

        private void OnDestroy()
        {
            EventSystem.Unsubscribe<VelocityDebugEvent>(OnVelocityDebugEvent);
            EventSystem.Unsubscribe<ChangedStateEvent<States>>(OnStateChanged);
        }

        private void Update()
        {
            float seconds = timer.TimeElapsed;
            int minutes = (int)(seconds / 60f);
            seconds -= minutes * 60f;
            seconds %= 60f;

            timerTxt.text = $"{minutes:00}:{seconds:00}";
        }

        internal void Restart()
        {
            timer.Restart();
        }

        private void OnVelocityDebugEvent(VelocityDebugEvent obj)
        {
            float fillAmount = obj.speed / obj.maxSpeed;
            fillAmount = Mathf.Round(fillAmount * speedBarSegments) / speedBarSegments;
            speedBar.fillAmount = fillAmount;
            velocityText.text = obj.speed.ToString("00");
        }

        private void OnStateChanged(ChangedStateEvent<States> obj)
        {
            if (obj.state == States.Play)
                timer.Resume();
            else
                timer.Pause();
        }
    }
}