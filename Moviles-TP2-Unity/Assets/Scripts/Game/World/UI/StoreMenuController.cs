using Game.World.GameManager;
using TMPro;
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

        private int buyPrice = 1;

        private void Start()
        {
            UpdateBar();
            UpdateBuyPrice();
            UpdateCreditsText();
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