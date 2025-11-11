namespace Tools.Pool
{
    public interface IResetable
    {
        public void Restart();
        public void Assign(params object[] parameters);
    }
}