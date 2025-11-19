using Game.Events;
using TMPro;
using Tools.EventSystem;
using Tools.ServiceProvider;
using UnityEngine;

namespace Game.World.FabbriPlugin
{
    internal sealed class LoggerMenuController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI logsTxt;

        private void Awake()
        {
            ServiceProvider.GetService<EventSystem>().Subscribe<LogsDeletedEvent>(OnLogsDeleted);
        }
        
        private void Start()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            ServiceProvider.GetService<EventSystem>().Unsubscribe<LogsDeletedEvent>(OnLogsDeleted);
        }

        public void OnReadLogs()
        {
            ServiceProvider.GetService<FileManager>().WriteFile("Logs Loaded");
            logsTxt.text = ServiceProvider.GetService<FileManager>().ReadFile();
        }

        public void OnDeleteLogs()
        {
            ServiceProvider.GetService<FileManager>().DeleteFile();
        }

        private void OnLogsDeleted(LogsDeletedEvent _)
        {
            logsTxt.text = "";
        }
    }
}