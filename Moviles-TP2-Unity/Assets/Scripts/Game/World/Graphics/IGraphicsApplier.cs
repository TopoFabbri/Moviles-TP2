namespace Game.World.Graphics
{
    public interface IGraphicsApplier
    {
        void ApplyAll(GraphicsSettingsData data);
        void ApplySceneObjects(GraphicsSettingsData data);
        void RestoreDefaults();
    }
}
