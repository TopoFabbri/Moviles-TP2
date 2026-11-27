namespace Game.World.Graphics
{
    public interface IGraphicsStorage
    {
        GraphicsSettingsData Load();
        void Save(GraphicsSettingsData data);
        void Clear();
    }
}
