using Game.World.Character;
using Tools.ServiceProvider;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Input
{
    public class GlobalButtonListener : MonoBehaviour
    {
        private void Start()
        {
            foreach (Selectable selectable in Selectable.allSelectablesArray)
            {
                if (selectable is Button button)
                    button.onClick.AddListener(OnSelectable);
                else if (selectable is Toggle toggle)
                    toggle.onValueChanged.AddListener(_ => OnSelectable());
                else if (selectable is InputField inputField)
                    inputField.onSubmit.AddListener(_ => OnSelectable());
                else if (selectable is Slider slider)
                    slider.onValueChanged.AddListener(_ => OnSelectable());
            }
        }

        private static void OnSelectable()
        {
            ServiceProvider.GetService<AndroidVibrator>().Vibrate(5L);
        }
    }
}
