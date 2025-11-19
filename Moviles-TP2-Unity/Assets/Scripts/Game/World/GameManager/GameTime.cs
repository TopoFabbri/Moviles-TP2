using Tools.ServiceProvider;
using Tools.Time;

namespace Game.World.GameManager
{
    internal class GameTime : IService
    {
        internal Timer Timer { get; } = new();
    }
}