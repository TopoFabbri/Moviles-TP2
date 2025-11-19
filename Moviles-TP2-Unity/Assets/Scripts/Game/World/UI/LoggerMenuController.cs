using Game.Events;
using TMPro;
using Tools.EventSystem;
using UnityEngine;

namespace Game.World.UI
{
    internal sealed class LoggerMenuController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI logsTxt;

        private void Awake()
        {
            EventSystem.Subscribe<LogsDeletedEvent>(OnLogsDeleted);
        }
        
        private void Start()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            EventSystem.Unsubscribe<LogsDeletedEvent>(OnLogsDeleted);
        }

        public void OnReadLogs()
        {
            FileManager.WriteFile("Logs Loaded");
            logsTxt.text = FileManager.ReadFile();
        }

        public void OnDeleteLogs()
        {
            FileManager.DeleteFile();
        }

        private void OnLogsDeleted(LogsDeletedEvent _)
        {
            logsTxt.text = "";
        }
    }
}