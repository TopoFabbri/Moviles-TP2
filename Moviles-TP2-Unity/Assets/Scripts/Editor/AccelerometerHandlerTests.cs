using Game.World.Character;
using NUnit.Framework;
using UnityEngine;

namespace Editor
{
    [TestFixture]
    public class AccelerometerHandlerTests
    {
        private const float MaxTilt = 45f;

        [Test]
        public void Attitude_NeutralPose_ReturnsZero()
        {
            Quaternion origin = Quaternion.Euler(35f, 10f, -5f);
            Vector2 tilt = AccelerometerHandler.ComputeTiltFromAttitude(origin, origin, MaxTilt);

            Assert.AreEqual(0f, tilt.x, 1e-4f);
            Assert.AreEqual(0f, tilt.y, 1e-4f);
        }

        [Test]
        public void Attitude_PitchAndRollWithinRange_AreProportional()
        {
            Quaternion origin = Quaternion.Euler(40f, 0f, 0f);

            // Pitch forward 22.5 deg (negative X rotation in device frame) -> +0.5 pitch
            Quaternion pitchedForward = origin * Quaternion.Euler(-22.5f, 0f, 0f);
            Vector2 pitchTilt = AccelerometerHandler.ComputeTiltFromAttitude(origin, pitchedForward, MaxTilt);
            Assert.AreEqual(0f, pitchTilt.x, 1e-3f);
            Assert.AreEqual(0.5f, pitchTilt.y, 1e-3f);

            // Roll right 22.5 deg (negative Z rotation in device frame) -> +0.5 roll
            Quaternion rolledRight = origin * Quaternion.Euler(0f, 0f, -22.5f);
            Vector2 rollTilt = AccelerometerHandler.ComputeTiltFromAttitude(origin, rolledRight, MaxTilt);
            Assert.AreEqual(0.5f, rollTilt.x, 1e-3f);
            Assert.AreEqual(0f, rollTilt.y, 1e-3f);
        }

        [Test]
        public void Attitude_Past90Degrees_ClampsWithoutBouncing()
        {
            Quaternion origin = Quaternion.identity;

            // 135 deg roll right -> must stay clamped at +1 with zero spurious pitch
            Quaternion rolledPast90 = origin * Quaternion.Euler(0f, 0f, -135f);
            Vector2 rollTilt = AccelerometerHandler.ComputeTiltFromAttitude(origin, rolledPast90, MaxTilt);
            Assert.AreEqual(1f, rollTilt.x, 1e-3f);
            Assert.AreEqual(0f, rollTilt.y, 1e-3f);

            // 135 deg roll left -> must stay clamped at -1 with zero spurious pitch
            Quaternion rolledLeftPast90 = origin * Quaternion.Euler(0f, 0f, 135f);
            Vector2 rollLeftTilt = AccelerometerHandler.ComputeTiltFromAttitude(origin, rolledLeftPast90, MaxTilt);
            Assert.AreEqual(-1f, rollLeftTilt.x, 1e-3f);
            Assert.AreEqual(0f, rollLeftTilt.y, 1e-3f);

            // 150 deg pitch forward -> must stay clamped at +1 without bouncing or spurious roll
            Quaternion pitchedForwardPast90 = origin * Quaternion.Euler(-150f, 0f, 0f);
            Vector2 pitchFwdTilt = AccelerometerHandler.ComputeTiltFromAttitude(origin, pitchedForwardPast90, MaxTilt);
            Assert.AreEqual(0f, pitchFwdTilt.x, 1e-3f);
            Assert.AreEqual(1f, pitchFwdTilt.y, 1e-3f);

            // 150 deg pitch backward -> must stay clamped at -1 without bouncing or spurious roll
            Quaternion pitchedBackPast90 = origin * Quaternion.Euler(150f, 0f, 0f);
            Vector2 pitchBackTilt = AccelerometerHandler.ComputeTiltFromAttitude(origin, pitchedBackPast90, MaxTilt);
            Assert.AreEqual(0f, pitchBackTilt.x, 1e-3f);
            Assert.AreEqual(-1f, pitchBackTilt.y, 1e-3f);
        }

