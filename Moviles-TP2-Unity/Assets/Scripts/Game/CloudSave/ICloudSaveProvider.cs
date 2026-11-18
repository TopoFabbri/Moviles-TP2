using System;

namespace Game.CloudSave
{
    public interface ICloudSaveProvider
    {
        bool IsAvailable { get; }
        void LoadCloudSave(Action<bool, CloudSaveData> onComplete);
        void SaveToCloud(CloudSaveData data, Action<bool> onComplete);
    }
}
