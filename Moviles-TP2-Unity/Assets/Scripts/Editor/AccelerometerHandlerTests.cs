using System.Reflection;
using Game.World.Character;
using Game.World.Character.DroneModules;
using Game.World.GameManager;
using Game.World.UI;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

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

        [Test]
        public void SwingTwist_PureYaw_StripsToIdentity()
        {
            Quaternion pureYaw = Quaternion.Euler(0f, 75f, 0f);
            Quaternion swing = AccelerometerHandler.StripYawSwingTwist(pureYaw);

            Assert.AreEqual(0f, Quaternion.Angle(Quaternion.identity, swing), 1e-3f);
        }

        [Test]
        public void SwingTwist_YawThenPitchOrRoll_PreservesTiltAndStripsYaw()
        {
            Quaternion pitch = Quaternion.Euler(-30f, 0f, 0f);
            Quaternion yawThenPitch = Quaternion.Euler(0f, 110f, 0f) * pitch;
            Quaternion swingPitch = AccelerometerHandler.StripYawSwingTwist(yawThenPitch);
            Assert.AreEqual(0f, Quaternion.Angle(pitch, swingPitch), 1e-2f);
            Assert.AreEqual(0f, swingPitch.y, 1e-4f);

            Quaternion roll = Quaternion.Euler(0f, 0f, -40f);
            Quaternion yawThenRoll = Quaternion.Euler(0f, -95f, 0f) * roll;
            Quaternion swingRoll = AccelerometerHandler.StripYawSwingTwist(yawThenRoll);
            Assert.AreEqual(0f, Quaternion.Angle(roll, swingRoll), 1e-2f);
            Assert.AreEqual(0f, swingRoll.y, 1e-4f);
        }

        [Test]
        public void SwingTwist_InvalidQuaternion_ReturnsIdentity()
        {
            Quaternion zero = new(0f, 0f, 0f, 0f);
            Assert.AreEqual(Quaternion.identity, AccelerometerHandler.StripYawSwingTwist(zero));
            Assert.AreEqual(Quaternion.identity, AccelerometerHandler.ComputeSwingFromAttitude(zero, Quaternion.identity));
            Assert.AreEqual(Quaternion.identity, AccelerometerHandler.ComputeSwingFromGravity(Vector3.zero, Vector3.down));
        }

        [Test]
        public void ApplySwingDeadZoneAndInvert_RespectsDeadZoneAndAxes()
        {
            Quaternion smallTilt = Quaternion.Euler(-2f, 0f, 0f);
            Quaternion insideDeadZone = AccelerometerHandler.ApplySwingDeadZoneAndInvert(smallTilt, 3f, false, true);
            Assert.AreEqual(0f, Quaternion.Angle(Quaternion.identity, insideDeadZone), 1e-4f);

            Quaternion rollRight = Quaternion.Euler(0f, 0f, -30f);
            Quaternion normalRoll = AccelerometerHandler.ApplySwingDeadZoneAndInvert(rollRight, 0f, false, true);
            Quaternion invertedRoll = AccelerometerHandler.ApplySwingDeadZoneAndInvert(rollRight, 0f, true, true);
            Assert.Less(normalRoll.z, 0f);
            Assert.Greater(invertedRoll.z, 0f);

            Quaternion pitchFwd = Quaternion.Euler(-30f, 0f, 0f);
            Quaternion defaultPitch = AccelerometerHandler.ApplySwingDeadZoneAndInvert(pitchFwd, 0f, false, true);
            Quaternion nonInvertedPitch = AccelerometerHandler.ApplySwingDeadZoneAndInvert(pitchFwd, 0f, false, false);
            Assert.Less(defaultPitch.x, 0f);
            Assert.Greater(nonInvertedPitch.x, 0f);
        }

        [Test]
        public void ComputeBankYawRate_CouplesRollAndPitch()
        {
            Assert.AreEqual(0f, AccelerometerHandler.ComputeBankYawRate(Quaternion.identity), 1e-5f);

            // Banking right (negative Z rotation) -> positive yaw rate
            Quaternion rightBank = Quaternion.Euler(0f, 0f, -30f);
            float rightRate = AccelerometerHandler.ComputeBankYawRate(rightBank);
            Assert.Greater(rightRate, 0.45f);

            // Banking left (positive Z rotation) -> negative yaw rate
            Quaternion leftBank = Quaternion.Euler(0f, 0f, 30f);
            float leftRate = AccelerometerHandler.ComputeBankYawRate(leftBank);
            Assert.Less(leftRate, -0.45f);

            // Pitching while banked amplifies yaw rate
            Quaternion bankedAndPitched = Quaternion.Euler(-30f, 0f, -30f);
            float coupledRate = AccelerometerHandler.ComputeBankYawRate(bankedAndPitched);
            Assert.Greater(coupledRate, rightRate);
        }

        [Test]
        public void StepBankToTurn_AccumulatesContinuousYawAndPreservesTilt()
        {
            float yaw = 0f;
            Quaternion rightBank = Quaternion.Euler(0f, 0f, -45f);

            Quaternion rot = Quaternion.identity;
            for (int i = 0; i < 10; i++)
                rot = RotationModule.StepBankToTurn(ref yaw, rightBank, 0f, 100f, 1f, 0.1f);

            Assert.Greater(yaw, 60f);
            Quaternion expected = Quaternion.Euler(0f, yaw, 0f) * rightBank;
            Assert.AreEqual(0f, Quaternion.Angle(expected, rot), 1e-3f);
        }

        [Test]
        public void StepFpvAcro_RotatesAroundLocalAxesAndHoldsAttitudeAtNeutral()
        {
            Quaternion start = Quaternion.identity;

            // Pitch 90 deg around local X
            Quaternion pitched = RotationModule.StepFpvAcro(start, new Vector3(0f, 1f, 0f), 90f, 1f);
            Assert.AreEqual(0f, Quaternion.Angle(Quaternion.Euler(90f, 0f, 0f), pitched), 1e-2f);

            // Neutral input holds current attitude
            Quaternion held = RotationModule.StepFpvAcro(pitched, Vector3.zero, 90f, 1f);
            Assert.AreEqual(0f, Quaternion.Angle(pitched, held), 1e-4f);

            // Full 360-degree roll loop returns to start without gimbal lock
            Quaternion looped = start;
            for (int i = 0; i < 36; i++)
                looped = RotationModule.StepFpvAcro(looped, new Vector3(0f, 0f, -1f), 100f, 0.1f);
            Assert.AreEqual(0f, Quaternion.Angle(start, looped), 1e-1f);
        }

        [Test]
        public void RotationModule_MidFlightModeSwitch_PreservesOrientationWithoutSnapping()
        {
            GameObject droneGo = new("TestDronePivot");
            GameObject camGo = new("TestCamPivot");
            try
            {
                RotationModule module = new(droneGo.transform, camGo.transform, 100f, 10f);

                // Start in BankToTurn, set a non-zero yaw via localRotation, then switch to FpvAcro
                module.RotateBankToTurn(Quaternion.identity, 0f, 1f);
                droneGo.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

                // Switching to FpvAcro with zero input must hold ~90 deg yaw instead of snapping to identity
                module.RotateFpvAcro(Vector3.zero);
                Assert.AreEqual(0f, Quaternion.Angle(Quaternion.Euler(0f, 90f, 0f), droneGo.transform.localRotation), 1e-2f);

                // Rotate to 180 deg yaw in FpvAcro, then switch back to BankToTurn with neutral tilt
                droneGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                module.RotateBankToTurn(Quaternion.identity, 0f, 1f);
                Assert.AreEqual(0f, Quaternion.Angle(Quaternion.Euler(0f, 180f, 0f), droneGo.transform.localRotation), 1e-2f);
            }
            finally
            {
                Object.DestroyImmediate(droneGo);
                Object.DestroyImmediate(camGo);
            }
        }

        [Test]
        public void GameplayMenuController_EnsureGyroModeControl_DoesNotDuplicateContentHierarchy()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/GameplaySubmenu.prefab");
            Assert.IsNotNull(prefab, "GameplaySubmenu.prefab must exist");

            GameObject instance = Object.Instantiate(prefab);
            try
            {
                GameplayMenuController controller = instance.GetComponent<GameplayMenuController>();
                Assert.IsNotNull(controller);

                ScrollRect scrollRect = instance.GetComponentInChildren<ScrollRect>();
                Assert.IsNotNull(scrollRect);
                RectTransform content = scrollRect.content;
                Assert.IsNotNull(content);

                int initialChildCount = content.childCount;
                Assert.AreEqual(9, initialChildCount);

                MethodInfo startMethod = typeof(GameplayMenuController).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(startMethod);

                // Invoke Start() to populate GyroMode controls
                startMethod.Invoke(controller, null);

                // Should only add GyroModeSpace, GyroModeTxt, and GyroModeBtn inside Content (not clone Content onto Viewport)
                Assert.AreEqual(1, scrollRect.viewport.childCount, "Viewport must only contain one Content child, not a duplicated Content menu");
                Assert.AreEqual(initialChildCount + 3, content.childCount);
                Assert.AreEqual("ControlSchemeTxt", content.GetChild(0).name);
                Assert.AreEqual("ControlSchemeBtn", content.GetChild(1).name);
                Assert.AreEqual("GyroModeSpace", content.GetChild(2).name);
                Assert.AreEqual("GyroModeTxt", content.GetChild(3).name);
                Assert.AreEqual("GyroModeBtn", content.GetChild(4).name);

                TextMeshProUGUI gyroLabel = content.GetChild(3).GetComponent<TextMeshProUGUI>();
                Assert.IsNotNull(gyroLabel);
                Assert.AreEqual("Gyro Mode:", gyroLabel.text);

                ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
                Assert.IsNull(fitter, "Content must not have ContentSizeFitter without LayoutElements, as it shrinks Content height on layout rebuilds");
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                Assert.GreaterOrEqual(content.sizeDelta.y, 1100f, "Content height must expand to fit the added GyroMode controls without clipping and survive layout rebuilds");

                // Verify children do not overlap vertically after layout rebuild
                for (int i = 1; i < content.childCount; i++)
                {
                    RectTransform prev = content.GetChild(i - 1) as RectTransform;
                    RectTransform curr = content.GetChild(i) as RectTransform;
                    Assert.IsNotNull(prev);
                    Assert.IsNotNull(curr);
                    float prevBottom = prev.anchoredPosition.y - (prev.sizeDelta.y * prev.pivot.y);
                    float currTop = curr.anchoredPosition.y + (curr.sizeDelta.y * (1f - curr.pivot.y));
                    Assert.LessOrEqual(currTop, prevBottom + 1e-2f, $"Child {curr.name} (top={currTop}) must not vertically overlap {prev.name} (bottom={prevBottom})");
                }

                // Calling Start() again (or EnsureGyroModeControl when gyroModeText is cleared) must be idempotent
                startMethod.Invoke(controller, null);
                Assert.AreEqual(initialChildCount + 3, content.childCount);

                // Toggling GameObject active state (re-opening settings menu) refreshes UI from GameSave via OnEnable
                MethodInfo onEnableMethod = typeof(GameplayMenuController).GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(onEnableMethod);

                GyroMode beforeMode = GameSave.GyroMode;
                GameSave.GyroMode = beforeMode == GyroMode.BankToTurn ? GyroMode.FpvAcro : GyroMode.BankToTurn;
                onEnableMethod.Invoke(controller, null);

                UnityEngine.UI.Button gyroBtn = content.GetChild(4).GetComponent<UnityEngine.UI.Button>();
                Assert.IsNotNull(gyroBtn);
                TextMeshProUGUI gyroBtnText = gyroBtn.GetComponentInChildren<TextMeshProUGUI>();
                Assert.IsNotNull(gyroBtnText);
                string expectedAfterReopen = GameSave.GyroMode == GyroMode.BankToTurn ? "Bank-to-Turn" : "FPV Acro";
                Assert.AreEqual(expectedAfterReopen, gyroBtnText.text);

                // Clicking GyroModeBtn toggles GameSave.GyroMode and updates button text
                GyroMode modeBeforeClick = GameSave.GyroMode;
                gyroBtn.onClick.Invoke();
                Assert.AreNotEqual(modeBeforeClick, GameSave.GyroMode);

                string expectedText = GameSave.GyroMode == GyroMode.BankToTurn ? "Bank-to-Turn" : "FPV Acro";
                Assert.AreEqual(expectedText, gyroBtnText.text);

                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                Assert.GreaterOrEqual(content.sizeDelta.y, 1100f, "Content height must remain >= 1100f after clicking GyroModeBtn and rebuilding layout");

                // Restore original mode
                GameSave.GyroMode = beforeMode;
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }
    }
}
