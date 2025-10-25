using Model.Tools.EventSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Input
{
    public class InputListener : MonoBehaviour
    {
        public void OnMove(InputValue input)
        {
            Vector2 inputVector = input.Get<Vector2>();
            
            EventSystem.Raise<MoveAction>(inputVector);
        }
        
        public void OnLook(InputValue input)
        {
            Vector2 inputVector = input.Get<Vector2>();
            
            EventSystem.Raise<LookAction>(inputVector);
        }
    }
}
