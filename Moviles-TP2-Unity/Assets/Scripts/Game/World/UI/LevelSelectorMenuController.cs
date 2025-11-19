using System.Collections.Generic;
using System.IO;
using Game.World.GameManager;
using TMPro;
using Tools.ServiceProvider;
using UnityEngine;
using UnityEngine.UI;

namespace Game.World.UI
{
    public class LevelSelectorMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject buttonTemplate;
        
        private readonly List<Button> buttons = new();
        
        private void Start()
        {
            for (int i = 0; i < ServiceProvider.GetService<LevelList>().GetLevels().Count; i++)
            {
                string levelName = Path.GetFileNameWithoutExtension(ServiceProvider.GetService<LevelList>().GetLevels()[i]);

                GameObject button = Instantiate(buttonTemplate, buttonTemplate.transform.parent);
                button.name = levelName + "Btn";

                button.GetComponentInChildren<TextMeshProUGUI>().text = levelName;
                Button btnComponent = button.GetComponent<Button>();
                
                buttons.Add(btnComponent);

                int levelIndex = i;
                btnComponent.onClick.AddListener(() => LoadLevel(levelIndex));
            }
            
            buttonTemplate.SetActive(false);
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            foreach (Button button in buttons)
                button.onClick.RemoveAllListeners();
        }

        private static void LoadLevel(int level)
        {
            GameSave.CurrentLevel = level;
            ServiceProvider.GetService<LevelList>().LoadCurrentLevel();
        }
    }
}