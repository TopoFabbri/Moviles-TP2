using System;
using Game.CloudSave;
using Game.World.GameManager;
using GooglePlayGames;
using Tools.ServiceProvider;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Game.World.IAP
{
    public class IAPService : IIAPService, IDetailedStoreListener
    {
        public const string NoAdsProductId = "no_ads";
        private const string TesterEmail = "matufabbri@gmail.com";
        private const string DefaultPriceString = "$1.99";

        private IStoreController _storeController;
        private IExtensionProvider _extensionProvider;
        private Action<bool> _pendingPurchaseCallback;
        private bool _isInitializing;

        public bool IsInitialized => _storeController != null && _extensionProvider != null;

        public string NoAdsPriceString
        {
            get
            {
                if (_storeController != null)
                {
                    Product product = _storeController.products.WithID(NoAdsProductId);
                    if (product != null && !string.IsNullOrEmpty(product.metadata?.localizedPriceString))
                        return product.metadata.localizedPriceString;
                }
                return DefaultPriceString;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void AutoRegister()
        {
            if (!ServiceProvider.TryGetService(out IAPService service))
            {
                service = new IAPService();
            }
            ServiceProvider.TryAddService<IIAPService>(service);
        }

        public IAPService()
        {
            ServiceProvider.TryAddService<IAPService>(this);
            ServiceProvider.TryAddService<IIAPService>(this);

            CheckTesterAccount();
            InitializePurchasing();
        }

        private void InitializePurchasing()
        {
            if (IsInitialized || _isInitializing)
                return;

            _isInitializing = true;
            try
            {
                ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
                builder.AddProduct(NoAdsProductId, ProductType.NonConsumable);
                UnityPurchasing.Initialize(this, builder);
            }
            catch (Exception ex)
            {
                _isInitializing = false;
                Debug.LogWarning($"[IAPService] UnityPurchasing.Initialize exception: {ex.Message}");
            }
        }

        public void OnAccountChanged()
        {
            CheckTesterAccount();
        }

        private static bool CheckTesterAccount()
        {
            try
            {
                if (PlayGamesPlatform.Instance != null && PlayGamesPlatform.Instance.IsAuthenticated())
                {
                    string userName = PlayGamesPlatform.Instance.GetUserDisplayName();
                    string userId = PlayGamesPlatform.Instance.GetUserId();
                    if (string.Equals(userName, TesterEmail, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(userId, TesterEmail, StringComparison.OrdinalIgnoreCase))
                    {
                        GrantNoAdsIfNew();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IAPService] Tester account check failed: {ex.Message}");
            }

            return false;
        }

        private static void GrantNoAdsIfNew()
        {
            if (!GameSave.HasNoAds)
            {
                GameSave.HasNoAds = true;
                GameSave.AdsEnabled = false;
                if (CloudSaveService.HasInstance)
                    CloudSaveService.Instance.SaveNow();
            }
        }

        public void BuyNoAds(Action<bool> onComplete = null)
        {
            if (GameSave.HasNoAds)
            {
                onComplete?.Invoke(true);
                return;
            }

            if (IsInitialized)
            {
                Product product = _storeController.products.WithID(NoAdsProductId);
                if (product != null && product.availableToPurchase)
                {
                    _pendingPurchaseCallback = onComplete;
                    _storeController.InitiatePurchase(product);
                    return;
                }
            }

#if UNITY_EDITOR
            Debug.Log("[IAPService] Editor Fake Store: granting No Ads.");
            GrantNoAdsIfNew();
            onComplete?.Invoke(true);
#else
            if (!IsInitialized)
                InitializePurchasing();
            Debug.LogWarning("[IAPService] Store not initialized or product unavailable.");
            onComplete?.Invoke(false);
#endif
        }

        public void RestorePurchases(Action<bool> onComplete = null)
        {
            if (CheckTesterAccount())
            {
                onComplete?.Invoke(true);
                return;
            }

            if (!IsInitialized)
            {
#if UNITY_EDITOR
                Debug.Log("[IAPService] Editor Fake Store: RestorePurchases.");
                onComplete?.Invoke(GameSave.HasNoAds);
#else
                InitializePurchasing();
                Debug.LogWarning("[IAPService] Cannot restore purchases: store not initialized.");
                onComplete?.Invoke(GameSave.HasNoAds);
#endif
                return;
            }

            Product noAdsProduct = _storeController.products.WithID(NoAdsProductId);
            if (noAdsProduct != null && noAdsProduct.hasReceipt)
            {
                GrantNoAdsIfNew();
                onComplete?.Invoke(true);
                return;
            }

#if UNITY_ANDROID
            IGooglePlayStoreExtensions googleExtensions = _extensionProvider.GetExtension<IGooglePlayStoreExtensions>();
            if (googleExtensions != null)
            {
                googleExtensions.RestoreTransactions((success, error) =>
                {
                    Product restored = _storeController.products.WithID(NoAdsProductId);
                    bool owned = restored != null && restored.hasReceipt;
                    if (owned)
                        GrantNoAdsIfNew();
                    onComplete?.Invoke(owned || GameSave.HasNoAds);
                });
                return;
            }
#elif UNITY_IOS
            IAppleExtensions appleExtensions = _extensionProvider.GetExtension<IAppleExtensions>();
            if (appleExtensions != null)
            {
                appleExtensions.RestoreTransactions((success, error) =>
                {
                    Product restored = _storeController.products.WithID(NoAdsProductId);
                    bool owned = restored != null && restored.hasReceipt;
                    if (owned)
                        GrantNoAdsIfNew();
                    onComplete?.Invoke(owned || GameSave.HasNoAds);
                });
                return;
            }
#endif

            onComplete?.Invoke(GameSave.HasNoAds);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _isInitializing = false;
            _storeController = controller;
            _extensionProvider = extensions;

            Product noAdsProduct = _storeController.products.WithID(NoAdsProductId);
            if (noAdsProduct != null && noAdsProduct.hasReceipt)
            {
                GrantNoAdsIfNew();
            }
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            _isInitializing = false;
            Debug.LogWarning($"[IAPService] Initialization failed: {error}");
            if (ServiceProvider.TryGetService<Game.World.FabbriPlugin.IDiagnosticLogService>(out var diag))
                diag.RecordFailure("IAP", "Initialize", error.ToString());
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            _isInitializing = false;
            Debug.LogWarning($"[IAPService] Initialization failed: {error} - {message}");
            if (ServiceProvider.TryGetService<Game.World.FabbriPlugin.IDiagnosticLogService>(out var diag))
                diag.RecordFailure("IAP", "Initialize", $"{error} - {message}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            if (string.Equals(purchaseEvent.purchasedProduct.definition.id, NoAdsProductId, StringComparison.Ordinal))
            {
                GrantNoAdsIfNew();
                Action<bool> callback = _pendingPurchaseCallback;
                _pendingPurchaseCallback = null;
                callback?.Invoke(true);
            }

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.LogWarning($"[IAPService] Purchase failed for {product.definition.id}: {failureReason}");
            if (ServiceProvider.TryGetService<Game.World.FabbriPlugin.IDiagnosticLogService>(out var diag))
                diag.RecordFailure("IAP", "Purchase", $"{product.definition.id}: {failureReason}");
            Action<bool> callback = _pendingPurchaseCallback;
            _pendingPurchaseCallback = null;
            callback?.Invoke(false);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Debug.LogWarning($"[IAPService] Purchase failed for {product.definition.id}: {failureDescription.reason} - {failureDescription.message}");
            if (ServiceProvider.TryGetService<Game.World.FabbriPlugin.IDiagnosticLogService>(out var diag))
                diag.RecordFailure("IAP", "Purchase", $"{product.definition.id}: {failureDescription.reason} - {failureDescription.message}");
            Action<bool> callback = _pendingPurchaseCallback;
            _pendingPurchaseCallback = null;
            callback?.Invoke(false);
        }
    }
}
