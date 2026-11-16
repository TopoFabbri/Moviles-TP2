using Game.World.Ads;
using Game.World.GameManager;
using TMPro;
using Tools.ServiceProvider;
using UnityEngine;
using UnityEngine.UI;

namespace Game.World.UI
{
    internal sealed class StoreMenuController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI creditsText;
        [SerializeField] private TextMeshProUGUI buyBtnText;
        
        [SerializeField] private Image bar;
        [SerializeField] private int barSegments = 20;
        [SerializeField] private float addedSpeedMultiplier = 3f;
        
        [SerializeField] private string creditsString = "Credits: ";
        [SerializeField] private string buyBtnString = "Buy: <price> credits";

        [Header("Rewarded Ad Configuration")]
        [SerializeField] private Button watchAdBtn;
        [SerializeField] private TextMeshProUGUI watchAdBtnText;
        [SerializeField] private int rewardedCredits = 5;
        [SerializeField] private string watchAdBtnString = "Free: +<credits> creds";
        [SerializeField] private string adCooldownString = "Ad: <seconds>s";

        private int buyPrice = 1;
        private IAdService adService;

        private void Start()
        {
            adService = ServiceProvider.GetService<IAdService>();
            SetupRewardedAdUI();
            UpdateBar();
            UpdateBuyPrice();
            UpdateCreditsText();
        }

        private void Update()
        {
            UpdateRewardedAdUI();
        }

        private void SetupRewardedAdUI()
        {
            if (watchAdBtn == null && buyBtnText != null)
            {
                Button originalBuyBtn = buyBtnText.GetComponentInParent<Button>();
                if (originalBuyBtn != null && originalBuyBtn.transform.parent != null)
                {
                    Transform buyContainer = originalBuyBtn.transform.parent;
                    Transform contentParent = buyContainer.parent;
                    if (contentParent != null)
                    {
                        GameObject adContainer = Instantiate(buyContainer.gameObject, contentParent);
                        adContainer.name = "WatchAdContainer";
                        watchAdBtn = adContainer.GetComponentInChildren<Button>();
                        watchAdBtnText = adContainer.GetComponentInChildren<TextMeshProUGUI>();
                    }
                }
            }

            if (watchAdBtn != null)
            {
                // Disable any persistent inspector listeners inherited from cloning the Buy button
                for (int i = 0; i < watchAdBtn.onClick.GetPersistentEventCount(); i++)
                {
                    watchAdBtn.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
                }

                watchAdBtn.onClick.RemoveAllListeners();
                watchAdBtn.onClick.AddListener(OnWatchAdBtnClicked);
            }
        }

        private void UpdateRewardedAdUI()
        {
            if (watchAdBtn == null || watchAdBtnText == null) return;

            if (adService == null)
                adService = ServiceProvider.GetService<IAdService>();

            float remaining = 0f;
            bool canShow = adService != null && adService.CanShowRewarded(out remaining);

            watchAdBtn.interactable = canShow;
            if (canShow)
            {
                watchAdBtnText.text = watchAdBtnString.Replace("<credits>", rewardedCredits.ToString());
            }
            else
            {
                watchAdBtnText.text = adCooldownString.Replace("<seconds>", Mathf.CeilToInt(remaining).ToString());
            }
        }

        public void OnWatchAdBtnClicked()
        {
            if (adService == null)
                adService = ServiceProvider.GetService<IAdService>();

            if (adService == null || !adService.CanShowRewarded(out _)) return;

            adService.ShowRewarded("Store", success =>
            {
                if (success)
                {
                    GameSave.Credits += rewardedCredits;
                    UpdateCreditsText();
                }
            });
        }

        public void OnBuyBtnClicked()
        {
            if (bar.fillAmount >= 1f) return;
            if (GameSave.Credits < buyPrice) return;
            
            GameSave.Credits -= buyPrice;
            GameSave.AddedSpeed += addedSpeedMultiplier;
            GameSave.AddedSpeed = Mathf.Min(GameSave.AddedSpeed, barSegments * addedSpeedMultiplier);
            
            UpdateBar();
            UpdateBuyPrice();
            UpdateCreditsText();
        }

        private void UpdateBar()
        {
            bar.fillAmount = GameSave.AddedSpeed / addedSpeedMultiplier / barSegments;
        }

        private void UpdateBuyPrice()
        {
            buyPrice = Mathf.Max(3 * (int)(GameSave.AddedSpeed / addedSpeedMultiplier), 1);
            buyBtnText.text = buyBtnString.Replace("<price>", buyPrice.ToString());
        }
        
        private void UpdateCreditsText()
        {
            creditsText.text = creditsString + GameSave.Credits;
        }
    }
}