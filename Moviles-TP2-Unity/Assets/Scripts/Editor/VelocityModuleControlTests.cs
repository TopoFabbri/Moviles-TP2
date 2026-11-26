using Game.World.Character.DroneModules;
using Game.World.GameManager;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Editor
{
    public class VelocityModuleControlTests
    {
        private string previousPlayerId;

        [SetUp]
        public void SetUp()
        {
            previousPlayerId = GameSave.ActivePlayerId;
            GameSave.SetActiveAccount("control_test_account");
            GameSave.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            GameSave.Reset();
            GameSave.SetActiveAccount(previousPlayerId);
        }

        [Test]
        public void Accelerate_ZeroControl_UsesBaseAcceleration()
        {
            GameSave.AddedControl = 0f;
            var module = new VelocityModule(acceleration: 10f, maxSpeed: 20f);

            module.Accelerate(Vector3.forward, 0.5f);

            Assert.AreEqual(5f, module.Velocity.z, 0.001f);
        }

        [Test]
        public void Accelerate_HalfControl_DoublesAccelerationAndDeceleration()
        {
            GameSave.AddedControl = 0.5f;
            var module = new VelocityModule(acceleration: 10f, maxSpeed: 20f);

            module.Accelerate(Vector3.forward, 0.5f);
            Assert.AreEqual(10f, module.Velocity.z, 0.001f);

            module.Accelerate(Vector3.zero, 0.25f);
            Assert.AreEqual(5f, module.Velocity.z, 0.001f);
        }

        [Test]
        public void Accelerate_MaxControl_IsInstantForAccelerationAndDeceleration()
        {
            GameSave.AddedControl = 1f;
            var module = new VelocityModule(acceleration: 10f, maxSpeed: 20f);

            module.Accelerate(Vector3.forward, 0.016f);
            Assert.AreEqual(20f, module.Velocity.z, 0.001f);

            module.Accelerate(Vector3.zero, 0.016f);
            Assert.AreEqual(0f, module.Velocity.z, 0.001f);
        }
    }
}
