namespace ODEngine.Core
{
    public interface IBaseSettings
    {
        public bool Fullscreen { get; set; }
        public int TextureSizeDiv { get; set; }
        public void Save();
        public void Load();
    }
}
