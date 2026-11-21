using Game.CloudSave;
using Game.World.GameManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.World.UI
{
    public class CloudSavePopupController : MonoBehaviour
    {
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private TextMeshProUGUI localStatsText;
        [SerializeField] private TextMeshProUGUI cloudStatsText;
        [SerializeField] private Button useCloudButton;
        [SerializeField] private Button keepLocalButton;

        private void Awake()
        {
            if (popupRoot != null)
                popupRoot.SetActive(false);

            if (useCloudButton != null)
                useCloudButton.onClick.AddListener(OnUseCloudClicked);

            if (keepLocalButton != null)
                keepLocalButton.onClick.AddListener(OnKeepLocalClicked);
        }

        private void OnEnable()
        {
            CloudSaveService service = CloudSaveService.Instance;
            service.OnCloudConflictDetected += ShowPopup;
            service.OnCloudConflictCleared += ClosePopup;

            if (service.PendingCloudData != null)
                ShowPopup(service.PendingCloudData);
        }

        private void OnDisable()
        {
            if (CloudSaveService.HasInstance)
            {
                CloudSaveService.Instance.OnCloudConflictDetected -= ShowPopup;
                CloudSaveService.Instance.OnCloudConflictCleared -= ClosePopup;
            }
        }

        private void OnDestroy()
        {
            if (useCloudButton != null)
                useCloudButton.onClick.RemoveListener(OnUseCloudClicked);

            if (keepLocalButton != null)
                keepLocalButton.onClick.RemoveListener(OnKeepLocalClicked);
        }

        public void ShowPopup(CloudSaveData cloudData)
        {
            if (cloudData == null)
                return;

            if (localStatsText != null)
            {
                localStatsText.text =
                    $"Level: {GameSave.CurrentLevel}\n" +
                    $"Credits: {GameSave.Credits}\n" +
                    $"Added Speed: {GameSave.AddedSpeed:F1}";
            }

            if (cloudStatsText != null)
            {
                cloudStatsText.text =
                    $"Level: {cloudData.currentLevel}\n" +
                    $"Credits: {cloudData.credits}\n" +
                    $"Added Speed: {cloudData.addedSpeed:F1}";
            }

            if (popupRoot != null)
                popupRoot.SetActive(true);
        }

        private void OnUseCloudClicked()
        {
            CloudSaveService.Instance.ResolveUseCloud();
            ClosePopup();
        }

        private void OnKeepLocalClicked()
        {
            CloudSaveService.Instance.ResolveKeepLocal();
            ClosePopup();
        }

        private void ClosePopup()
        {
            if (popupRoot != null)
                popupRoot.SetActive(false);
        }
    }
}
