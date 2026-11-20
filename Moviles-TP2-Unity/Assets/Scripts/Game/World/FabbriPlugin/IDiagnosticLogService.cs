using Tools.ServiceProvider;

namespace Game.World.FabbriPlugin
{
    public interface IDiagnosticLogService : IService
    {
        void RecordFailure(string category, string action, string reason, string details = null);
        void RecordInfo(string category, string message);
        string BuildDiagnosticReport();
        void ClearLogs();
    }
}
