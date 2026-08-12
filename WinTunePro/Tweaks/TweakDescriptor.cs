namespace WinTunePro.Tweaks
{
    public class TweakDescriptor
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool RequiresElevation { get; set; }
    }
}
