using System.Text;
using Game.World.FabbriPlugin;
using NUnit.Framework;
using Tools.ServiceProvider;
using UnityEditor;
using UnityEngine;

namespace Editor.Diagnostics
{
    [TestFixture]
    public class DiagnosticLogServiceTests
    {
        private DiagnosticLogService _service;

        [SetUp]
        public void SetUp()
        {
            ServiceProvider.Clear(true);
            _service = new DiagnosticLogService();
            ServiceProvider.TryAddService<DiagnosticLogService>(_service);
            ServiceProvider.TryAddService<IDiagnosticLogService>(_service);
            _service.ClearLogs();
        }

        [TearDown]
        public void TearDown()
        {
            _service?.ClearLogs();
            ServiceProvider.Clear(true);
        }

        [Test]
        public void RecordFailure_WritesFormattedEntry_AndIncludesInReport()
        {
            _service.RecordFailure("ADS", "ShowRewarded", "NotReady", "cooldown=5s");

            string report = _service.BuildDiagnosticReport();
            Assert.That(report, Does.Contain("=== DRONERACER DIAGNOSTIC REPORT ==="));
            Assert.That(report, Does.Contain("--- SERVICES & SAVE STATE ---"));
            Assert.That(report, Does.Contain("[FAIL] [ADS] ShowRewarded: NotReady | cooldown=5s"));
        }

        [Test]
        public void RecordFailure_DeduplicatesIdenticalFailureWithinWindow()
        {
            _service.RecordFailure("GPGS", "SignIn", "Canceled");
            _service.RecordFailure("GPGS", "SignIn", "Canceled");

            string logs = ServiceProvider.GetService<FileManager>().ReadFile();
            int count = logs.Split(new[] { "[FAIL] [GPGS] SignIn: Canceled" }, System.StringSplitOptions.None).Length - 1;
            Assert.AreEqual(1, count);
        }

        [Test]
        public void RecordInfo_WritesInfoEntry()
        {
            _service.RecordInfo("IAP", "Initialized successfully");

            string logs = ServiceProvider.GetService<FileManager>().ReadFile();
            Assert.That(logs, Does.Contain("[INFO] [IAP] Initialized successfully"));
        }

        [Test]
        public void ClearLogs_RemovesStoredEntries()
        {
            _service.RecordFailure("CLOUD_SAVE", "Load", "Timeout");
            _service.ClearLogs();

            string report = _service.BuildDiagnosticReport();
            Assert.That(report, Does.Contain("(no log entries recorded)"));
        }

        [Test]
        public void FileManager_CapsLogSizeAt64KB()
        {
            var fm = ServiceProvider.GetService<FileManager>();
            string chunk = new string('A', 2048);
            for (int i = 0; i < 40; i++)
            {
                fm.WriteFile($"line-{i:D2}-{chunk}");
            }

            string content = fm.ReadFile();
            Assert.LessOrEqual(Encoding.UTF8.GetByteCount(content), 64 * 1024);
            Assert.That(content, Does.Contain("line-39-"));
        }

        [Test]
        public void Logger_LogAndGetLogs_RoutesThroughFileManager()
        {
            var logger = ServiceProvider.GetService<Game.World.FabbriPlugin.Logger>();
            logger.Log("HelloFromLogger");

            string logs = logger.GetLogs();
            Assert.That(logs, Does.Contain("[INFO] [LOGGER] HelloFromLogger"));
        }

        [MenuItem("Tools/Diagnostics/Run Milestone 1 Tests")]
        public static void RunAllFromMenu()
        {
            var tests = new DiagnosticLogServiceTests();
            int passed = 0;
            int failed = 0;

            void Run(string name, System.Action body)
            {
                try
                {
                    tests.SetUp();
                    body();
                    tests.TearDown();
                    passed++;
                    Debug.Log($"[PASS] {name}");
                }
                catch (System.Exception ex)
                {
                    failed++;
                    Debug.LogError($"[FAIL] {name}: {ex}");
                }
            }

            Run(nameof(RecordFailure_WritesFormattedEntry_AndIncludesInReport), tests.RecordFailure_WritesFormattedEntry_AndIncludesInReport);
            Run(nameof(RecordFailure_DeduplicatesIdenticalFailureWithinWindow), tests.RecordFailure_DeduplicatesIdenticalFailureWithinWindow);
            Run(nameof(RecordInfo_WritesInfoEntry), tests.RecordInfo_WritesInfoEntry);
            Run(nameof(ClearLogs_RemovesStoredEntries), tests.ClearLogs_RemovesStoredEntries);
            Run(nameof(FileManager_CapsLogSizeAt64KB), tests.FileManager_CapsLogSizeAt64KB);
            Run(nameof(Logger_LogAndGetLogs_RoutesThroughFileManager), tests.Logger_LogAndGetLogs_RoutesThroughFileManager);

            Debug.Log($"[DiagnosticLogServiceTests] Complete: {passed} passed, {failed} failed.");
        }
    }
}
