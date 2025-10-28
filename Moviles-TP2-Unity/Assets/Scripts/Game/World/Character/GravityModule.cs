using UnityEngine;

namespace Game.World.Character
{
    internal sealed class GravityModule
    {
        private readonly Transform dronePivot;
        private readonly float gravity;
        private readonly float droneForce;
        private readonly float maxGravityCombat;

        internal GravityModule(Transform dronePivot, float gravity, float droneForce, float maxGravityCombat)
        {
            this.dronePivot = dronePivot;
            this.gravity = gravity;
            this.droneForce = droneForce;
            this.maxGravityCombat = maxGravityCombat;
        }
        
        internal void Update()
        {
            float droneGravityCombat = Mathf.Clamp(1f / dronePivot.up.y, 0f, maxGravityCombat);
            
            dronePivot.position += dronePivot.up * (droneForce * Time.deltaTime * droneGravityCombat);
            dronePivot.position -= Vector3.up * (gravity * Time.deltaTime);
        }
    }
}