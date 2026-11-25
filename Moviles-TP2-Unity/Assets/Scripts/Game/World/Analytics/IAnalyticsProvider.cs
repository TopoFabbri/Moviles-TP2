using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game.World.Analytics
{
    public interface IAnalyticsProvider
    {
        string Name { get; }
        bool IsInitialized { get; }

        Task InitializeAsync();
        void SetConsent(bool granted);
        void RecordEvent(string eventName, IDictionary<string, object> parameters, TimeSpan? duration = null, bool immediate = false);
        void Flush();
    }
}
