using System.Threading.Tasks;

namespace WinTunePro.Tweaks
{
    public abstract class RegistryTweak : ITweak
    {
        public abstract string Id { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract bool RequiresElevation { get; }

        public abstract Task<bool> ApplyAsync();
        public abstract Task<bool> RollbackAsync();
    }
}
