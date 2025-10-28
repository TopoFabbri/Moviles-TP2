using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.World
{
    public class GameManager : MonoBehaviour
    {
        public InputActionAsset actions;
        
        private void Awake()
        {
            Application.targetFrameRate = 60;
        }

        private void Update()
        {
            if (Accelerometer.current != null && !Accelerometer.current.enabled)
                InputSystem.EnableDevice(Accelerometer.current);
        }
    }
}
