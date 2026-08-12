using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinTunePro.Utils;

namespace WinTunePro.Tweaks
{
    public class TweakManager
    {
        private readonly ConcurrentDictionary<string, ITweak> _tweaks = new();

        public void Register(ITweak tweak)
        {
            if (tweak == null) return;
            _tweaks[tweak.Id] = tweak;
            Logger.LogInfo($"Tweak registered: {tweak.Id}");
        }

        public IEnumerable<TweakDescriptor> GetAll()
        {
            return _tweaks.Values.Select(t => new TweakDescriptor
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                RequiresElevation = t.RequiresElevation
            });
        }

        public ITweak? Get(string id)
        {
            if (id == null) return null;
            _tweaks.TryGetValue(id, out var t);
            return t;
        }

        public async Task<bool> ApplyByIdAsync(string id)
        {
            var t = Get(id);
            if (t == null) return false;
            Logger.LogInfo($"Applying tweak via manager: {id}");
            return await t.ApplyAsync();
        }

        public async Task<bool> RollbackByIdAsync(string id)
        {
            var t = Get(id);
            if (t == null) return false;
            Logger.LogInfo($"Rolling back tweak via manager: {id}");
            return await t.RollbackAsync();
        }
    }
}
