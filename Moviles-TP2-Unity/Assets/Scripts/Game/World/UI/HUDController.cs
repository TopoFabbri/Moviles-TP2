using Game.Events;
using Game.World.GameManager;
using TMPro;
using Tools.EventSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Game.World.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private GameObject androidControls;
        [SerializeField] private TextMeshProUGUI velocityText;
        [SerializeField] private TextMeshProUGUI timerTxt;
        [SerializeField] private Image speedBar;
        [SerializeField] private int speedBarSegments = 32;

        private void Awake()
        {
            EventSystem.Subscribe<VelocityDebugEvent>(OnVelocityDebugEvent);
            
            androidControls.SetActive(Application.platform == RuntimePlatform.Android || Application.isEditor);
        }

        private void OnDestroy()
        {
            EventSystem.Unsubscribe<VelocityDebugEvent>(OnVelocityDebugEvent);
        }

        private void Update()
        {
            float seconds = GameTime.Timer.TimeElapsed;
            int minutes = (int)(seconds / 60f);
            seconds -= minutes * 60f;
            seconds %= 60f;

            timerTxt.text = $"{minutes:00}:{seconds:00}";
        }

        private void OnVelocityDebugEvent(VelocityDebugEvent obj)
        {
            float fillAmount = obj.speed / obj.maxSpeed;
            fillAmount = Mathf.Round(fillAmount * speedBarSegments) / speedBarSegments;
            speedBar.fillAmount = fillAmount;
            velocityText.text = obj.speed.ToString("00");
        }
    }
}