using System;
using Tools.ServiceProvider;

namespace Game.World.IAP
{
    public interface IIAPService : IService
    {
        bool IsInitialized { get; }
        string NoAdsPriceString { get; }
        void BuyNoAds(Action<bool> onComplete = null);
        void RestorePurchases(Action<bool> onComplete = null);
    }
}
