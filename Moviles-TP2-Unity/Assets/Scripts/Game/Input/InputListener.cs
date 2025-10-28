using Tools.EventSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Input
{
    public class InputListener : MonoBehaviour
    {
        public void OnMove(InputValue input)
        {
            Vector3 inputVector = input.Get<Vector3>();
            
            EventSystem.Raise<MoveAction>(inputVector);
        }
        
        public void OnLook(InputValue input)
        {
            Vector3 inputVector = input.Get<Vector3>();
            print(inputVector);
            
            EventSystem.Raise<LookAction>(inputVector);
        }
    }
}
