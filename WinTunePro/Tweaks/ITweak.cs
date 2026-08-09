using System.Threading.Tasks;

namespace WinTunePro.Tweaks
{
    public interface ITweak
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
        bool RequiresElevation { get; }
        Task<bool> ApplyAsync();
        Task<bool> RollbackAsync();
    }
}