        [Test]
        public void Attitude_ZeroOrInvalidQuaternion_ReturnsZeroWithoutNaN()
        {
            Quaternion zero = new(0f, 0f, 0f, 0f);
            Vector2 tilt1 = AccelerometerHandler.ComputeTiltFromAttitude(zero, Quaternion.identity, MaxTilt);
            Vector2 tilt2 = AccelerometerHandler.ComputeTiltFromAttitude(Quaternion.identity, zero, MaxTilt);

            Assert.AreEqual(Vector2.zero, tilt1);
            Assert.AreEqual(Vector2.zero, tilt2);
            Assert.IsFalse(float.IsNaN(tilt1.x) || float.IsNaN(tilt1.y));
        }

        [Test]
        public void Gravity_NeutralPose_ReturnsZero()
        {
            Vector3 origin = new Vector3(0f, -0.7f, -0.7f).normalized;
            Vector2 tilt = AccelerometerHandler.ComputeTiltFromGravity(origin, origin, MaxTilt);

            Assert.AreEqual(0f, tilt.x, 1e-4f);
            Assert.AreEqual(0f, tilt.y, 1e-4f);
        }

        [Test]
        public void Gravity_PitchPast90Degrees_ClampsWithoutBouncing()
        {
            // Upright neutral: gravity along -Y
            Vector3 origin = new(0f, -1f, 0f);

            // Tilted forward 45 deg: gravity along (0, -0.707, -0.707) -> +1.0 pitch
            Vector3 forward45 = new Vector3(0f, -1f, -1f).normalized;
            Vector2 tilt45 = AccelerometerHandler.ComputeTiltFromGravity(origin, forward45, MaxTilt);
            Assert.AreEqual(1f, tilt45.y, 1e-3f);

            // Tilted forward 135 deg (past flat!): gravity along (0, +0.707, -0.707) -> must STILL be +1.0 with zero spurious roll!
            Vector3 forward135 = new Vector3(0f, 1f, -1f).normalized;
            Vector2 tilt135 = AccelerometerHandler.ComputeTiltFromGravity(origin, forward135, MaxTilt);
            Assert.AreEqual(0f, tilt135.x, 1e-3f);
            Assert.AreEqual(1f, tilt135.y, 1e-3f);
        }

        [Test]
        public void Gravity_RollPast90Degrees_ClampsWithoutBouncing()
        {
            Vector3 origin = new(0f, -1f, 0f);

            // Tilted right 135 deg (upside-down right): gravity along (+0.707, +0.707, 0) -> must STILL be +1.0 with zero spurious pitch!
            Vector3 right135 = new Vector3(1f, 1f, 0f).normalized;
            Vector2 tilt = AccelerometerHandler.ComputeTiltFromGravity(origin, right135, MaxTilt);
            Assert.AreEqual(1f, tilt.x, 1e-3f);
            Assert.AreEqual(0f, tilt.y, 1e-3f);
        }

        [Test]
        public void Gravity_ZeroVector_ReturnsZeroWithoutNaN()
        {
            Vector2 tilt1 = AccelerometerHandler.ComputeTiltFromGravity(Vector3.zero, Vector3.down, MaxTilt);
            Vector2 tilt2 = AccelerometerHandler.ComputeTiltFromGravity(Vector3.down, Vector3.zero, MaxTilt);

            Assert.AreEqual(Vector2.zero, tilt1);
            Assert.AreEqual(Vector2.zero, tilt2);
        }

        [Test]
        public void ApplyDeadZone_HandlesBoundaryAndRescaling()
        {
            Assert.AreEqual(0f, AccelerometerHandler.ApplyDeadZone(0.05f, 0.1f), 1e-5f);
            Assert.AreEqual(0f, AccelerometerHandler.ApplyDeadZone(-0.1f, 0.1f), 1e-5f);
            Assert.AreEqual(0.5f, AccelerometerHandler.ApplyDeadZone(0.55f, 0.1f), 1e-5f);
            Assert.AreEqual(-1f, AccelerometerHandler.ApplyDeadZone(-1f, 0.1f), 1e-5f);
            Assert.AreEqual(1f, AccelerometerHandler.ApplyDeadZone(1f, 0.1f), 1e-5f);
        }
    }
}
