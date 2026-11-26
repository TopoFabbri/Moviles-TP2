using Game.CloudSave;
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

        [Header("Control Upgrade Configuration")]
        [SerializeField] private Image controlBar;
        [SerializeField] private Button controlBuyBtn;
        [SerializeField] private TextMeshProUGUI controlBuyBtnText;
        [SerializeField] private int controlBarSegments = 20;
        [SerializeField] private string controlLabelString = "Control:";

        private int buyPrice = 1;
        private int controlBuyPrice = 1;
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
            SetupControlUI();
            SetupRewardedAdUI();
            SetupNoAdsUI();
            WireBakedButtonListeners();
            RefreshUI();
        }

        private void WireBakedButtonListeners()
        {
            if (controlBuyBtn != null)
            {
                controlBuyBtn.onClick.RemoveListener(BuyControlUpgrade);
                controlBuyBtn.onClick.AddListener(BuyControlUpgrade);
            }

            if (watchAdBtn != null)
            {
                watchAdBtn.onClick.RemoveListener(OnWatchAdClicked);
                watchAdBtn.onClick.AddListener(OnWatchAdClicked);
            }

            if (noAdsBtn != null)
            {
                noAdsBtn.onClick.RemoveListener(OnNoAdsClicked);
                noAdsBtn.onClick.AddListener(OnNoAdsClicked);
            }

            if (restorePurchasesBtn != null)
            {
                restorePurchasesBtn.onClick.RemoveListener(OnRestorePurchasesClicked);
                restorePurchasesBtn.onClick.AddListener(OnRestorePurchasesClicked);
            }
        }

        private void OnEnable()
        {
            GameSave.OnSaveDataChanged += RefreshUI;
            RefreshUI();
        }

        private void OnDisable()
        {
            GameSave.OnSaveDataChanged -= RefreshUI;
        }

        public void RefreshUI()
        {
            UpdateBar();
            UpdateBuyPrice();
            UpdateControlBar();
            UpdateControlBuyPrice();
            UpdateCreditsText();
            UpdateNoAdsUI();
        }

        private void Update()
        {
            UpdateRewardedAdUI();
            UpdateNoAdsUI();
        }

        private void SetupControlUI()
        {
            if (buyBtnText == null || bar == null) return;

            Button originalBuyBtn = buyBtnText.GetComponentInParent<Button>();
            if (originalBuyBtn == null || originalBuyBtn.transform.parent == null) return;

            Transform buyContainer = originalBuyBtn.transform.parent;
            Transform contentParent = buyContainer.parent;
            if (contentParent == null) return;

            ContentSizeFitter fitter = contentParent.GetComponent<ContentSizeFitter>();
            if (fitter == null)
                fitter = contentParent.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            if (controlBar == null && bar.transform.parent != null)
            {
                Transform barContainer = bar.transform.parent;
                int barIndex = barContainer.GetSiblingIndex();
                if (barIndex > 0)
                {
                    GameObject labelObj = Instantiate(contentParent.GetChild(barIndex - 1).gameObject, contentParent);
                    labelObj.name = "Control";
                    TextMeshProUGUI labelText = labelObj.GetComponent<TextMeshProUGUI>();
                    if (labelText != null)
                        labelText.text = controlLabelString;
                }

                GameObject controlBarContainer = Instantiate(barContainer.gameObject, contentParent);
                controlBarContainer.name = "ControlBar";
                Image[] images = controlBarContainer.GetComponentsInChildren<Image>(true);
                foreach (Image img in images)
                {
                    if (img.type == Image.Type.Filled)
                    {
                        controlBar = img;
                        break;
                    }
                }
            }

            if (controlBuyBtn == null)
            {
                GameObject controlBuyContainer = Instantiate(buyContainer.gameObject, contentParent);
                controlBuyContainer.name = "ControlBuy";
                controlBuyBtn = controlBuyContainer.GetComponentInChildren<Button>();
                controlBuyBtnText = controlBuyContainer.GetComponentInChildren<TextMeshProUGUI>();
            }

            if (controlBuyBtn != null)
            {
                for (int i = 0; i < controlBuyBtn.onClick.GetPersistentEventCount(); i++)
                {
                    controlBuyBtn.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
                }

                controlBuyBtn.onClick.RemoveAllListeners();
                controlBuyBtn.onClick.AddListener(OnBuyControlBtnClicked);
            }
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
                            ConfigureButtonAutoSizing(noAdsBtnText);
                        }

                        if (restorePurchasesBtn == null)
                        {
                            GameObject restoreContainer = Instantiate(buyContainer.gameObject, contentParent);
                            restoreContainer.name = "RestorePurchasesContainer";
                            restorePurchasesBtn = restoreContainer.GetComponentInChildren<Button>();
                            restorePurchasesBtnText = restoreContainer.GetComponentInChildren<TextMeshProUGUI>();
                            ConfigureButtonAutoSizing(restorePurchasesBtnText);
                        }
                    }
                }
            }

            if (noAdsBtn != null)
            {
                ConfigureButtonAutoSizing(noAdsBtnText);
                for (int i = 0; i < noAdsBtn.onClick.GetPersistentEventCount(); i++)
                {
                    noAdsBtn.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
                }

                noAdsBtn.onClick.RemoveAllListeners();
                noAdsBtn.onClick.AddListener(OnNoAdsBtnClicked);
            }

            if (restorePurchasesBtn != null)
            {
                ConfigureButtonAutoSizing(restorePurchasesBtnText);
                for (int i = 0; i < restorePurchasesBtn.onClick.GetPersistentEventCount(); i++)
                {
                    restorePurchasesBtn.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
                }

                restorePurchasesBtn.onClick.RemoveAllListeners();
                restorePurchasesBtn.onClick.AddListener(OnRestorePurchasesBtnClicked);
            }
        }

        private static void ConfigureButtonAutoSizing(TextMeshProUGUI label)
        {
            if (label == null) return;
            label.enableAutoSizing = true;
            label.fontSizeMin = 18f;
            label.fontSizeMax = 75f;
            label.margin = new Vector4(10f, 0f, 10f, 0f);
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
                    if (ServiceProvider.TryGetService<Analytics.IAnalyticsService>(out var analytics))
                    {
                        analytics.TrackCurrencyEarned("rewarded_ad", rewardedCredits, GameSave.CurrentLevel, GameSave.Credits);
                        analytics.TrackAdInteraction("rewarded", "Store", true);
                    }
                    if (CloudSaveService.HasInstance)
                        CloudSaveService.Instance.SaveNow();
                }
                else if (ServiceProvider.TryGetService<Analytics.IAnalyticsService>(out var analytics))
                {
                    analytics.TrackAdInteraction("rewarded", "Store", false);
                }
            });
        }

        public void OnBuyBtnClicked()
        {
            if (bar.fillAmount >= 1f) return;
            if (GameSave.Credits < buyPrice) return;
            
            int spent = buyPrice;
            GameSave.Credits -= spent;
            GameSave.AddedSpeed += addedSpeedMultiplier;
            GameSave.AddedSpeed = Mathf.Min(GameSave.AddedSpeed, barSegments * addedSpeedMultiplier);
            
            UpdateBar();
            UpdateBuyPrice();
            UpdateCreditsText();

            if (ServiceProvider.TryGetService<Analytics.IAnalyticsService>(out var analytics))
            {
                int upgradeLevel = Mathf.RoundToInt(GameSave.AddedSpeed / addedSpeedMultiplier);
                analytics.TrackInGamePurchase("speed_upgrade", "credits", spent, "CREDITS", upgradeLevel, GameSave.Credits);
            }

            if (CloudSaveService.HasInstance)
                CloudSaveService.Instance.SaveNow();
        }

        public void OnBuyControlBtnClicked()
        {
            if (controlBar == null || controlBar.fillAmount >= 1f) return;
            if (GameSave.Credits < controlBuyPrice) return;

            int spent = controlBuyPrice;
            GameSave.Credits -= spent;
            int nextLevel = Mathf.Min(Mathf.RoundToInt(GameSave.AddedControl * controlBarSegments) + 1, controlBarSegments);
            GameSave.AddedControl = nextLevel >= controlBarSegments ? 1f : (float)nextLevel / controlBarSegments;

            UpdateControlBar();
            UpdateControlBuyPrice();
            UpdateCreditsText();

            if (ServiceProvider.TryGetService<Analytics.IAnalyticsService>(out var analytics))
            {
                analytics.TrackInGamePurchase("control_upgrade", "credits", spent, "CREDITS", nextLevel, GameSave.Credits);
            }

            if (CloudSaveService.HasInstance)
                CloudSaveService.Instance.SaveNow();
        }

        private void UpdateBar()
        {
            bar.fillAmount = GameSave.AddedSpeed / addedSpeedMultiplier / barSegments;
        }

        private void UpdateControlBar()
        {
            if (controlBar == null) return;
            controlBar.fillAmount = Mathf.Clamp01(GameSave.AddedControl);
        }

        private void UpdateBuyPrice()
        {
            buyPrice = Mathf.Max(3 * (int)(GameSave.AddedSpeed / addedSpeedMultiplier), 1);
            buyBtnText.text = buyBtnString.Replace("<price>", buyPrice.ToString());
        }

        private void UpdateControlBuyPrice()
        {
            if (controlBuyBtnText == null) return;
            int currentLevel = Mathf.RoundToInt(GameSave.AddedControl * controlBarSegments);
            controlBuyPrice = Mathf.Max(3 * currentLevel, 1);
            controlBuyBtnText.text = buyBtnString.Replace("<price>", controlBuyPrice.ToString());
        }
        
        private void UpdateCreditsText()
        {
            creditsText.text = creditsString + GameSave.Credits;
        }
    }
}