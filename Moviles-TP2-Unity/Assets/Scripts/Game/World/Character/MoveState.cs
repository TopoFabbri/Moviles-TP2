using System;
using Model.Tools.FSM;
using UnityEngine;

namespace Game.World.Character
{
    public class MoveState : State
    {
        private Vector3 rotation = Vector3.zero;
        
        public override Type[] OnTickParamTypes => new[] { typeof(Vector2), typeof(Vector2), typeof(Transform), typeof(Transform), typeof(float), typeof(float), typeof(float) };

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            Vector2 moveInput = (Vector2)parameters[0];
            Vector2 lookInput = (Vector2)parameters[1];
            Transform dronePivot = (Transform)parameters[2];
            Transform camPivot = (Transform)parameters[3];
            float moveSpeed = (float)parameters[4];
            float rotSpeed = (float)parameters[5];
            float droneCatchupSpeed = (float)parameters[6];

            BehaviourActions actions = Pool.Get<BehaviourActions>();

            Vector3 moveVector = new();

            actions.AddMultiThreadableBehaviour(0, () => { CheckInput(moveInput, lookInput); });
            actions.AddMainThreadBehaviour(0, () => { CalculateRotation(lookInput, rotSpeed); });
            actions.AddMainThreadBehaviour(0, () => { moveVector = CalculateMoveVector(dronePivot, moveInput); });

            actions.AddMainThreadBehaviour(1, () => { Move(dronePivot, camPivot, moveVector, moveSpeed); });
            actions.AddMainThreadBehaviour(1, () => { Rotate(dronePivot, camPivot, droneCatchupSpeed); });

            return actions;
        }

        private void CheckInput(Vector2 moveInput, Vector2 lookInput)
        {
            if (moveInput.magnitude < float.Epsilon && lookInput.magnitude < float.Epsilon)
                flag.Invoke(DroneController.Flags.Break);
        }

        private void Rotate(Transform dronePivot, Transform camPivot, float droneCatchupSpeed)
        {
            camPivot.rotation = Quaternion.Euler(rotation);
            dronePivot.rotation = Quaternion.Slerp(dronePivot.rotation, camPivot.rotation, droneCatchupSpeed * Time.deltaTime);
        }

        private void CalculateRotation(Vector2 lookInput, float rotSpeed)
        {
            lookInput = lookInput.normalized * rotSpeed * Time.deltaTime;
            
            rotation.x += lookInput.y;
            rotation.y += lookInput.x;
            rotation.z = 0;
        }

        private static void Move(Transform dronePivot, Transform camPivot, Vector3 moveVector, float moveSpeed)
        {
            dronePivot.Translate(moveVector * moveSpeed * Time.deltaTime, Space.World);
            camPivot.position = dronePivot.position;
        }

        private static Vector3 CalculateMoveVector(Transform dronePivot, Vector2 moveInput)
        {
            Vector3 moveVector = dronePivot.forward * moveInput.y + dronePivot.right * moveInput.x;
            return moveVector;
        }
    }
}