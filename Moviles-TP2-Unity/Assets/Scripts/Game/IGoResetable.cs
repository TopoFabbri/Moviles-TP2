using Tools.Pool;

namespace Game
{
    public interface IGoResetable : IResetable
    {
        public int PrefabId { get; set; }
    }
}