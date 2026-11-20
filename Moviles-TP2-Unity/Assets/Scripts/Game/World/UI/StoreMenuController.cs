using Game.World.Ads;
using Game.World.GameManager;
using Game.World.IAP;
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

        [Header("No Ads IAP Configuration")]
        [SerializeField] private Button noAdsBtn;
        [SerializeField] private TextMeshProUGUI noAdsBtnText;
        [SerializeField] private Button restorePurchasesBtn;
        [SerializeField] private TextMeshProUGUI restorePurchasesBtnText;

        private int buyPrice = 1;
        private IAdService adService;
        private IIAPService iapService;
        private float _noAdsFeedbackUntil;
        private string _noAdsFeedbackText;
        private float _restoreFeedbackUntil;
        private string _restoreFeedbackText;

        private void Start()
        {
            adService = ServiceProvider.GetService<IAdService>();
            iapService = ServiceProvider.GetService<IIAPService>();
            SetupRewardedAdUI();
            SetupNoAdsUI();
            UpdateBar();
            UpdateBuyPrice();
            UpdateCreditsText();
            UpdateNoAdsUI();
        }

        private void Update()
        {
            UpdateRewardedAdUI();
            UpdateNoAdsUI();
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

        private void SetupNoAdsUI()
        {
            if (buyBtnText != null)
            {
                Button originalBuyBtn = buyBtnText.GetComponentInParent<Button>();
                if (originalBuyBtn != null && originalBuyBtn.transform.parent != null)
                {
                    Transform buyContainer = originalBuyBtn.transform.parent;
                    Transform contentParent = buyContainer.parent;
                    if (contentParent != null)
                    {
                        if (noAdsBtn == null)
                        {
                            GameObject noAdsContainer = Instantiate(buyContainer.gameObject, contentParent);
                            noAdsContainer.name = "NoAdsContainer";
                            noAdsBtn = noAdsContainer.GetComponentInChildren<Button>();
                            noAdsBtnText = noAdsContainer.GetComponentInChildren<TextMeshProUGUI>();
                        }

                        if (restorePurchasesBtn == null)
                        {
                            GameObject restoreContainer = Instantiate(buyContainer.gameObject, contentParent);
                            restoreContainer.name = "RestorePurchasesContainer";
                            restorePurchasesBtn = restoreContainer.GetComponentInChildren<Button>();
                            restorePurchasesBtnText = restoreContainer.GetComponentInChildren<TextMeshProUGUI>();
                        }
                    }
                }
            }

            if (noAdsBtn != null)
            {
                for (int i = 0; i < noAdsBtn.onClick.GetPersistentEventCount(); i++)
                {
                    noAdsBtn.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
                }

                noAdsBtn.onClick.RemoveAllListeners();
                noAdsBtn.onClick.AddListener(OnNoAdsBtnClicked);
            }

            if (restorePurchasesBtn != null)
            {
                for (int i = 0; i < restorePurchasesBtn.onClick.GetPersistentEventCount(); i++)
                {
                    restorePurchasesBtn.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
                }

                restorePurchasesBtn.onClick.RemoveAllListeners();
                restorePurchasesBtn.onClick.AddListener(OnRestorePurchasesBtnClicked);
            }
        }

        private void UpdateNoAdsUI()
        {
            if (iapService == null)
                iapService = ServiceProvider.GetService<IIAPService>();

            if (noAdsBtnText != null)
            {
                if (Time.unscaledTime < _noAdsFeedbackUntil)
                {
                    noAdsBtnText.text = _noAdsFeedbackText;
                }
                else if (GameSave.HasNoAds)
                {
                    noAdsBtnText.text = GameSave.AdsEnabled ? "Ads: ON" : "Ads: OFF";
                }
                else
                {
                    string price = iapService != null ? iapService.NoAdsPriceString : "$1.99";
                    noAdsBtnText.text = $"No Ads ({price})";
                }
            }

            if (restorePurchasesBtn != null)
            {
                restorePurchasesBtn.gameObject.SetActive(!GameSave.HasNoAds);
                if (restorePurchasesBtnText != null)
                    restorePurchasesBtnText.text = Time.unscaledTime < _restoreFeedbackUntil
                        ? _restoreFeedbackText
                        : "Restore Purchases";
            }
        }

        public void OnNoAdsBtnClicked()
        {
            if (GameSave.HasNoAds)
            {
                GameSave.AdsEnabled = !GameSave.AdsEnabled;
                UpdateNoAdsUI();
                return;
            }

            if (iapService == null)
                iapService = ServiceProvider.GetService<IIAPService>();

            if (iapService == null)
            {
                _noAdsFeedbackText = "Store Unavailable";
                _noAdsFeedbackUntil = Time.unscaledTime + 2f;
                UpdateNoAdsUI();
                return;
            }

            iapService.BuyNoAds(success =>
            {
                if (!success)
                {
                    _noAdsFeedbackText = "Unavailable";
                    _noAdsFeedbackUntil = Time.unscaledTime + 2f;
                }
                UpdateNoAdsUI();
            });
        }

        public void OnRestorePurchasesBtnClicked()
        {
            if (iapService == null)
                iapService = ServiceProvider.GetService<IIAPService>();

            if (iapService == null)
            {
                _restoreFeedbackText = "Store Unavailable";
                _restoreFeedbackUntil = Time.unscaledTime + 2f;
                UpdateNoAdsUI();
                return;
            }

            iapService.RestorePurchases(success =>
            {
                if (!success)
                {
                    _restoreFeedbackText = "Not Found";
                    _restoreFeedbackUntil = Time.unscaledTime + 2f;
                }
                UpdateNoAdsUI();
            });
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