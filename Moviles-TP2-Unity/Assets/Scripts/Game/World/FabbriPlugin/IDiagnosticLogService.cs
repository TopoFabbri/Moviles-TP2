using System;
using Tools.ServiceProvider;

namespace Game.World.FabbriPlugin
{
    public interface IDiagnosticLogService : IService
    {
        event Action<string, string, string> OnFailureRecorded;
        void RecordFailure(string category, string action, string reason, string details = null);
        void RecordInfo(string category, string message);
        string BuildDiagnosticReport();
        void ShareReport(string subject = "DroneRacer Diagnostic Report");
        void ClearLogs();
    }
}
