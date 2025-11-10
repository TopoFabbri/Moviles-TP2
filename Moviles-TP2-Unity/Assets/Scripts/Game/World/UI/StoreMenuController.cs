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

        [SerializeField] private string creditsString = "Credits: ";
        [SerializeField] private string buyBtnString = "Buy: <price> credits";

        private int buyPrice = 1;
        
        public void OnBuyBtnClicked()
        {
            if (bar.fillAmount >= 1f) return;
            
            bar.fillAmount += Mathf.Min(1f / barSegments, 1f - bar.fillAmount);
            
            buyPrice++;
            buyBtnText.text = buyBtnString.Replace("<price>", buyPrice.ToString());
            creditsText.text = creditsString + buyPrice;
        }
    }
}