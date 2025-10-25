using System;
using Model.Tools.FSM;
using UnityEngine;

namespace Game.World.Character
{
    internal sealed class MoveState : State
    {
        private Vector3 rotation = Vector3.zero;
        private Vector3 velocity = Vector3.zero;

        public override Type[] OnTickParamTypes =>
            new[] { typeof(Vector2), typeof(Vector2), typeof(Transform), typeof(Transform), typeof(float), typeof(float), typeof(VelocityModule) };

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            Vector2 moveInput = (Vector2)parameters[0];
            Vector2 lookInput = (Vector2)parameters[1];
            Transform dronePivot = (Transform)parameters[2];
            Transform camPivot = (Transform)parameters[3];
            float rotSpeed = (float)parameters[4];
            float droneCatchupSpeed = (float)parameters[5];
            VelocityModule velocityModule = (VelocityModule)parameters[6];

            float dt = Time.deltaTime;
            Vector3 droneForward = dronePivot.forward;
            Vector3 droneRight = dronePivot.right;

            BehaviourActions actions = Pool.Get<BehaviourActions>();

            actions.AddMultiThreadableBehaviour(0, () => { CheckInput(moveInput, lookInput); });
            actions.AddMultiThreadableBehaviour(0, () => { CalculateRotation(lookInput, rotSpeed, dt); });
            actions.AddMultiThreadableBehaviour(0, () => { CalculateVelocity(velocityModule, moveInput, droneForward, droneRight, dt); });

            actions.AddMainThreadBehaviour(1, () => { Rotate(dronePivot, camPivot, droneCatchupSpeed); });
            actions.AddMainThreadBehaviour(1, () => { Move(dronePivot, camPivot); });
            
            return actions;
        }

        private void Move(Transform dronePivot, Transform camPivot)
        {
            dronePivot.position += velocity;
            camPivot.position = dronePivot.position;
        }

        private void CalculateVelocity(VelocityModule velocityModule, Vector2 moveInput, Vector3 droneForward, Vector3 droneRight, float dt)
        {
            velocityModule.Accelerate(new Vector3(moveInput.x, 0f, moveInput.y), dt);
            velocity = droneForward * velocityModule.GetVelocity().z + droneRight * velocityModule.GetVelocity().x;
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

        private void CalculateRotation(Vector2 lookInput, float rotSpeed, float dt)
        {
            lookInput = lookInput.normalized * rotSpeed * dt;

            rotation.x += lookInput.y;
            rotation.y += lookInput.x;
            rotation.z = 0;
        }
    }
}