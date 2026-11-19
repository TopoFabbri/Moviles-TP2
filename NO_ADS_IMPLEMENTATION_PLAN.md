# Implementation Plan: "No Ads" IAP & Ad Toggle System

## Goal Description
Implement a real-money In-App Purchase (IAP) for **"No Ads"** using Unity IAP (`com.unity.purchasing`) / Google Play Billing.
- When a player purchases **No Ads**, interstitial ads (on Game Over and Level Finished) are disabled by default.
- Players who own **No Ads** can toggle ads **ON / OFF** from the Store menu.
- A **"Restore Purchases"** button in the Store menu allows players to restore their No Ads entitlement.
- **Rewarded ads** ("Free: +5 creds" in the Store) remain available at all times regardless of No Ads ownership or toggle state.
- `matufabbri@gmail.com` is automatically recognized as a buyer/tester.
- **Product ID**: `no-ads` (draft exists in Google Play Console).

---

## IMPORTANT: Deletion & Completion Instructions
> **Rule for the implementing agent**:
> This plan file (`NO_ADS_IMPLEMENTATION_PLAN.md`) MUST be preserved throughout development and testing.
> **DO NOT DELETE** this file until:
> 1. All implementation, tests, and manual verification are complete.
> 2. The Google Play Console activation step via the browser subagent has completed.
> 3. You have received **explicit user confirmation** approving the finished feature.

---

## Proposed Changes

### 1. Package Dependency (via `unity-package-management`)
- Use the `unity-package-management` skill (`unity-cli` / UPM) to install `com.unity.purchasing` into `Moviles-TP2-Unity`.

---

### 2. Persistence (`GameSave` & `CloudSaveData`)
#### [MODIFY] `Moviles-TP2-Unity/Assets/Scripts/Game/World/GameManager/GameSave.cs`
Add two persisted properties following the existing `PlayerId + Key` pattern:
- `HasNoAds` (`bool`): `true` if the player owns the No Ads IAP.
- `AdsEnabled` (`bool`): `true` by default; when `HasNoAds` is purchased or restored, `AdsEnabled` is set to `false` automatically. Players with `HasNoAds == true` can toggle `AdsEnabled` between `true` and `false`.

#### [MODIFY] `Moviles-TP2-Unity/Assets/Scripts/Game/CloudSave/CloudSaveData.cs`
- Add `public bool hasNoAds;` and `public bool adsEnabled = true;` so No Ads ownership and toggle preference sync with Cloud Save.

---

### 3. IAP Service
#### [NEW] `IIAPService.cs` (`Moviles-TP2-Unity/Assets/Scripts/Game/World/IAP/IIAPService.cs`)
- Interface extending `IService` with:
  - `bool IsInitialized { get; }`
  - `string NoAdsPriceString { get; }` (localized price from store, e.g. `"$1.99"`)
  - `void BuyNoAds(Action<bool> onComplete = null)`
  - `void RestorePurchases(Action<bool> onComplete = null)`

#### [NEW] `IAPService.cs` (`Moviles-TP2-Unity/Assets/Scripts/Game/World/IAP/IAPService.cs`)
- Implements `IIAPService` and Unity IAP's `IDetailedStoreListener`.
- Registers itself with `ServiceProvider` via `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]` (matching `AdService`).
- Configures product ID **`no-ads`** as `ProductType.NonConsumable`.
- **Automatic Buyer Recognition**: Checks if the signed-in Google Play / tester account matches `matufabbri@gmail.com` (and automatically grants `HasNoAds = true` if not already set, while still allowing `AdsEnabled` to be toggled).
- **Restore Purchases**: `RestorePurchases(onComplete)` queries Google Play / Apple store receipts via `IExtensionProvider` / `IAppleExtensions` / `IGooglePlayStoreExtensions` and sets `GameSave.HasNoAds = true` + `GameSave.AdsEnabled = false` if `no-ads` has a valid receipt.
- Includes Unity Editor Fake Store / mock fallback so purchasing and restoring can be tested in Play Mode without a physical device.

---

### 4. Ad Suppression Logic
#### [MODIFY] `Moviles-TP2-Unity/Assets/Scripts/Game/World/Ads/AdService.cs`
- In `ShowInterstitial(string placement, Action onClosed = null)`:
  ```csharp
  if (GameSave.HasNoAds && !GameSave.AdsEnabled)
  {
      LogAdEvent($"Skipping Interstitial Ad (NoAds active, Placement: {placement})");
      onClosed?.Invoke();
      return;
  }
  ```
- `ShowRewarded(string placement, Action<bool> onRewardResult)` remains **unchanged** so players can always watch rewarded ads to earn free credits.

---

### 5. Store UI (`StoreMenuController`)
#### [MODIFY] `Moviles-TP2-Unity/Assets/Scripts/Game/World/UI/StoreMenuController.cs`
- Add serialized fields (with automatic cloning fallback from `buyBtnText` container, matching `SetupRewardedAdUI()`):
  - `noAdsBtn` (`Button`) & `noAdsBtnText` (`TextMeshProUGUI`)
  - `restorePurchasesBtn` (`Button`) & `restorePurchasesBtnText` (`TextMeshProUGUI`)
- Behavior:
  - **If `!GameSave.HasNoAds`**: `noAdsBtn` displays `"No Ads (<price>)"` and clicking it calls `iapService.BuyNoAds(...)`. `restorePurchasesBtn` is visible and calls `iapService.RestorePurchases(...)`.
  - **If `GameSave.HasNoAds`**: `noAdsBtn` acts as the **Ads Toggle**, displaying `"Ads: OFF"` or `"Ads: ON"`. Clicking it toggles `GameSave.AdsEnabled = !GameSave.AdsEnabled` and refreshes the label.

---

### 6. Activation on Google Play Console (Final Step)
- As the final deployment step, delegate to the **browser subagent** to navigate to Google Play Console:
  - Open **DroneRacer** → **Monetize** → **Products** → **In-app products**.
  - Locate the draft product with ID `no-ads`.
  - Activate the product so it is live for testers and production.

---

## Verification Plan

### Automated Tests
- Update `Moviles-TP2-Unity/Assets/Scripts/Editor/AdServiceTests.cs` with EditMode unit tests:
  - `ShowInterstitial` invokes `onClosed` immediately without setting `IsAdShowing` when `HasNoAds == true && AdsEnabled == false`.
  - `ShowInterstitial` shows the ad when `HasNoAds == true && AdsEnabled == true`.
  - `CanShowRewarded` / `ShowRewarded` still works when `HasNoAds == true && AdsEnabled == false`.
- Run Unity EditMode tests via `unity-cli` or Unity Test Runner.

### Manual Verification
1. Open the Store menu in Play Mode: verify **"No Ads"** and **"Restore Purchases"** buttons appear alongside **"Free: +5 creds"**.
2. Click **"No Ads"** (Fake Store / Editor mock): verify `HasNoAds` becomes `true`, `AdsEnabled` becomes `false`, and the button updates to **"Ads: OFF"**.
3. Trigger Game Over / Level Finished: verify **no interstitial ad** is shown.
4. Return to Store and click **"Free: +5 creds"**: verify the **rewarded ad still plays** and grants +5 credits.
5. Click **"Ads: OFF"** to toggle it to **"Ads: ON"**: verify interstitial ads play again on Game Over / Level Finished.
6. Test **"Restore Purchases"** button in Store to confirm it restores `HasNoAds` and updates the UI.
7. Run the browser subagent step to activate the `no-ads` draft on Google Play Console.
8. Request user confirmation of completion. Once approved, delete `NO_ADS_IMPLEMENTATION_PLAN.md`.
